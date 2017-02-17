using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Random = UnityEngine.Random;

using SciFi.Players;
using SciFi.Players.Attacks;
using SciFi.Players.Modifiers;
using SciFi.Items;
using SciFi.UI;
using SciFi.Environment.Effects;
using SciFi.Scenes;
using SciFi.Network;
using SciFi.AI;
using SciFi.Util;

namespace SciFi {
    /// Handles "bookkeeping" for the game - which players are active,
    /// when is the game over, who won, when should an item spawn, etc.
    public class GameController : NetworkBehaviour {
        /// Is the game currently active?
        private bool isPlaying;

        /// Set on scene change to the countdown in the main game scene.
        private Countdown countdown;

        /// Either the network server's spawn prefab list in multiplayer mode,
        /// or a JitList which adds the prefab to the list when it is requested,
        /// in single player mode. Note - this approach would not work over the
        /// network, but luckily, it is only needed in single player mode.
        private static IList<GameObject> spawnPrefabList;
        private GameObjectPool gameObjectPool;

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
        /// Teams/colors
        int[] teams;
        /// Connections for each client
        NetworkConnection[] sClientConnections;
        /// AI Level for each player, or -1 if none.
        int[] sAILevels;
        /// Is this client the winner? This is always false if the game
        /// is not over yet.
        bool cIsWinner;
        /// The ID of the player controlled by this client.
        /// <seealso cref="SciFi.Players.Player.eId" />
        int cPlayerId;
        /// Need to wait for all clients to be initialized
        /// to send player info and sync starting the game.
        int sReadyClients = 0;
        /// Total number of clients - game starts
        /// when sReadyClients == sNumClients.
        int sNumClients;

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
        public void RegisterNewPlayer(GameObject playerObject, string displayName, int team, NetworkConnection conn) {
            activePlayersGo = activePlayersGo.Concat(new[] { playerObject }).ToArray();
            displayNames = displayNames.Concat(new[] { displayName }).ToArray();
            teams = teams.Concat(new[] { team }).ToArray();
            sClientConnections = sClientConnections.Concat(new[] { conn }).ToArray();
            sAILevels = sAILevels.Concat(new[] { 0 }).ToArray();
        }

        /// Adds a new computer player. Only AI level 1 is supported right now. Level 0 does nothing.
        public void RegisterNewComputerPlayer(GameObject playerObject, string displayName, int team, int aiLevel) {
            activePlayersGo = activePlayersGo.Concat(new[] { playerObject }).ToArray();
            displayNames = displayNames.Concat(new[] { displayName }).ToArray();
            teams = teams.Concat(new[] { team }).ToArray();
            sClientConnections = sClientConnections.Concat(new NetworkConnection[] { null }).ToArray();
            sAILevels = sAILevels.Concat(new[] { aiLevel }).ToArray();
        }

        /// A null return value means this player is either not valid
        /// or is controlled by the server.
        [Server]
        public NetworkConnection ConnectionForPlayer(int playerId) {
            if (playerId > sClientConnections.Length) {
                return null;
            }
            return sClientConnections[playerId];
        }

        /// Start the game.
        /// <param name="countdown">Show the countdown and delay
        ///   the game from starting until it is done.</param>
        [Server]
        public void StartGame(bool countdown = true) {
            activePlayers = activePlayersGo.Select(p => p.GetComponent<Player>()).ToArray();

            for (var i = 0; i < activePlayers.Length; i++) {
                var player = activePlayers[i];
                player.eId = i;
                if (string.IsNullOrEmpty(displayNames[i])) {
                    player.eDisplayName = "P" + (i + 1);
                } else {
                    player.eDisplayName = displayNames[i];
                }
                player.eTeam = teams[i];
                player.eLives = 5;
            }

            _PlayersInitialized(activePlayers);

            StartCoroutine(StartGameWhenPlayersReady());
        }

        IEnumerator StartGameWhenPlayersReady() {
            yield return new WaitUntil(() => activePlayers.All(p => p.IsInitialized()));

            for (var i = 0; i < activePlayers.Length; i++) {
                var player = activePlayers[i];
                EventLifeChanged(player.eId, player.eLives);
            }

            displayNames = null;
            teams = null;

            RpcCreateCharacterList(activePlayers.Select(p => p.netId).ToArray());

            this.countdown = GameObject.Find("Canvas").GetComponent<Countdown>();
            RpcStartGame(countdown);
            if (countdown) {
                this.countdown.StartGame();
                System.GC.Collect();
                this.countdown.OnFinished += _ => {
                    this.isPlaying = true;
                    foreach (var p in activePlayers) {
                        p.RemoveModifier(Modifier.CantMove);
                        p.RemoveModifier(Modifier.CantAttack);
                    }
                    _GameStarted();
                };
            } else {
                this.isPlaying = true;
                _GameStarted();
            }
        }

        /// Initialize the list of players on the client.
        [ClientRpc]
        void RpcCreateCharacterList(NetworkInstanceId[] ids) {
            activePlayersGo = ids.Select(id => ClientScene.FindLocalObject(id)).ToArray();
            activePlayers = activePlayersGo.Select(p => p.GetComponent<Player>()).ToArray();
            StartCoroutine(WaitForPlayersToSync());
        }

        IEnumerator WaitForPlayersToSync() {
            yield return new WaitWhile(() => activePlayers.Count(p => p.eId == 0) > 1);
            cPlayerId = activePlayers.First(p => p.hasAuthority).eId;
            // If this copy is both client and server, the server
            // side will already have called this.
            if (!isServer) {
                _PlayersInitialized(activePlayers);
            }
        }

        /// Find the active player with ID <c>id</c>.
        public Player GetPlayer(int id) {
            if (activePlayers == null) {
                return null;
            }
            if (id >= activePlayers.Length) {
                return null;
            }

            return activePlayers[id];
        }

        /// Find the winner and report it to the clients.
        int FindWinner() {
            Player winner;
            try {
                winner = activePlayers.Single(p => p.eLives > 0);
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

        /// Start the game on the client. Note the isServer checks -
        /// the server will already have raised these events
        /// when a client/server copy is running, and we don't want to
        /// call them twice.
        [ClientRpc]
        void RpcStartGame(bool countdown) {
            cIsWinner = false;
            if (!countdown) {
                this.isPlaying = true;
                if (!isServer) {
                    _GameStarted();
                }
                return;
            }

            this.countdown = GameObject.Find("Canvas").GetComponent<Countdown>();
            this.countdown.StartGame();
            System.GC.Collect();
            this.countdown.OnFinished += _ => {
                this.isPlaying = true;
                if (!isServer) {
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
            return spawnPrefabList.IndexOf(prefab);
        }

        /// Convert an index in the spawnable prefabs list to a prefab object.
        public static GameObject IndexToPrefab(int index) {
            return spawnPrefabList[index];
        }

        public GameObject GetFromPool(int prefabIndex, Vector3 position, Quaternion rotation) {
            return gameObjectPool.Get(prefabIndex, position, rotation);
        }

        public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation) {
            return gameObjectPool.Get(PrefabToIndex(prefab), position, rotation);
        }

        /// Deduct a life from the player, respawn, and
        /// check if the game is over.
        [Server]
        public void Die(GameObject playerObject) {
            var player = playerObject.GetComponent<Player>();
            --player.eLives;
            if (activePlayers.Count(p => p.eLives > 0) == 1) {
                int winnerId = FindWinner();

                foreach (var go in activePlayersGo) {
                    Destroy(go);
                }

                EndGame(winnerId);
                return;
            }

            player.eDamage = 0;
            if (player.eLives <= 0) {
                var rb = player.GetComponent<Rigidbody2D>();
                rb.isKinematic = true;
                rb.gravityScale = 0;
                player.RpcYouDeadFool();
            } else {
                player.RpcRespawn(new Vector3(0f, 7f));
            }
            EventLifeChanged(player.eId, player.eLives);
        }

        [Server]
        public void Hit(GameObject obj, IAttack attack, int damage) {
            Hit(obj, attack, null, damage, 0f, false);
        }

        [Server]
        public void HitNoVelocityReset(GameObject obj, IAttack attack, GameObject attackingObject, int damage, float knockback) {
            Hit(obj, attack, attackingObject, damage, knockback, false);
        }

        [Server]
        public void Hit(GameObject obj, IAttack attack, GameObject attackingObject, int damage, float knockback) {
            Hit(obj, attack, attackingObject, damage, knockback, true);
        }

        [Server]
        private void Hit(GameObject obj, IAttack attack, GameObject attackingObject, int damage, float knockback, bool resetVelocity) {
            if (damage != 0) {
                TakeDamage(obj, attack, damage);
            }
            if (!Mathf.Approximately(knockback, 0f)) {
                Knockback(attackingObject, obj, knockback, resetVelocity);
            }
        }

        /// Inflict damage on a player or item.
        [Server]
        void TakeDamage(GameObject obj, IAttack attack, int amount) {
            var player = obj.GetComponent<Player>();
            if (player == null) {
                var item = obj.GetComponent<Item>();
                if (item != null) {
                    ItemTakeDamage(item, attack, amount);
                } else {
                    var projectile = obj.GetComponent<Projectile>();
                    if (projectile != null) {
                        projectile.Interact(attack);
                    }
                }
            } else {
                PlayerTakeDamage(player, attack, amount);
            }
        }

        /// Inflict damage on a player.
        [Server]
        void PlayerTakeDamage(Player player, IAttack attack, int amount) {
            if (player.IsModifierEnabled(Modifier.Invincible)) {
                return;
            }
            player.eDamage += amount;
            player.Hit(amount);
            player.Interact(attack);
            EventDamageChanged(player.eId, player.eDamage);
        }

        /// Inflict damage on an item.
        [Server]
        void ItemTakeDamage(Item item, IAttack attack, int amount) {
            item.TakeDamage(amount);
            item.Interact(attack);
        }

        /// Inflict knockback on a player. Determines which direction the knockback
        /// should come from, based on the projectile's initial force if set,
        /// or the attacking object's offset to the player.
        [Server]
        void Knockback(GameObject attackingObject, GameObject playerObject, float amount, bool resetVelocity) {
            var player = playerObject.GetComponent<Player>();
            if (player == null) {
                return;
            }
            if (player.IsModifierEnabled(Modifier.Invincible)) {
                return;
            }
            amount *= 50 * player.eDamage;
            if (amount < 5000 && amount > 0) {
                amount = 5000;
            } else if (amount > -5000 && amount < 0) {
                amount = -5000;
            }
            Vector2 vector;
            var projectile = attackingObject.GetComponent<Projectile>();
            var initialForceX = 0f;
            if (projectile != null) {
                // Do knockback in the direction the projectile is moving in.
                initialForceX = projectile.GetInitialVelocity().x;
            }

            if (!Mathf.Approximately(initialForceX, 0f)) {
                if (initialForceX > 0) {
                    vector = new Vector2(amount, amount);
                } else {
                    vector = new Vector2(-amount, amount);
                }
            } else {
                // Projectile is stationary, base direction off its offset to the player
                if ((playerObject.transform.position - attackingObject.transform.position).x > 0) {
                    vector = new Vector2(amount, amount);
                } else {
                    vector = new Vector2(-amount, amount);
                }
            }
            player.Knockback(vector, resetVelocity);
        }

        /// Initialize fields that other objects depend on.
        void Awake() {
            Instance = this;
            activePlayersGo = new GameObject[0];
            displayNames = new string[0];
            teams = new int[0];
            sClientConnections = new NetworkConnection[0];
            sAILevels = new int[0];
            Layers.Init();
            PlayersInitialized += players => {
                foreach (var player in players) {
                    IInputManager playerInputManager;
                    if (isClient && player.eId == cPlayerId) {
                        playerInputManager = GetComponent<InputManager>();
                    } else if (isServer && sAILevels[player.eId] > 0) {
                        var aiim = new AIInputManager();
                        AddAI(player.gameObject, aiim, sAILevels[player.eId]);
                        playerInputManager = aiim;
                    } else {
                        playerInputManager = new NullInputManager();
                    }
                    player.GameControllerReady(this, playerInputManager);
                }
            };

            GameStarted += () => {
                foreach (var player in activePlayers) {
                    player.RemoveModifier(Modifier.CantAttack);
                    player.RemoveModifier(Modifier.CantMove);
                }
            };

            DontDestroyOnLoad(gameObject);
        }

        [Server]
        void AddAI(GameObject player, AIInputManager inputManager, int level) {
            if (level < 1 || level > 1) {
                throw new System.ArgumentOutOfRangeException("level");
            }

            var ai = player.AddComponent<DumbAI>();
            ai.inputManager = inputManager;
        }

        void Start() {
            if (TransitionParams.gameType == GameType.Single) {
                spawnPrefabList = new JitList<GameObject>();
            } else {
                spawnPrefabList = NetworkManager.singleton.spawnPrefabs;
            }
            gameObjectPool = new GameObjectPool();
        }

        public override void OnStartServer() {
            NetworkServer.RegisterHandler(NetworkMessages.ClientGameReady, ClientGameReady);
        }

        [Server]
        void ClientGameReady(NetworkMessage message) {
            if (++sReadyClients == sNumClients) {
                StartGame(
#if UNITY_EDITOR
                    false
#endif
                );
            }
        }

        [Server]
        public void SetClientCount(int numClients) {
            sNumClients = numClients;
        }

        public override void OnStartClient() {
            if (TransitionParams.gameType == GameType.Multi) {
                NetworkController.clientConnectionToServer.Send(NetworkMessages.ClientGameReady, new EmptyMessage());
            }
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
}