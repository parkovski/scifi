using UnityEngine;

using SciFi.Environment.Effects;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class BoneArm : MonoBehaviour, IAttackSource {
        public GameObject boneHandPrefab;
        public GameObject attachedHand;
        public bool alwaysThrowHand = false;

        [HideInInspector]
        public Player player;

        Collider2D attachedHandCollider;
        SpriteRenderer[] spriteRenderers;
        bool isActive;
        bool isAttacking;
        int power;
        HitSet hits;
        ColliderCount colliderCount;

        void Start() {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            attachedHandCollider = attachedHand.GetComponent<Collider2D>();
            hits = new HitSet();
            colliderCount = new ColliderCount();
            Hide();
        }

        public void HandMaybeDetach() {
            if (Random.Range(0, 3) == 1
#if UNITY_EDITOR
                || alwaysThrowHand
#endif
            ) {
                spriteRenderers[spriteRenderers.Length - 1].enabled = false;
                attachedHandCollider.enabled = false;
                player.CmdSpawnProjectileFlipped(
                    GameController.PrefabToIndex(boneHandPrefab),
                    attachedHand.transform.position,
                    attachedHand.transform.rotation,
                    new Vector2(7f, 2.5f).FlipDirection(player.eDirection),
                    -(500.FlipDirection(player.eDirection)),
                    player.eDirection == Direction.Right
                );
            }
        }

        public void StartAttacking(int power) {
            this.power = power;
            isAttacking = true;
            foreach (var obj in colliderCount.ObjectsWithPositiveCount) {
                Hit(obj);
            }
        }

        public void Show() {
            hits.Clear();
            colliderCount.Clear();
            attachedHandCollider.enabled = true;
            ShowHide(true);
        }

        public void Hide() {
            ShowHide(false);
        }

        void ShowHide(bool show) {
            isActive = show;
            if (!show) {
                isAttacking = false;
            }
            foreach (var sr in spriteRenderers) {
                sr.enabled = show;
            }
        }

        void Hit(GameObject obj) {
            var hit = Attack.GetAttackHit(obj.layer);
            if (hit != AttackHit.None && !hits.CheckOrFlag(obj)) {
                var knockback = ((float)power).Scale(1, 10, 5, 10);
                GameController.Instance.Hit(obj, this, player.gameObject, (int)knockback, knockback);
                Effects.Star(obj.transform.position);
            }
        }

        public void ChildCollide(GameObject child, Collider2D collider) {
            if (!isActive) {
                return;
            }

            if (collider.gameObject == player.gameObject) {
                return;
            }

            if (!isAttacking) {
                colliderCount.Increase(collider);
                return;
            }

            Hit(collider.gameObject);
        }

        public void ChildEndCollide(GameObject child, Collider2D collider) {
            if (!isAttacking) {
                colliderCount.Decrease(collider);
            }
        }

        // IAttack implementation
        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}