using UnityEngine;
using System.Collections.Generic;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class CalcBook : MonoBehaviour {
        public GameObject spawnedBy;
        public int power;
        public bool attacking = false;

        HashSet<GameObject> hitObjects;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
            hitObjects = new HashSet<GameObject>();
        }

        /// This can get called before Start -
        /// this seems like a bug :(.
        void OnTriggerEnter2D(Collider2D collider) {
            if (!attacking) {
                return;
            }

            if (collider.gameObject.tag == "Player" && !hitObjects.Contains(collider.gameObject)) {
                hitObjects.Add(collider.gameObject);
                GameController.Instance.TakeDamage(collider.gameObject, power * 2);
                GameController.Instance.Knockback(spawnedBy, collider.gameObject, power);
                Effects.Star(collider.bounds.ClosestPoint(transform.position));
            }
        }
    }
}