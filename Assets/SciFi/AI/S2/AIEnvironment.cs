using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using SciFi.Environment.State;
using SciFi.Players;

namespace SciFi.AI.S2 {
    /// The value types contained here are public. Unfortunately,
    /// there is no way to make them publicly immutable without
    /// a lot of code duplication, so I'll just note here - these
    /// should never be changed through a reference to this type!
    public class AIEnvironment {
        /// Unity's `Time.time` is not thread-safe.
        public float time { get; private set; }
        /// Random is not thread safe, so each thread provides its own here.
        /// FIXME! This will not work! This is a shared object!
        public System.Random threadRandom;
        /// Note: This is shared, deal with that if it ever becomes mutable.
        int[] aiIdMap;

        private IStateSnapshotProvider<GameSnapshot> gameSp;
        public GameSnapshot game;

        private IStateSnapshotProvider<StageState> stageSp;
        public StageState stage;

        private IStateSnapshotProvider<PlayerSnapshot>[] playersSp;
        public PlayerSnapshot[] players;

        public AIEnvironment(
            IEnumerable<int> aiIdMap,
            IStateSnapshotProvider<GameSnapshot> gameSp,
            IStateSnapshotProvider<StageState> stageSp,
            IEnumerable<IStateSnapshotProvider<PlayerSnapshot>> playersSp
        )
        {
            this.time = 0;
            this.aiIdMap = aiIdMap.ToArray();
            this.gameSp = gameSp;
            this.stageSp = stageSp;
            this.playersSp = playersSp.ToArray();
            this.players = new PlayerSnapshot[this.playersSp.Length];
            for (var i = 0; i < this.players.Length; i++) {
                this.players[i].attacks = new AttackState[Player.attackCount];
            }
        }

        public AIEnvironment(AIEnvironment orig) {
            this.time = orig.time;
            this.aiIdMap = orig.aiIdMap;
            this.gameSp = orig.gameSp;
            this.stageSp = orig.stageSp;
            this.playersSp = orig.playersSp;
            this.players = new PlayerSnapshot[this.playersSp.Length];
            for (var i = 0; i < this.players.Length; i++) {
                this.players[i].attacks = new AttackState[Player.attackCount];
            }
        }

        public void Update(float time) {
            this.time = time;
            gameSp.GetStateSnapshot(ref game);
            stageSp.GetStateSnapshot(ref stage);
            for (int i = 0; i < playersSp.Length; i++) {
                playersSp[i].GetStateSnapshot(ref players[i]);
            }
        }

        public int AiCount() {
            return this.aiIdMap.Length;
        }

        public int AiPlayerId(int aiId) {
            return this.aiIdMap[aiId];
        }
    }
}