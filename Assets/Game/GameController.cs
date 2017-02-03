using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Random = UnityEngine.Random;

using SciFi.Players;
using SciFi.Players.Modifiers;
using SciFi.Items;
using SciFi.UI;
using SciFi.Environment.Effects;

namespace SciFi {
    public delegate void DamageChangedHandler(int playerId, int newDamage);
    public delegate void LifeChangedHandler(int playerId, int newLives);
    public delegate void StartGameHandler();
    public delegate void PlayersInitializedHandler(Player[] players);

    /// Commonly used layers. This is initialized by the <see cref="GameController" />
    /// so if you need a layer before it is created, use <c>LayerMask.NameToLayer(string)</c>.
    public static class Layers {
        public static int projectiles;
        public static int items;
        public static int players;
        public static int displayOnly;
        public static int heldAttacks;
        public static int touchControls;
        public static int noncollidingItems;
        public static int shield;
        public static int projectileInteractables;

        /// Initialize the layer IDs. To be called by <see cref="GameController" />.
        public static void Init() {
            projectiles = LayerMask.NameToLayer("Projectiles");
            items = LayerMask.NameToLayer("Items");
            players = LayerMask.NameToLayer("Players");
            displayOnly = LayerMask.NameToLayer("Display Only");
            heldAttacks = LayerMask.NameToLayer("Held Attacks");
            touchControls = LayerMask.NameToLayer("Touch Controls");
            noncollidingItems = LayerMask.NameToLayer("Noncolliding Items");
            shield = LayerMask.NameToLayer("Shield");
            projectileInteractables = LayerMask.NameToLayer("Projectile Interactables");
        }
    }

    /// How often will items appear on the screen?
    public enum ItemFrequency {
        None,
        VeryLow,
        Low,
        Normal,
        High,
        VeryHigh,
    }

    /// Handles "bookkeeping" for the game - which players are active,
    /// when is the game over, who won, when should an item spawn, etc.
    public class GameController : NetworkBehaviour {
        /// Is the game currently active?
        private bool isPlaying;

        /// Set on scene change to the countdown in the main game scene.
        private Countdown countdown;

        // Items
        public ItemFrequency itemFrequency;
        /// A list set in the Unity editor of all the items that can
        /// be spawned during the game.
        public List<GameObject> items;
        /// When will the next item appear?
        float nextItemTime;

        /// Active players, even if dead. Null if no game is running,
        /// guaranteed not null if a game is running.
        Player[] activePlayers;
        /// GameObjects for active players.
        GameObject[] activePlayersGo;
        /// Each player's nickname, only valid before the game starts.
        /// To access these during the game, use <see cref="SciFi.Players.Player.eDisplayName" />.
        string[] displayNames;
        /// Is this client the winner? This is always false if the game
        /// is not over yet.
        bool cIsWinner;
        /// The ID of the player controlled by this client.
        /// <seealso cref="SciFi.Players.Player.eId" />
        int cPlayerId;

        /// Event emitted when a player's damage changes.
        [SyncEvent]
        public event DamageChangedHandler EventDamageChanged;
        /// Event emitted when a player's lives change.
        /// Implies damage is set to 0.
        [SyncEvent]
        public event LifeChangedHandler EventLifeChanged;

        private event StartGameHandler _GameStarted;
        /// Event emitted when the game starts. If the game is already
        /// in progress when you subscribe to this event, the callback
        /// will be called immediately.
        public event StartGameHandler GameStarted {
            add {
                _GameStarted += value;
                if (isPlaying) {
                    value();
                }
            }
            remove {
                _GameStarted -= value;
            }
        }

        private event PlayersInitializedHandler _PlayersInitialized;
        /// Event emitted when players are initialized. If they are already
        /// initialized when you subscribe to this event, the callback will
        /// be called immediately.
        public event PlayersInitializedHandler PlayersInitialized {
            add {
                _PlayersInitialized += value;
                if (activePlayers != null) {
                    value(activePlayers);
                }
            }
            remove {
                _PlayersInitialized -= value;
            }
        }

        /// One GameController to rule them all, one GameController to find them...
        public static GameController Instance { get; private set; }

        /// Add a new player to the game, only before the game has started.
        /// <param name="playerObject">A pre-spawned player object.</param>
        /// <param name="displayName">May be null to get the default display name ("P1", etc.).</param>
        [Server]
        public void RegisterNewPlayer(GameObject playerObject, string displayName) {
            activePlayersGo = activePlayersGo.Concat(new[] { playerObject }).ToArray();
            displayNames = displayNames.Concat(new[] { displayName }).ToArray();
        }

        /// Start the game.
        /// <param name="countdown">Show the countdown and delay
        ///   the game from starting until it is done.</param>
        [Server]
        public void StartGame(bool countdown = true) {
            activePlayers = activePlayersGo.Select(p => p.GetComponent<Player>()).ToArray();

            for (var i = 0; i < activePlayersGo.Length; i++) {
                var player = activePlayersGo[i].GetComponent<Player>();
                player.eId = i;
                if (string.IsNullOrEmpty(displayNames[i])) {
                    player.eDisplayName = "P" + (i + 1);
                } else {
                    player.eDisplayName = displayNames[i];
                }
                player.eLives = 5;
                if (countdown) {
                    player.AddModifier(Modifier.CantMove);
                    player.AddModifier(Modifier.CantAttack);
                }
                EventLifeChanged(player.eId, player.eLives);
            }

            _PlayersInitialized(activePlayers);

            displayNames = null;

            RpcCreateCharacterList(activePlayers.Select(p => p.netId).ToArray());

            this.countdown = GameObject.Find("Canvas").GetComponent<Countdown>();
            RpcStartGame(countdown);
            if (countdown) {
                this.countdown.StartGame();
                this.countdown.OnFinished += _ => {
                    this.isPlaying = true;
                    foreach (var p in activePlayers) {
                        p.RemoveModifier(Modifier.CantMove);
                        p.RemoveModifier(Modifier.CantAttack);
                    }
                    if (_GameStarted != null) {
                        _GameStarted();
                    }
                };
            } else {
                this.isPlaying = true;
                if (_GameStarted != null) {
                    _GameStarted();
                }
            }
        }

        /// Initialize the list of players on the client.
        [ClientRpc]
        void RpcCreateCharacterList(NetworkInstanceId[] ids) {
            activePlayersGo = ids.Select(id => ClientScene.FindLocalObject(id)).ToArray();
            activePlayers = activePlayersGo.Select(p => p.GetComponent<Player>()).ToArray();
            cPlayerId = activePlayers.First(p => p.hasAuthority).eId;
            if (_PlayersInitialized != null) {
                _PlayersInitialized(activePlayers);
            }
        }

        /// Find the active player with ID <c>id</c>.
        public Player GetPlayer(int id) {
            if (id >= activePlayers.Length) {
                return null;
            }

            return activePlayers[id];
        }

        /// Find the winner and report it to the clients.
        int FindWinner() {
            Player winner;
            try {
                winner = activePlayers.Single(p => p.eLives != 0);
            } catch {
                return -1;
            }
            return winner.eId;
        }

        /// Is this client the winner? Always false
        /// if the game is still in progress.
        [Client]
        public bool IsWinner() {
            return cIsWinner;
        }

        /// End the game and load the game over scene.
        [Server]
        public void EndGame(int winnerId) {
            StartCoroutine(TransitionToGameOver());
            RpcEndGame(winnerId);
        }

        /// Is the game currently in progress?
        public bool IsPlaying() {
            return isPlaying;
        }

        /// Start the game on the client.
        [ClientRpc]
        void RpcStartGame(bool countdown) {
            cIsWinner = false;
            if (!countdown) {
                this.isPlaying = true;
                if (_GameStarted != null) {
                    _GameStarted();
                }
                return;
            }
            this.countdown = GameObject.Find("Canvas").GetComponent<Countdown>();
            this.countdown.StartGame();
            this.countdown.OnFinished += _ => {
                this.isPlaying = true;
                if (_GameStarted != null) {
                    _GameStarted();
                }
            };
        }

        /// End the game on the client and load the game over scene.
        [ClientRpc]
        void RpcEndGame(int winnerId) {
            if (winnerId == cPlayerId) {
                cIsWinner = true;
            }
            StartCoroutine(TransitionToGameOver());
        }

        IEnumerator TransitionToGameOver() {
            var music = GameObject.Find("Music").GetComponent<AudioSource>();
            isPlaying = false;
            Effects.FadeOut();
            Effects.FadeOutAudio(music, .9f, 20);
            yield return new WaitForSeconds(1f);
            activePlayersGo = new GameObject[0];
            activePlayers = new Player[0];
            SceneManager.LoadScene("GameOver");
        }

        /// Convert a prefab object to its index in the spawnable prefabs list.
        public static int PrefabToIndex(GameObject prefab) {
            return NetworkManager.singleton.spawnPrefabs.IndexOf(prefab);
        }

        /// Convert an index in the spawnable prefabs list to a prefab object.
        public static GameObject IndexToPrefab(int index) {
            return NetworkManager.singleton.spawnPrefabs[index];
        }

        /// Deduct a life from the player, respawn, and
        /// check if the game is over.
        [Command]
        public void CmdDie(GameObject playerObject) {
            var player = playerObject.GetComponent<Player>();
            --player.eLives;
            if (activePlayers.Count(p => p.eLives != 0) == 1) {
                int winnerId = FindWinner();

                foreach (var go in activePlayersGo) {
                    Destroy(go);
                }

                EndGame(winnerId);
                return;
            }

            player.eDamage = 0;
            player.RpcRespawn(new Vector3(0f, 7f));
            EventLifeChanged(player.eId, player.eLives);
        }

        /// Inflict damage on a player or item.
        [Server]
        public void TakeDamage(GameObject obj, int amount) {
            var player = obj.GetComponent<Player>();
            if (player == null) {
                var item = obj.GetComponent<Item>();
                if (item != null) {
                    ItemTakeDamage(item, amount);
                }
            } else {
                PlayerTakeDamage(player, amount);
            }
        }

        /// Inflict damage on a player.
        [Server]
        void PlayerTakeDamage(Player player, int amount) {
            if (player.IsModifierEnabled(Modifier.Invincible)) {
                return;
            }
            player.eDamage += amount;
            EventDamageChanged(player.eId, player.eDamage);
        }

        /// Inflict damage on an item.
        [Server]
        void ItemTakeDamage(Item item, int amount) {
            item.TakeDamage(amount);
        }

        /// Inflict knockback on a player.
        [Server]
        public void Knockback(GameObject attackingObject, GameObject playerObject, float amount) {
            var player = playerObject.GetComponent<Player>();
            if (player == null) {
                return;
            }
            if (player.IsModifierEnabled(Modifier.Invincible)) {
                return;
            }
            amount *= 50 * player.eDamage;
            Vector3 vector;
            var projectile = attackingObject.GetComponent<Projectile>();
            var initialForceX = 0f;
            if (projectile != null) {
                // Do knockback in the direction the projectile is moving in.
                initialForceX = projectile.GetInitialForce().x;
            }

            if (!Mathf.Approximately(initialForceX, 0f)) {
                if (initialForceX > 0) {
                    vector = new Vector3(amount, amount);
                } else {
                    vector = new Vector3(-amount, amount);
                }
            } else {
                // Projectile is stationary, base direction off its offset to the player
                if ((playerObject.transform.position - attackingObject.transform.position).x > 0) {
                    vector = new Vector3(amount, amount);
                } else {
                    vector = new Vector3(-amount, amount);
                }
            }
            player.RpcKnockback(vector);
        }

        /// Add a player modifier potentially from a client without authority.
        /// TODO: Exposing this may allow cheating.
        [Command]
        public void CmdAddModifier(NetworkInstanceId playerId, ModId modId) {
            var player = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
            player.AddModifier(Modifier.FromId(modId));
        }

        /// Remove a player modifier potentially from a client without authority.
        /// TODO: Exposing this may allow cheating.
        [Command]
        public void CmdRemoveModifier(NetworkInstanceId playerId, ModId modId) {
            var player = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
            player.RemoveModifier(Modifier.FromId(modId));
        }

        /// Initialize fields that other objects depend on.
        void Awake() {
            Instance = this;
            activePlayersGo = new GameObject[0];
            displayNames = new string[0];
            Layers.Init();

            DontDestroyOnLoad(gameObject);
        }

        /// Spawn items when they are due.
        void Update() {
            if (!isServer) {
                return;
            }

            if (!isPlaying) {
                return;
            }

            if (itemFrequency != ItemFrequency.None && Time.time > nextItemTime) {
                nextItemTime = Time.time + GetNextItemSpawnTime();
                SpawnItem();
            }
        }

        /// Convert from <see cref="ItemFrequency" /> to time.
        /// The times returned from this are randomly generated
        /// from a range based on the current item frequency.
        float GetNextItemSpawnTime() {
            float min = 0f, max = 0f;
            switch (itemFrequency) {
            case ItemFrequency.None:
                return 0f;
            case ItemFrequency.VeryLow:
                min = 20f;
                max = 30f;
                break;
            case ItemFrequency.Low:
                min = 15f;
                max = 23f;
                break;
            case ItemFrequency.Normal:
                min = 10f;
                max = 16f;
                break;
            case ItemFrequency.High:
                min = 7f;
                max = 12f;
                break;
            case ItemFrequency.VeryHigh:
                min = 5f;
                max = 9f;
                break;
            }

            return Random.Range(min, max);
        }

        [Server]
        void SpawnItem() {
            var prefab = items[Random.Range(0, items.Count)];
            var item = Instantiate(prefab, new Vector2(Random.Range(-6f, 6f), 5f), Quaternion.identity);
            NetworkServer.Spawn(item);
        }
    }
}