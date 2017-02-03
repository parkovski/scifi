using UnityEngine.Networking;

namespace SciFi.Players.Modifiers {
    public abstract class SpeedModifier : Modifier {
        public abstract void Modify(SyncListUInt modifiers, ref float maxSpeed, ref float force);
    }
}