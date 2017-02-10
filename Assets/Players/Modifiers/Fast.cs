namespace SciFi.Players.Modifiers {
    public class Fast : SpeedModifier {
        public override ModId Id { get { return ModId.Fast; } }
        public override void Modify(uint modifierState, ref float maxSpeed, ref float force) {
            if (!IsEnabled(modifierState)) {
                return;
            }

            maxSpeed *= 1.5f;
            force *= 1.5f;
        }
    }
}
