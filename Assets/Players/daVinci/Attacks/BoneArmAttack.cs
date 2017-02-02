using UnityEngine;

namespace SciFi.Players.Attacks {
    public class BoneArmAttack : Attack {
        BoneArm boneArm;
        Animator animator;
        SpriteRenderer[] spriteRenderers;

        public BoneArmAttack(Player player, BoneArm boneArm)
            : base(player, false)
        {
            boneArm.player = player;
            this.boneArm = boneArm;
            this.animator = boneArm.GetComponent<Animator>();
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            boneArm.Show();
            if (direction == Direction.Left) {
                animator.SetTrigger("SwingLeft");
            } else {
                animator.SetTrigger("SwingRight");
            }
        }
    }
}