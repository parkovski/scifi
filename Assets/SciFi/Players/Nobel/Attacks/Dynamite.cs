using UnityEngine;
using UnityEngine.Networking;
using System;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Dynamite : Projectile {
        public GameObject explosionPrefab;

        public GameObject[] additionalSticks;
        public float explosionScale;
        public int damage;
        public float knockback;

        /// Called on the server when the dynamite is exploded.
        [HideInInspector]
        public Action explodeCallback;
        /// Called on the server when the dynamite is destroyed.
        [HideInInspector]
        public Action<GameObject> destroyCallback;

        void OnDestroy() {
            if (destroyCallback != null) {
                destroyCallback(gameObject);
            }
        }

        [Server]
        public void Explode() {
            var explosionGo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var explosion = explosionGo.GetComponent<Explosion>();
            explosion.scale = explosionScale;
            explosion.damage = damage;
            explosion.knockback = knockback;
            NetworkServer.Spawn(explosionGo);

            if (explodeCallback != null) {
                explodeCallback();
            }
            Destroy(gameObject);
        }
    }
}