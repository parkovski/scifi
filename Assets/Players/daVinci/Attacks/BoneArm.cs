using UnityEngine;
using System.Collections.Generic;

using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class BoneArm : MonoBehaviour, IAttack {
        public GameObject boneHandPrefab;
        public GameObject attachedHand;
        public bool alwaysThrowHand = false;

        [HideInInspector]
        public Player player;

        SpriteRenderer[] spriteRenderers;
        bool isActive;
        HashSet<GameObject> hitObjects;

        void Start() {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            hitObjects = new HashSet<GameObject>();
            Hide();
        }

        public void HandMaybeDetach() {
            if (Random.Range(0, 3) == 1
#if UNITY_EDITOR
                || alwaysThrowHand
#endif
            ) {
                spriteRenderers[spriteRenderers.Length - 1].enabled = false;
                var detachedHandGo = Instantiate(boneHandPrefab, attachedHand.transform.position, attachedHand.transform.rotation);
                Vector2 force;
                float torque;
                if (player.eDirection == Direction.Left) {
                    force = new Vector2(-350f, 50f);
                    torque = 3;
                } else {
                    force = new Vector2(350f, 50f);
                    torque = -3;
                    detachedHandGo.GetComponent<SpriteRenderer>().flipX = true;
                }
                player.CmdSpawnCustomProjectile(detachedHandGo, force, torque);
            }
        }

        public void Show() {
            hitObjects.Clear();
            ShowHide(true);
        }

        public void Hide() {
            ShowHide(false);
        }

        void ShowHide(bool show) {
            isActive = show;
            foreach (var sr in spriteRenderers) {
                sr.enabled = show;
            }
        }

        public void ChildCollide(GameObject child, Collider2D collider) {
            if (!isActive) {
                return;
            }

            if (collider.gameObject == player.gameObject) {
                return;
            }

            if (Attack.GetAttackHit(collider.gameObject.layer) != AttackHit.None) {
                if (hitObjects.Contains(collider.gameObject)) {
                    return;
                }
                hitObjects.Add(collider.gameObject);
                Effects.Star(collider.bounds.ClosestPoint(collider.gameObject.transform.position));
                GameController.Instance.Hit(collider.gameObject, this, player.gameObject, 5, 3.5f);
            }
        }

        // IAttack implementation
        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}