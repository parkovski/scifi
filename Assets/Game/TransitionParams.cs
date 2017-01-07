namespace SciFi.Scenes {
    public enum GameType {
        Single,
        Multi,
    }

    public static class TransitionParams {
        #region Title screen -> Player picker
        public static GameType gameType = GameType.Single;
        #endregion

        #region Player picker -> Lobby
        public static string playerName = "Newton";
        #endregion
    }
}