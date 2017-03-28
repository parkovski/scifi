using System;

using SciFi.Util;

namespace SciFi.Players.Modifiers {
    /// Adds a modifier state with a condition
    /// for removal so it can't be forgotten.
    public class ModifierStateChange : FiniteAction {
        Player player;
        ModId modId;

        public ModifierStateChange(Player player, ModId modId, Func<bool> shouldEnd)
            : base(player, 0.25f, shouldEnd)
        {
            this.player = player;
            this.modId = modId;
        }

        protected override void OnStart() {
            player.AddModifier(modId);
        }

        protected override void OnEnd() {
            player.RemoveModifier(modId);
        }
    }
}