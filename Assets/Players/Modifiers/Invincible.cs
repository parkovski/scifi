using UnityEngine;

namespace SciFi.Players.Modifiers {
    public class Invincible : Modifier {
        public override ModId Id { get { return ModId.Invincible; } }

        public void TryAddKnockback(uint modifierState, Rigidbody2D rb, Vector2 force) {
            if (IsEnabled(modifierState)) {
                return;
            }

            rb.AddForce(force);
        }
    }
}