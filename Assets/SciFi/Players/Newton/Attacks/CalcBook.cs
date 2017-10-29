using UnityEngine;
using UnityEngine.Networking;

using SciFi.Items;
using SciFi.Environment.Effects;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class CalcBook : MonoBehaviour, IAttackSource {
        public GameObject spawnedBy;
        public int power;

        bool attacking = false;
        AudioSource audioSource;

        HitSet hits;
        /// Keep track of the objects colliding with the this
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

        public void Show(bool show) {
            var collider = this.GetComponent<BoxCollider2D>();
            var animator = this.GetComponent<Animator>();
            var spriteRenderer = this.GetComponent<SpriteRenderer>();
            collider.enabled = show;
            animator.enabled = show;
            spriteRenderer.enabled = show;
            if (show) {
                var player = spawnedBy.GetComponent<Player>();
                this.transform.position = player.transform.position + new Vector3(1f, .5f).FlipDirection(player.eDirection);
                this.transform.localRotation =
                    player.eDirection == Direction.Left
                        ? Quaternion.Euler(0f, 0f, -20f)
                        : Quaternion.Euler(0f, 0f, 20f);
            }
        }

        /// To expose to the animator. Also resets hit counts.
        public void Hide() {
            Show(false);
            hits.Clear();
            colliderCount.Clear();
        }

        public void StartAttacking() {
            if (!NetworkServer.active) {
                return;
            }

            foreach (var obj in colliderCount.ObjectsWithPositiveCount) {
                Hit(obj);
            }
            colliderCount.Clear();
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