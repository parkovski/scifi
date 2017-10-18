namespace SciFi.AI.S2 {
    public static class StrategySets {
        public static Strategy[] GetStandardSet(int aiId, AIInputManager inputManager) {
            return new Strategy[] {
                new StayOnStage(aiId, inputManager),
                new Wander(aiId, inputManager)
            };
        }
    }
}