using System;

using SciFi.Util;

namespace SciFi.Players.Modifiers {
    public class ModifierMultiStateChange : FiniteAction {
        Player player;
        ModId[] modIds;

        public ModifierMultiStateChange(Player player, ModId[] modIds, Func<bool> shouldEnd)
            : base(player, 0.25f, shouldEnd)
        {
            this.player = player;
            this.modIds = modIds;
        }

        protected override void OnStart() {
            for (int i = 0; i < modIds.Length; i++) {
                player.AddModifier(modIds[i]);
            }
        }

        protected override void OnEnd() {
            for (int i = 0; i < modIds.Length; i++) {
                player.RemoveModifier(modIds[i]);
            }
        }
    }
}