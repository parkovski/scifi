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
using SciFi.Network.Web;
using SciFi.AI;
using SciFi.Util;

namespace SciFi {
    /// Handles "bookkeeping" for the game - which players are active,
    /// when is the game over, who won, when should an item spawn, etc.
    public class GameController : NetworkBehaviour {
        /// Is the game currently active?
        private bool isPlaying;

        private StateChangeListenerFactory stateChangeListenerFactory;
        private IStateChangeListener stateChangeListener;
        private PlayDataLogger playDataLogger;

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

        public int startingLives;
        public int startingDamage = 0;
        /// Player data for the active players.
        List<ServerPlayerData> sPlayerData;
        /// Is this client the winner? This is always false if the game
        /// is not over yet.
        /// The ID of the player controlled by this client.
        /// <seealso cref="SciFi.Players.Player.eId" />
        int cPlayerId;
        /// Need to wait for all clients to be initialized
        /// to send player info and sync starting the game.
        int sReadyClients = 0;
        /// Total number of clients - game starts
        /// when sReadyClients == sNumClients.
        int sNumClients;

        int pDbgLagField = -1;
        float nextPingUpdateTime;
        const float pingUpdateTime = 1f;

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

        private bool playersInitialized = false;
        private event PlayersInitializedHandler _PlayersInitialized;
        /// Event emitted when players are initialized. If they are already
        /// initialized when you subscribe to this event, the callback will
        /// be called immediately.
        public event PlayersInitializedHandler PlayersInitialized {
            add {
                _PlayersInitialized += value;
                if (playersInitialized) {
                    value(sPlayerData.Select(d => d.player).ToArray());
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
        public int RegisterNewPlayer(GameObject playerObject, string displayName, int team, NetworkConnection conn) {
            sPlayerData.Add(new ServerPlayerData {
                playerGo = playerObject,
                displayName = displayName,
                team = team,
                clientConnection = conn,
                aiLevel = 0,
                leaderboardPlayerId = -1,
            });
            return sPlayerData.Count - 1;
        }

        /// Adds a new computer player. Only AI level 1 is supported right now. Level 0 does nothing.
        [Server]
        public int RegisterNewComputerPlayer(GameObject playerObject, string displayName, int team, int aiLevel) {
            sPlayerData.Add(new ServerPlayerData {
                playerGo = playerObject,
                displayName = displayName,
                team = team,
                clientConnection = null,
                aiLevel = aiLevel,
                leaderboardPlayerId = -1,
            });
            return sPlayerData.Count - 1;
        }

        [Server]
        public void SetLeaderboardId(int playerId, int leaderboardId) {
            if (playerId >= sPlayerData.Count) {
                return;
            }
            sPlayerData[playerId].leaderboardPlayerId = leaderboardId;
        }

        void InitializePlayers() {
            playersInitialized = true;
            _PlayersInitialized(sPlayerData.Select(d => d.player).ToArray());
        }

        /// A null return value means this player is either not valid
        /// or is controlled by the server.
        [Server]
        public NetworkConnection ConnectionForPlayer(int playerId) {
            if (playerId > sPlayerData.Count) {
                return null;
            }
            return sPlayerData[playerId].clientConnection;
        }

        /// Start the game.
        /// <param name="showCountdown">Show the countdown and delay
        ///   the game from starting until it is done.</param>
        [Server]
        public void StartGame(bool showCountdown = true) {
            stateChangeListener = stateChangeListenerFactory.Get();
            for (var i = 0; i < sPlayerData.Count; i++) {
                var data = sPlayerData[i];
                var player = data.player;
                player.eId = i;
                if (string.IsNullOrEmpty(data.displayName)) {
                    player.eDisplayName = "P" + (i + 1);
                } else {
                    player.eDisplayName = data.displayName;
                }
                player.eTeam = data.team;
                player.eLives = startingLives;
                player.eDamage = startingDamage;
                data.positionSampler = new ManualCacheSampler<Vector2>(.25f, () => {
                    stateChangeListener.PlayerPositionChanged(i, player.transform.position);
                });
            }

            InitializePlayers();

            StartCoroutine(StartGameWhenPlayersReady(showCountdown));
        }

        IEnumerator StartGameWhenPlayersReady(bool showCountdown) {
            yield return new WaitUntil(() => sPlayerData.All(d => d.player.IsInitialized()));

            stateChangeListener = stateChangeListenerFactory.Get();
            stateChangeListener.GameStarted();
            for (var i = 0; i < sPlayerData.Count; i++) {
                var player = sPlayerData[i].player;
                EventLifeChanged(player.eId, player.eLives, player.eDamage);
                stateChangeListener.LifeChanged(player.eId, player.eLives);
                stateChangeListener.DamageChanged(player.eId, player.eDamage);
            }

            RpcCreateCharacterList(sPlayerData.Select(d => d.player.netId).ToArray());

            this.countdown = GameObject.Find("StaticCanvas").GetComponent<Countdown>();
            RpcStartGame(showCountdown);
            if (showCountdown) {
                this.countdown.Start();
                System.GC.Collect();
                this.countdown.OnFinished += _ => {
                    this.isPlaying = true;
                    _GameStarted();
                    stateChangeListener.GameStarted();
                };
            } else {
                this.isPlaying = true;
                _GameStarted();
                stateChangeListener.GameStarted();
            }
        }

        /// Initialize the list of players on the client.
        [ClientRpc]
        void RpcCreateCharacterList(NetworkInstanceId[] ids) {
            if (isServer) {
                return;
            }
            sPlayerData = ids.Select(id => new ServerPlayerData {
                playerGo = ClientScene.FindLocalObject(id),
            }).ToList();
            StartCoroutine(WaitForPlayersToSync());
        }

        IEnumerator WaitForPlayersToSync() {
            yield return new WaitWhile(() => sPlayerData.Count(d => d.player.eId == 0) > 1);
            yield return new WaitUntil(() => sPlayerData.Any(d => d.player.hasAuthority));
            cPlayerId = sPlayerData.First(d => d.player.hasAuthority).player.eId;
            foreach (var pd in sPlayerData) {
                stateChangeListener.LifeChanged(pd.player.eId, pd.player.eLives);
                stateChangeListener.DamageChanged(pd.player.eId, pd.player.eDamage);
            }
            // If this copy is both client and server, the server
            // side will already have called this.
            if (!isServer) {
                InitializePlayers();
            }
        }

        /// Find the active player with ID <c>id</c>.
        public Player GetPlayer(int id) {
            if (id >= sPlayerData.Count) {
                return null;
            }

            return sPlayerData[id].player;
        }

        /// Find the winner and report it to the clients.
        int FindWinner() {
            Player winner;
            winner = sPlayerData.SingleOrDefault(d => d.player.eLives > 0).player;
            if (winner == null) {
                return -1;
            }
            return winner.eId;
        }

        /// End the game and load the game over scene.
        [Server]
        public void EndGame() {
            isPlaying = false;
            stateChangeListener.GameEnded();
            var winner = FindWinner();
            RpcEndGame(winner);
            var result = new MatchResult {
                winner = sPlayerData[winner].leaderboardPlayerId,
                players = sPlayerData.Select(d => new PlayerMatchInfo {
                    id = d.leaderboardPlayerId,
                    kills = d.player.sKills,
                    deaths = d.player.sDeaths,
                }).ToArray(),
            };
            foreach (var d in sPlayerData) {
                Destroy(d.playerGo);
            }
            StartCoroutine(TransitionToGameOver());
#if !UNITY_EDITOR
            if (TransitionParams.gameType == GameType.Multi) {
#endif
                StartCoroutine(ReportMatchResult(result));
#if !UNITY_EDITOR
            }
#endif
        }

        IEnumerator ReportMatchResult(MatchResult result) {
            var request = Leaderboard.PostMatchResultsRequest(result);
            if (request == null) {
                yield break;
            }
            yield return request.Send();
            if (request.isNetworkError) {
                print("Error reporting match result: " + request.error);
            } else {
                print("match creation: " + request.downloadHandler.text);
            }
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
        void RpcStartGame(bool showCountdown) {
            if (isServer) {
                return;
            }
            if (!showCountdown) {
                this.isPlaying = true;
                _GameStarted();
                stateChangeListener.GameStarted();
                return;
            }

            this.countdown = GameObject.Find("StaticCanvas").GetComponent<Countdown>();
            this.countdown.Start();
            System.GC.Collect();
            this.countdown.OnFinished += _ => {
                this.isPlaying = true;
                _GameStarted();
                stateChangeListener.GameStarted();
            };
        }

        /// End the game on the client and load the game over scene.
        [ClientRpc]
        void RpcEndGame(int winnerId) {
            isPlaying = false;
            if (winnerId == cPlayerId) {
                TransitionParams.isWinner = true;
            } else {
                TransitionParams.isWinner = false;
            }
            if (!isServer) {
                StartCoroutine(TransitionToGameOver());
            }
        }

        IEnumerator TransitionToGameOver() {
            var music = GameObject.Find("Music").GetComponent<AudioSource>();
            Effects.FadeOut();
            Effects.FadeOutAudio(music, .9f, 20);
            yield return new WaitForSeconds(1f);
            sPlayerData.Clear();
            SceneManager.LoadScene("GameOver");
            if (TransitionParams.gameType == GameType.Single) {
                var netMan = FindObjectOfType<SinglePlayerNetworkManager>();
                netMan.StopHost();
                Destroy(netMan);
            }
        }

        /// Convert a prefab object to its index in the spawnable prefabs list.
        public static int PrefabToIndex(GameObject prefab) {
            return spawnPrefabList.IndexOf(prefab);
        }

        /// Convert an index in the spawnable prefabs list to a prefab object.
        public static GameObject IndexToPrefab(int index) {
            return spawnPrefabList[index];
        }

        public GameObject GetFromNetPool(int prefabIndex, Vector3 position, Quaternion rotation) {
            return gameObjectPool.GetNet(prefabIndex, position, rotation);
        }

        public GameObject GetFromLocalPool(GameObject prefab, Vector3 position, Quaternion rotation) {
            return gameObjectPool.Get(prefab, position, rotation);
        }

        /// Deduct a life from the player, respawn, and
        /// check if the game is over.
        [Server]
        public void Die(GameObject playerObject) {
            var player = playerObject.GetComponent<Player>();
            --player.eLives;
            ++player.sDeaths;
            if (player.sLastAttacker != null) {
                ++player.sLastAttacker.sKills;
            }
            if (sPlayerData.Count(d => d.player.eLives > 0) == 1) {
                EndGame();
                return;
            }

            player.eDamage = startingDamage;
            if (player.eLives <= 0) {
                var rb = player.GetComponent<Rigidbody2D>();
                rb.isKinematic = true;
                rb.gravityScale = 0;
                player.RpcYouDeadFool();
            } else {
                player.RpcRespawn(new Vector3(0f, 5f));
                player.AddModifier(ModId.CantMove);
                player.AddModifier(ModId.CantAttack);
                player.AddModifier(ModId.Invincible);
                StartCoroutine(RemoveRespawnModifiers(player));
            }
            EventLifeChanged(player.eId, player.eLives, player.eDamage);
            stateChangeListener.LifeChanged(player.eId, player.eLives);
            stateChangeListener.DamageChanged(player.eId, player.eDamage);
        }

        IEnumerator RemoveRespawnModifiers(Player player) {
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => player.transform.position.y < 3f);
            player.RemoveModifier(ModId.CantMove);
            player.RemoveModifier(ModId.CantAttack);
            yield return new WaitForSeconds(2f);
            player.RemoveModifier(ModId.Invincible);
        }

        [Server]
        public void Hit(GameObject obj, IAttackSource attack, int damage) {
            Hit(obj, attack, null, damage, 0f, false);
        }

        [Server]
        public void HitNoVelocityReset(GameObject obj, IAttackSource attack, GameObject attackingObject, int damage, float knockback) {
            Hit(obj, attack, attackingObject, damage, knockback, false);
        }

        [Server]
        public void Hit(GameObject obj, IAttackSource attack, GameObject attackingObject, int damage, float knockback) {
            Hit(obj, attack, attackingObject, damage, knockback, true);
        }

        [Server]
        private void Hit(GameObject obj, IAttackSource attack, GameObject attackingObject, int damage, float knockback, bool resetVelocity) {
            if (damage != 0) {
                TakeDamage(obj, attack, damage);
            }
            if (!Mathf.Approximately(knockback, 0f)) {
                Knockback(attackingObject, obj, knockback, resetVelocity);
            }
        }

        /// Inflict damage on a player or item.
        [Server]
        void TakeDamage(GameObject obj, IAttackSource attack, int amount) {
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
        void PlayerTakeDamage(Player player, IAttackSource attack, int amount) {
            if (player.IsModifierEnabled(ModId.Invincible)) {
                return;
            }
            player.eDamage += amount;
            player.Hit(amount);
            player.Interact(attack);
            EventDamageChanged(player.eId, player.eDamage);
            stateChangeListener.DamageChanged(player.eId, player.eDamage);
        }

        /// Inflict damage on an item.
        [Server]
        void ItemTakeDamage(Item item, IAttackSource attack, int amount) {
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
            if (player.IsModifierEnabled(ModId.Invincible)) {
                return;
            }
            if (!Mathf.Approximately(amount, 0f)) {
                amount *= 50 * player.eDamage;
                if (amount < 5000 && amount > 0) {
                    amount = 5000;
                } else if (amount > -5000 && amount < 0) {
                    amount = -5000;
                }
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
            stateChangeListenerFactory = new StateChangeListenerFactory();
#if UNITY_EDITOR
            playDataLogger = new PlayDataLogger(Application.streamingAssetsPath + "/pdl.txt");
            stateChangeListenerFactory.Add(playDataLogger);
#endif
            sPlayerData = new List<ServerPlayerData>();
            Layers.Init();
            PlayersInitialized += players => {
                FindObjectOfType<EnableUI>().Enable();
                foreach (var player in players) {
                    IInputManager playerInputManager;
                    if (isClient && player.eId == cPlayerId) {
                        playerInputManager = GetComponent<InputManager>();
                    } else if (isServer && sPlayerData[player.eId].aiLevel > 0) {
                        var aiim = new AIInputManager();
                        AddAI(player.gameObject, aiim, sPlayerData[player.eId].aiLevel);
                        playerInputManager = aiim;
                    } else {
                        playerInputManager = NullInputManager.Instance;
                    }
                    player.GameControllerReady(this, playerInputManager);
                }
            };

            GameStarted += () => {
                foreach (var data in sPlayerData) {
                    var player = data.player;
                    player.RemoveModifier(ModId.CantAttack);
                    player.RemoveModifier(ModId.CantMove);
                    if (player.isLocalPlayer) {
                        FindObjectOfType<CameraScroll>().playerToFollow = player;
                    }
                }
            };

            if (TransitionParams.gameType == GameType.Single) {
                spawnPrefabList = new JitList<GameObject>();
            } else {
                spawnPrefabList = NetworkManager.singleton.spawnPrefabs;
            }
            gameObjectPool = new GameObjectPool();

            Instance = this;
        }

        void OnApplicationPause(bool pause) {
        }

        void OnApplicationQuit() {
            if (playDataLogger != null) {
                playDataLogger.Dispose();
            }
        }

        [Server]
        void AddAI(GameObject player, AIInputManager inputManager, int level) {
            AIBase ai;
            if (level == 1) {
                ai = player.AddComponent<DumbAI>();
            } else if (level == 2) {
                ai = player.AddComponent<StrategyAI>();
            } else {
                throw new System.ArgumentOutOfRangeException("level");
            }
            ai.inputManager = inputManager;
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
            if (Time.time > nextPingUpdateTime) {
                if (pDbgLagField == -1) {
                    pDbgLagField = DebugPrinter.Instance.NewField();
                }
                nextPingUpdateTime = Time.time + pingUpdateTime;
                DebugPrinter.Instance.SetField(pDbgLagField, "Ping: " + 0);
            }

            for (int i = 0; i < sPlayerData.Count; i++) {
                var data = sPlayerData[i];
                if (data.positionSampler == null) {
                    continue;
                }
                data.positionSampler.Run(data.playerGo.transform.position);
            }

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
    public delegate void LifeChangedHandler(int playerId, int newLives, int newDamage);
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