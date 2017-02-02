using UnityEngine;

namespace SciFi.Players.Attacks {
    public class BoneArm : MonoBehaviour {
        public GameObject boneHandPrefab;
        public GameObject attachedHand;
        public bool alwaysThrowHand = false;

        [HideInInspector]
        public Player player;

        SpriteRenderer[] spriteRenderers;

        void Start() {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            Hide();
        }

        public void HandMaybeDetach() {
            if (Random.Range(0, 15) == 7
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
            ShowHide(true);
        }

        public void Hide() {
            ShowHide(false);
        }

        void ShowHide(bool show) {
            foreach (var sr in spriteRenderers) {
                sr.enabled = show;
            }
        }
    }
}