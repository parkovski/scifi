using UnityEngine;
using UnityEngine.Networking;

using SciFi.Items;
using SciFi.Environment.Effects;
using SciFi.Util;

namespace SciFi.Players.Attacks {
    public class CalcBook : MonoBehaviour, IAttackSource {
        public GameObject spawnedBy;
        public int power;

        bool attacking = false;
        AudioSource audioSource;

        HitSet hits;
        /// Keep track of the objects colliding with the book
        /// before it starts attacking, and issue a hit once
        /// attacking starts - this is because OnTriggerEnter2D
        /// will get called and do nothing, and the object won't get hit.
        ColliderCount colliderCount;

        public CalcBook() {
            hits = new HitSet();
            colliderCount = new ColliderCount();
        }

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
            audioSource = GetComponent<AudioSource>();
        }

        public void StartAttacking() {
            if (!NetworkServer.active) {
                return;
            }

            foreach (var obj in colliderCount.ObjectsWithPositiveCount) {
                Hit(obj);
            }
            attacking = true;
        }

        public void StopAttacking() {
            attacking = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }

        void Hit(GameObject obj) {
            var hit = Attack.GetAttackHit(obj.layer);
            if (hit == AttackHit.HitAndDamage && !hits.CheckOrFlag(obj)) {
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
                colliderCount.Increase(collider);
                return;
            }

            Hit(collider.gameObject);
        }

        void OnTriggerExit2D(Collider2D collider) {
            if (!NetworkServer.active) {
                return;
            }

            if (attacking) {
                return;
            }
            colliderCount.Decrease(collider);
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
        public Player Owner { get { return spawnedBy.GetComponent<Player>(); } }
    }
}