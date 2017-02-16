using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class CalcBook : MonoBehaviour, IAttack {
        public GameObject spawnedBy;
        public int power;

        [HideInInspector]
        public bool attacking = false;
        AudioSource audioSource;

        HashSet<GameObject> hitObjects;
        /// Keep track of the objects colliding with the book
        /// before it starts attacking, and issue a hit once
        /// attacking starts - this is because OnTriggerEnter2D
        /// will get called and do nothing, and the object won't get hit.
        Dictionary<GameObject, int> chargingColliderCount;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
            audioSource = GetComponent<AudioSource>();
        }

        void InitCollections() {
            if (hitObjects == null) {
                hitObjects = new HashSet<GameObject>();
            }
            if (chargingColliderCount == null) {
                chargingColliderCount = new Dictionary<GameObject, int>();
            }
        }

        public void StartAttacking() {
            if (!NetworkServer.active) {
                return;
            }

            InitCollections();
            foreach (var pair in chargingColliderCount) {
                if (pair.Value <= 0) {
                    continue;
                }
                if (pair.Key != null) {
                    Hit(pair.Key);
                }
            }
            attacking = true;
        }

        public void StopAttacking() {
            attacking = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }

        void Hit(GameObject obj) {
            InitCollections();
            var hit = Attack.GetAttackHit(obj.layer);
            if (hit == AttackHit.HitAndDamage && !hitObjects.Contains(obj)) {
                hitObjects.Add(obj);
                GameController.Instance.Hit(obj, this, spawnedBy, power * 2, power);
                audioSource.Play();
                Effects.Star(obj.transform.position);
            }
        }

        /// This can get called before Start -
        /// this seems like a bug :(.
        void OnTriggerEnter2D(Collider2D collider) {
            if (!NetworkServer.active) {
                return;
            }

            if (!attacking) {
                InitCollections();
                int colliderCount;
                if (chargingColliderCount.TryGetValue(collider.gameObject, out colliderCount)) {
                    chargingColliderCount[collider.gameObject] = colliderCount + 1;
                } else {
                    chargingColliderCount[collider.gameObject] = 1;
                }
                return;
            }

            Hit(collider.gameObject);
        }

        void OnTriggerExit2D(Collider2D collider) {
            if (!NetworkServer.active) {
                return;
            }

            if (attacking || chargingColliderCount == null) {
                return;
            }
            int colliderCount;
            if (chargingColliderCount.TryGetValue(collider.gameObject, out colliderCount)) {
                if (colliderCount > 0) {
                    chargingColliderCount[collider.gameObject] = colliderCount - 1;
                }
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}