using UnityEngine.Networking;

namespace SciFi.Players.Modifiers {
    public class Fast : SpeedModifier {
        public override ModId Id { get { return ModId.Fast; } }
        public override void Modify(SyncListUInt modifiers, ref float maxSpeed, ref float force) {
            if (!IsEnabled(modifiers)) {
                return;
            }

            maxSpeed *= 1.5f;
            force *= 1.5f;
        }
    }
}
