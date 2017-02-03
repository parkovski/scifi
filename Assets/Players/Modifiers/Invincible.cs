using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Players.Modifiers {
    public class Invincible : Modifier {
        public override ModId Id { get { return ModId.Invincible; } }

        public void TryAddKnockback(SyncListUInt modifiers, Rigidbody2D rb, Vector2 force) {
            if (IsEnabled(modifiers)) {
                return;
            }

            rb.AddForce(force);
        }
    }
}