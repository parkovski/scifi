using UnityEngine;
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

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
            hitObjects = new HashSet<GameObject>();
            audioSource = GetComponent<AudioSource>();
        }

        public void StopAttacking() {
            attacking = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }

        /// This can get called before Start -
        /// this seems like a bug :(.
        void OnTriggerEnter2D(Collider2D collider) {
            if (!attacking) {
                return;
            }

            var hit = Attack.GetAttackHit(collider.gameObject.layer);
            if (hit == AttackHit.HitAndDamage && !hitObjects.Contains(collider.gameObject)) {
                hitObjects.Add(collider.gameObject);
                GameController.Instance.Hit(collider.gameObject, this, spawnedBy, power * 2, power);
                audioSource.Play();
                Effects.Star(collider.bounds.ClosestPoint(transform.position));
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}