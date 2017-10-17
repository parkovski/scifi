using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using SciFi.Environment.State;

namespace SciFi.AI.S2 {
    /// The value types contained here are public. Unfortunately,
    /// there is no way to make them publicly immutable without
    /// a lot of code duplication, so I'll just note here - these
    /// should never be changed through a reference to this type!
    public class AIEnvironment {
        public readonly int aiCount;

        /// Unity's `Time.time` is not thread-safe.
        public float time { get; private set; }
        /// Random is not thread safe, so each thread provides its own here.
        public System.Random threadRandom;

        private IStateSnapshotProvider<GameSnapshot> gameSp;
        public GameSnapshot gameState;

        private IStateSnapshotProvider<StageState> stageSp;
        public StageState stageState;

        private IStateSnapshotProvider<PlayerSnapshot>[] playersSp;
        public PlayerSnapshot[] playerState;

        public AIEnvironment(
            int aiCount,
            IStateSnapshotProvider<GameSnapshot> gameSp,
            IStateSnapshotProvider<StageState> stageSp,
            IEnumerable<IStateSnapshotProvider<PlayerSnapshot>> playersSp
        )
        {
            this.aiCount = aiCount;
            this.time = 0;
            this.gameSp = gameSp;
            this.stageSp = stageSp;
            this.playersSp = playersSp.ToArray();
            this.playerState = new PlayerSnapshot[this.playersSp.Length];
        }

        public AIEnvironment(AIEnvironment orig) {
            this.aiCount = orig.aiCount;
            this.gameSp = orig.gameSp;
            this.stageSp = orig.stageSp;
            this.playersSp = orig.playersSp;
            this.playerState = new PlayerSnapshot[this.playersSp.Length];
        }

        public void Update(float time) {
            this.time = time;
            gameSp.GetStateSnapshot(ref gameState);
            stageSp.GetStateSnapshot(ref stageState);
            for (int i = 0; i < playersSp.Length; i++) {
                playersSp[i].GetStateSnapshot(ref playerState[i]);
            }
        }
    }
}