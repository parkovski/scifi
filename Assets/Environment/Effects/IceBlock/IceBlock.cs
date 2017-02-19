using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;
using SciFi.Players.Modifiers;

namespace SciFi.Environment.Effects {
    public class IceBlock : MonoBehaviour {
        [HideInInspector]
        public Player frozenPlayer;

        const float freezeTime = 3.5f;
        float unfreezeTime;

        void Start() {
            if (NetworkServer.active) {
                frozenPlayer.AddModifier(Modifier.Frozen);
                frozenPlayer.AddModifier(Modifier.Slow);
                unfreezeTime = Time.time + freezeTime;
            }
            transform.position = frozenPlayer.transform.position;
        }

        void Update() {
            transform.position = frozenPlayer.transform.position;

            if (!NetworkServer.active) {
                return;
            }

            if (Time.time > unfreezeTime) {
                frozenPlayer.RemoveModifier(Modifier.Frozen);
                frozenPlayer.RemoveModifier(Modifier.Slow);
                Destroy(gameObject);
            }
        }
    }
}