using UnityEngine;

namespace SciFi.Players.Attacks {
    public class PaintbrushAttack : Attack {
        Animator animator;

        public PaintbrushAttack(Player player, Paintbrush paintbrush)
            : base(player, 1f, false)
        {
            paintbrush.GetComponent<Paintbrush>().player = (daVinci)player;
            this.animator = paintbrush.GetComponent<Animator>();
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            if (direction == Direction.Left) {
                animator.SetTrigger("SwingLeft");
            } else {
                animator.SetTrigger("SwingRight");
            }
        }

        public override void OnCancel() {
        }
    }
}