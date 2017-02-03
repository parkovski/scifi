using UnityEngine.Networking;

namespace SciFi.Players.Modifiers {
    public class Slow : SpeedModifier {
        public override ModId Id { get { return ModId.Slow; } }

        public override void Modify(SyncListUInt modifiers, ref float maxSpeed, ref float force) {
            if (!IsEnabled(modifiers)) {
                return;
            }

            maxSpeed /= 2f;
            force /= 1.5f;
        }
    }
}