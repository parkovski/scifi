using UnityEngine;
using System;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class Dynamite : Projectile {
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

        public void Explode() {
            Effects.Explosion(transform.position);
            if (explodeCallback != null) {
                explodeCallback();
            }
            Destroy(gameObject);
        }
    }
}