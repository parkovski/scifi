using UnityEngine.Networking;

namespace SciFi.Items {
    public class Projectile : NetworkBehaviour {
        [SyncVar]
        public NetworkInstanceId spawnedBy;
        /// If the player is holding an item, set this so the projectile doesn't hit it.
        [SyncVar]
        public NetworkInstanceId spawnedByExtra = NetworkInstanceId.Invalid;

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
    }
}