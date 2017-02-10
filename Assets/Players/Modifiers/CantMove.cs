using UnityEngine;

namespace SciFi.Players.Modifiers {
    public class CantMove : Modifier {
        public override ModId Id { get { return ModId.CantMove; } }

        public void TryMove(uint modifierState, Rigidbody2D rb, Vector2 force) {
            if (IsEnabled(modifierState)) {
                return;
            }

            rb.AddForce(force);
        }
    }
}