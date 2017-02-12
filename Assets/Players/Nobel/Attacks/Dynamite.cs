using UnityEngine;
using UnityEngine.Networking;
using System;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class Dynamite : Projectile {
        public GameObject explosionPrefab;

        /// Called on the server when the dynamite is exploded.
        [HideInInspector]
        public Action explodeCallback;
        /// Called on the server when the dynamite is destroyed.
        [HideInInspector]
        public Action destroyCallback;

        void Start() {
            BaseStart();
        }

        void OnDestroy() {
            if (destroyCallback != null) {
                destroyCallback();
            }
        }

        [Server]
        public void Explode() {
            Effects.Explosion(transform.position);
            var explosionGo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var explosion = explosionGo.GetComponent<Explosion>();
            explosion.damage = 10;
            explosion.knockback = 5f;
            NetworkServer.Spawn(explosionGo);

            if (explodeCallback != null) {
                explodeCallback();
            }
            Destroy(gameObject);
        }
    }
}