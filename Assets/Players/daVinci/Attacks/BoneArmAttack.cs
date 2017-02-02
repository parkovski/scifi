using UnityEngine;
using System.Linq;

namespace SciFi.Players.Attacks {
    public class BoneArmAttack : Attack {
        GameObject boneArm;
        SpriteRenderer[] spriteRenderers;

        public BoneArmAttack(Player player, GameObject boneArm)
            : base(player, false)
        {
            this.boneArm = boneArm;
            spriteRenderers
                = Enumerable.Range(0, boneArm.transform.childCount)
                .Select(i => boneArm.transform.GetChild(i).GetComponent<SpriteRenderer>())
                .ToArray();
            ShowHide(false);
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            ShowHide(true);
            player.StartCoroutine(Hide());
        }

        System.Collections.IEnumerator Hide() {
            yield return new WaitForSeconds(1f);
            ShowHide(false);
        }

        void ShowHide(bool show) {
            foreach (var sr in spriteRenderers) {
                sr.enabled = show;
            }
        }
    }
}