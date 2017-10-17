namespace SciFi.AI.S2 {
    public static class StrategySets {
        public static Strategy[] GetStandardSet(int aiIndex) {
            return new Strategy[] {
                new StayOnStage(aiIndex),
                new Wander(aiIndex)
            };
        }
    }
}