using SciFi.Players.Modifiers;

namespace SciFi.Players.Hooks {
    public static class StandardHooks {
        public static void Install(HookCollection hooks, ModifierCollection modifiers) {
            new StandardWalkForce().Install(hooks);

            (modifiers.CantMove.Hook = new CantMoveMaxSpeedHook()).Install(hooks);
            new StandardMaxSpeed().Install(hooks);
            (modifiers.Slow.Hook = new SlowMaxSpeedHook()).Install(hooks);
            (modifiers.Fast.Hook = new FastMaxSpeedHook()).Install(hooks);

            new StandardJumpForce().Install(hooks);
        }
    }
}