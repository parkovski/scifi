namespace SciFi.Players.Modifiers {
    /// An empty marker modifier that just stores its ID.
    public class Marker : Modifier {
        readonly ModId id;

        public override ModId Id { get { return id; } }

        public Marker(ModId id) {
            this.id = id;
        }
    }
}