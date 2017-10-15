namespace SciFi.Environment.State {
    public struct GameState {
        public int[] aiPlayerIds;
        public StageState stage;
    }

    public struct GameSnapshot {
        public int aiCount;
        public StageState stage;
    }
}