using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;
using SciFi.Players.Modifiers;

namespace SciFi.Environment.Effects {
    public class IceBlock : MonoBehaviour, IPoolNotificationHandler {
        [HideInInspector]
        public Player frozenPlayer;

        const float freezeTime = 3.5f;
        float unfreezeTime;

        IPooledObject pooled;

        void Awake() {
            pooled = PooledObject.Get(gameObject);
        }

        void Start() {
            Reinit();
        }

        void Reinit() {
            if (NetworkServer.active) {
                frozenPlayer.AddModifier(Modifier.Frozen);
                frozenPlayer.AddModifier(Modifier.Slow);
            }
            unfreezeTime = Time.time + freezeTime;
            transform.position = frozenPlayer.transform.position;
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }

            transform.position = frozenPlayer.transform.position;

            if (Time.time > unfreezeTime) {
                if (NetworkServer.active) {
                    frozenPlayer.RemoveModifier(Modifier.Frozen);
                    frozenPlayer.RemoveModifier(Modifier.Slow);
                }
                pooled.Release();
            }
        }

        void IPoolNotificationHandler.OnAcquire() {
            Reinit();
            PooledObject.Enable(gameObject);
        }

        void IPoolNotificationHandler.OnRelease() {
            PooledObject.Disable(gameObject);
        }
    }
}