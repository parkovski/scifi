using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;

namespace SciFi.Items {
    /// A projectile, spawned by either a player or an item,
    /// which cannot be held but otherwise interacts with players
    /// and other items.
    public class Projectile : NetworkBehaviour, IAttack, IInteractable {
        /// The player or other object that spawned the projectile.
        /// Projectiles won't collide with objects that spawned them.
        [HideInInspector]
        protected NetworkInstanceId spawnedBy = NetworkInstanceId.Invalid;
        /// If the player is holding an item, set this so the projectile doesn't hit it.
        [HideInInspector]
        protected NetworkInstanceId spawnedByExtra = NetworkInstanceId.Invalid;
        [HideInInspector]
        bool flipX;

        /// When an object collides, it bounces in the opposite direction
        /// sometimes causing knockback to go in the wrong direction.
        /// This remembers the first force applied and uses that for knockback.
        /// If an object is supposed to bounce, it will need to change this.
        [SyncVar]
        protected Vector3 initialVelocity;

        [ClientRpc]
        void RpcInitialize(
            NetworkInstanceId spawnedBy,
            NetworkInstanceId spawnedByExtra,
            bool flipX,
            Vector3 position,
            Quaternion rotation
        ) {
            if (isServer) {
                return;
            }

            this.spawnedBy = spawnedBy;
            this.spawnedByExtra = spawnedByExtra;
            this.flipX = flipX;
            this.transform.position = position;
            this.transform.rotation = rotation;
            Initialize();
        }

        /// This also syncs position and rotation, so make sure to
        /// set those before calling this.
        /// !!! Be sure to spawn before calling this !!!
        [Server]
        public void Enable(NetworkInstanceId spawnedBy, NetworkInstanceId spawnedByExtra, bool flipX) {
            this.spawnedBy = spawnedBy;
            this.spawnedByExtra = spawnedByExtra;
            this.flipX = flipX;
            Initialize();
            RpcInitialize(spawnedBy, spawnedByExtra, flipX, transform.position, transform.rotation);
        }

        void Initialize() {
            // Don't let this object hit the player that created it.
            if (spawnedBy != NetworkInstanceId.Invalid) {
                Item.IgnoreCollisions(gameObject, ClientScene.FindLocalObject(spawnedBy));
            }

            if (spawnedByExtra != NetworkInstanceId.Invalid) {
                Item.IgnoreCollisions(gameObject, ClientScene.FindLocalObject(spawnedByExtra));
            }

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.flipX = flipX;
            }

            // Due to what seems to be a bug in Unity,
            // collisions can be detected before Start is called,
            // so we initialize stuff on a non-colliding layer and
            // change it here.
            gameObject.layer = Layers.projectiles;
        }

        /// To be called by pooled objects when they are released.
        protected void Disable() {
            if (spawnedBy != NetworkInstanceId.Invalid) {
                Item.IgnoreCollisions(gameObject, ClientScene.FindLocalObject(spawnedBy), false);
            }

            if (spawnedByExtra != NetworkInstanceId.Invalid) {
                Item.IgnoreCollisions(gameObject, ClientScene.FindLocalObject(spawnedByExtra), false);
            }
        }
        
        public void SetInitialVelocity(Vector3 velocity) {
            initialVelocity = velocity;
        }

        public Vector3 GetInitialVelocity() {
            return initialVelocity;
        }

        public AttackType Type { get { return AttackType.Projectile; } }
        public virtual AttackProperty Properties { get { return AttackProperty.None; } }

        public virtual void Interact(IAttack attack) {}
    }
}