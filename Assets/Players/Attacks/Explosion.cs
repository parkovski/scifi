using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class Explosion : NetworkBehaviour, IAttackSource {
        [SyncVar, HideInInspector]
        public float scale = 1;
        [SyncVar, HideInInspector]
        public int damage;
        [SyncVar, HideInInspector]
        public float knockback;

        const float aliveTime = 0.25f;
        float startTime;
        HashSet<GameObject> hitObjects;

        void Start() {
            hitObjects = new HashSet<GameObject>();
            startTime = Time.time;
            scale *= 200;
        }

        void Update() {
            float time = Time.time - startTime;
            if (time > aliveTime) {
                Destroy(gameObject);
            }
            var size = time.Scale(0, aliveTime, 1, scale);
            transform.localScale = new Vector3(size, size, 0);
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (!isServer) {
                return;
            }

            if (Attack.GetAttackHit(collider.gameObject.layer) == AttackHit.None) {
                return;
            }

            if (hitObjects.Contains(collider.gameObject)) {
                return;
            }
            hitObjects.Add(collider.gameObject);
            GameController.Instance.Hit(collider.gameObject, this, gameObject, damage, knockback);
        }

        public AttackType Type { get { return AttackType.Projectile; } }
        public AttackProperty Properties { get { return AttackProperty.Explosive; } }
    }
}
