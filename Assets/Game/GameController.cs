using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

using SciFi.Players;
using SciFi.UI;

namespace SciFi {
    public class DamageChangedEventArgs : EventArgs {
        public int playerId;
        public int newDamage;
    }
    public delegate void DamageChangedHandler(DamageChangedEventArgs args);

    public class LifeChangedEventArgs : EventArgs {
        public int playerId;
        public int newLives;
    }
    public delegate void LifeChangedHandler(LifeChangedEventArgs args);

    public static class Layers {
        public static int projectiles;
        public static int items;
        public static int players;
        public static int displayOnly;
        public static int heldAttacks;

        public static void Init() {
            projectiles = LayerMask.NameToLayer("Projectiles");
            items = LayerMask.NameToLayer("Items");
            players = LayerMask.NameToLayer("Players");
            displayOnly = LayerMask.NameToLayer("Display Only");
            heldAttacks = LayerMask.NameToLayer("Held Attacks");
        }
    }

    public enum ItemFrequency {
        None,
        VeryLow,
        Low,
        Normal,
        High,
        VeryHigh,
    }

    public class GameController : NetworkBehaviour {
        private bool isPlaying;

        /// Set on scene change to the countdown in the main game scene.
        [HideInInspector]
        public Countdown countdown;

        // Items
        public ItemFrequency itemFrequency;
        public List<GameObject> items;
        float nextItemTime;

        // Active players, even if dead. Null if no game is running,
        // guaranteed not null if a game is running.
        Player[] activePlayers;
        GameObject[] activePlayersGo;

        [SyncEvent]
        public event DamageChangedHandler EventDamageChanged;
        /// Implies damage is set to 0.
        [SyncEvent]
        public event LifeChangedHandler EventLifeChanged;

        public static GameController Instance { get; private set; }

        [Server]
        public void RegisterNewPlayer(GameObject playerObject) {
            activePlayersGo = activePlayersGo.Concat(new[] { playerObject }).ToArray();
        }

        [Server]
        public void StartGame() {
            activePlayers = activePlayersGo.Select(p => p.GetComponent<Player>()).ToArray();

            for (var i = 0; i < activePlayersGo.Length; i++) {
                var player = activePlayersGo[i].GetComponent<Player>();
                player.id = i + 1;
                player.lives = 5;
                //player.SuspendAllFeatures();
                EventLifeChanged(new LifeChangedEventArgs {
                    playerId = player.id,
                    newLives = player.lives,
                });
            }

            countdown = GameObject.Find("Canvas").GetComponent<Countdown>();
            RpcStartGame();
            countdown.StartGame();
            countdown.OnFinished += _ => {
                this.isPlaying = true;
                foreach (var p in activePlayers) {
                    // TODO: this needs to be run on both server and client
                    // including the SuspendAllFeatures call above.
                    // Attack and Movement are handled on the client,
                    // while Damage and Knockback are handled on the server.
                    p.ResumeAllFeatures(true);
                }
            };
        }

        [Server]
        public void EndGame() {
            RpcEndGame();
        }

        public bool IsPlaying() {
            return isPlaying;
        }

        [ClientRpc]
        void RpcStartGame() {
            countdown = GameObject.Find("Canvas").GetComponent<Countdown>();
            countdown.StartGame();
            countdown.OnFinished += _ => this.isPlaying = true;
        }

        [ClientRpc]
        void RpcEndGame() {
            isPlaying = false;
        }

        public static int PrefabToIndex(GameObject prefab) {
            return NetworkManager.singleton.spawnPrefabs.IndexOf(prefab);
        }

        public static GameObject IndexToPrefab(int index) {
            return NetworkManager.singleton.spawnPrefabs[index];
        }

        [Command]
        public void CmdDie(GameObject playerObject) {
            var player = playerObject.GetComponent<Player>();
            --player.lives;
            player.damage = 0;
            player.RpcRespawn(new Vector3(0f, 7f));
            EventLifeChanged(new LifeChangedEventArgs {
                playerId = player.id,
                newLives = player.lives,
            });
        }

        [Server]
        public void TakeDamage(GameObject playerObject, int amount) {
            var player = playerObject.GetComponent<Player>();
            if (!player.FeatureEnabled(PlayerFeature.Damage)) {
                    return;
            }
            player.damage += amount;
            var args = new DamageChangedEventArgs {
                playerId = player.id,
                newDamage = player.damage,
            };
            EventDamageChanged(args);
        }

        [Server]
        public void Knockback(GameObject attackingObject, GameObject playerObject, float amount) {
            var player = playerObject.GetComponent<Player>();
            if (!player.FeatureEnabled(PlayerFeature.Knockback)) {
                return;
            }
            amount *= player.damage;
            var vector = playerObject.transform.position - attackingObject.transform.position;
            var force = transform.up * amount;
            if (vector.x < 0) {
                amount = -amount;
            }
            force += transform.right * amount;
            player.RpcKnockback(force);
        }

        void Awake() {
            Instance = this;
            activePlayersGo = new GameObject[0];
            Layers.Init();

            DontDestroyOnLoad(gameObject);
        }

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