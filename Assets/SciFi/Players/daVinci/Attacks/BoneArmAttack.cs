using UnityEngine;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class BoneArmAttack : Attack {
        BoneArm boneArm;
        Animator animator;
        SpriteRenderer[] spriteRenderers;

        public BoneArmAttack(Player player, BoneArm boneArm)
            : base(player, 1f, true)
        {
            boneArm.player = player;
            this.boneArm = boneArm;
            this.animator = boneArm.GetComponent<Animator>();
        }

        public override void OnBeginCharging(Direction direction) {
            animator.ResetTrigger("ChargeLeft");
            animator.ResetTrigger("ChargeRight");
            animator.ResetTrigger("SwingLeft");
            animator.ResetTrigger("SwingRight");
            animator.ResetTrigger("Cancel");
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Empty")) {
                RequestCancel();
                return;
            }

            boneArm.Show();
            if (direction == Direction.Left) {
                animator.SetTrigger("ChargeLeft");
            } else {
                animator.SetTrigger("ChargeRight");
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var power = (int)Mathf.Clamp(chargeTime, 0.1f, 1.5f).Scale(0.1f, 1.5f, 1f, 10f);
            boneArm.StartAttacking(power);
            if (direction == Direction.Left) {
                animator.SetTrigger("SwingLeft");
            } else {
                animator.SetTrigger("SwingRight");
            }
        }

        public override void OnCancel() {
            if (!IsCharging) {
                return;
            }
            animator.SetTrigger("Cancel");
            boneArm.Hide();
        }
    }
}