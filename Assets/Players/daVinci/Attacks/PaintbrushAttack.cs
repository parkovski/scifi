using UnityEngine;

namespace SciFi.Players.Attacks {
    public class PaintbrushAttack : Attack {
        Paintbrush paintbrush;
        Animator animator;

        public PaintbrushAttack(Player player, Paintbrush paintbrush)
            : base(player, true)
        {
            this.paintbrush = paintbrush;
            this.animator = paintbrush.GetComponent<Animator>();
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            paintbrush.SetDirection(direction);
            paintbrush.SetPower(Mathf.Clamp((int)(chargeTime * 10f), 1, 10));
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