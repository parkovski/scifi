using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Items {
    /// A projectile, spawned by either a player or an item,
    /// which cannot be held but otherwise interacts with players
    /// and other items.
    public class Projectile : NetworkBehaviour {
        /// The player or other object that spawned the projectile.
        /// Projectiles won't collide with objects that spawned them.
        [SyncVar]
        public NetworkInstanceId spawnedBy;
        /// If the player is holding an item, set this so the projectile doesn't hit it.
        [SyncVar]
        public NetworkInstanceId spawnedByExtra = NetworkInstanceId.Invalid;

        /// When an object collides, it bounces in the opposite direction
        /// sometimes causing knockback to go in the wrong direction.
        /// This remembers the first force applied and uses that for knockback.
        /// If an object is supposed to bounce, it will need to change this.
        [SyncVar]
        protected Vector3 initialForce;

        protected void BaseStart() {
            // Don't let this object hit the player that created it.
            Item.IgnoreCollisions(gameObject, ClientScene.FindLocalObject(spawnedBy));

            if (spawnedByExtra != NetworkInstanceId.Invalid) {
                Item.IgnoreCollisions(gameObject, ClientScene.FindLocalObject(spawnedByExtra));
            }

            // Due to what seems to be a bug in Unity,
            // collisions can be detected before Start is called,
            // so we initialize stuff on a non-colliding layer and
            // change it here.
            gameObject.layer = Layers.projectiles;
        }
        
        public void AddInitialForce(Vector3 force) {
            initialForce = force;
            GetComponent<Rigidbody2D>().AddForce(force);
        }

        public Vector3 GetInitialForce() {
            return initialForce;
        }
    }
}