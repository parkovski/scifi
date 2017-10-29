using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;

namespace SciFi.Items {
    /// Wraps a projectile so it can be picked up and thrown.
    public class ProjectileItemContainer : Item {
        [SyncVar, HideInInspector]
        public GameObject projectile;

        void Start() {
            BaseStart();
            transform.position = projectile.transform.position;
            projectile.transform.parent = transform;
            projectile.layer = Layers.noncollidingItems;
            lRb = projectile.GetComponent<Rigidbody2D>();
            spriteRenderer = projectile.GetComponent<SpriteRenderer>();
        }

        void Update() {
            BaseUpdate();
            if (eOwnerGo != null) {
                projectile.transform.localPosition = Vector3.zero;
            }
        }

        protected override void OnDiscard() {
            projectile.layer = Layers.projectiles;
        }

        public override bool ShouldCharge() {
            return false;
        }

        public override bool ShouldThrow() {
            return true;
        }

        public override AttackType Type { get { return AttackType.Projectile; } }
    }
}