using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Items {
    public class GreenApple : Projectile {
        public GameObject explodingApple;

        public Quaternion explodeRotation;

        GameObject owner;
        [SyncVar]
        Vector3 ownerOffset;

        void Start() {
            owner = ClientScene.FindLocalObject(spawnedBy);
            this.ownerOffset = gameObject.transform.position - owner.transform.position;
        }

        [Server]
        public void Explode() {
            var exploding = Instantiate(explodingApple, gameObject.transform.position, explodeRotation);
            NetworkServer.Spawn(exploding);
            Destroy(gameObject);
        }

        void Update() {
            this.transform.position = owner.transform.position + ownerOffset;
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            var tag = collision.gameObject.tag;
            if (tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, 8);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 3f);
                Explode();
            }
        }
    }
}