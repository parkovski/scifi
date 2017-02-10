namespace SciFi.Players.Modifiers {
    public abstract class SpeedModifier : Modifier {
        public abstract void Modify(uint modifierState, ref float maxSpeed, ref float force);
    }
}