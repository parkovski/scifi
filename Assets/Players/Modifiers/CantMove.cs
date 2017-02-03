using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Players.Modifiers {
    public class CantMove : Modifier {
        public override ModId Id { get { return ModId.CantMove; } }

        public void TryMove(SyncListUInt modifiers, Rigidbody2D rb, Vector2 force) {
            if (IsEnabled(modifiers)) {
                return;
            }

            rb.AddForce(force);
        }
    }
}