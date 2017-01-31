using UnityEngine;
using System;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Dynamite : Projectile {
        [HideInInspector]
        public Action explodeCallback;

        void Start() {
            BaseStart();
        }

        public void Explode() {
            if (explodeCallback != null) {
                explodeCallback();
            }
            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (Attack.GetAttackHit(collision.gameObject.layer) != AttackHit.None) {
                Explode();
            }
        }
    }
}