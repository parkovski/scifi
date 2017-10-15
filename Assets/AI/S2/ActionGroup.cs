namespace SciFi.AI.S2 {
    /// An action group defines a set of decisions where only
    /// one may be chosen at a time. Each action group may run
    /// concurrently, but only one action from each group may
    /// be taken at one time.
    public static class ActionGroup {
        public const uint Count     = 5;
        public const uint All       = (1 << (int)ActionGroup.Count) - 1;
        public const uint Invalid   = 0xFFFFFFFF;

        public const uint Movement  = 1 << 0;
        public const uint Attack    = 1 << 1;
        public const uint Jump      = 1 << 2;
        public const uint Block     = 1 << 3;
        public const uint ItemSel   = 1 << 4;
    }
}