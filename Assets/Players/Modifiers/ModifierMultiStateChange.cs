using System;

using SciFi.Util;

namespace SciFi.Players.Modifiers {
    public class ModifierMultiStateChange : FiniteAction<Player> {
        ModId[] modIds;

        public ModifierMultiStateChange(Player player, ModId[] modIds, Func<bool> shouldEnd)
            : base(player, 0.25f, shouldEnd)
        {
            this.modIds = modIds;
        }

        protected override void OnStart() {
            for (int i = 0; i < modIds.Length; i++) {
                coroutineRunner.AddModifier(modIds[i]);
            }
        }

        protected override void OnEnd() {
            for (int i = 0; i < modIds.Length; i++) {
                coroutineRunner.RemoveModifier(modIds[i]);
            }
        }
    }
}