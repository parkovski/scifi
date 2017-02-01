using UnityEngine;
using System;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class Dynamite : Projectile {
        [HideInInspector]
        public Action explodeCallback;

        void Start() {
            BaseStart();
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