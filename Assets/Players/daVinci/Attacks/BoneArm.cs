using UnityEngine;
using System.Collections.Generic;

using SciFi.Environment.Effects;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class BoneArm : MonoBehaviour, IAttack {
        public GameObject boneHandPrefab;
        public GameObject attachedHand;
        public bool alwaysThrowHand = false;

        [HideInInspector]
        public Player player;

        Collider2D attachedHandCollider;
        SpriteRenderer[] spriteRenderers;
        bool isActive;
        HashSet<GameObject> hitObjects;

        void Start() {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            attachedHandCollider = attachedHand.GetComponent<Collider2D>();
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
                attachedHandCollider.enabled = false;
                player.CmdSpawnProjectileFlipped(
                    GameController.PrefabToIndex(boneHandPrefab),
                    attachedHand.transform.position,
                    attachedHand.transform.rotation,
                    new Vector2(350f, 50f).FlipDirection(player.eDirection),
                    -(3.FlipDirection(player.eDirection)),
                    player.eDirection == Direction.Right
                );
            }
        }

        public void Show() {
            hitObjects.Clear();
            attachedHandCollider.enabled = true;
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
                GameController.Instance.Hit(collider.gameObject, this, player.gameObject, 5, 5f);
            }
        }

        // IAttack implementation
        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}