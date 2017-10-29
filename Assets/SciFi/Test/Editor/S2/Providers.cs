using SciFi.AI.S2;
using SciFi.Environment.State;

namespace SciFi.Test {
    public class DummyGameSnapshotProvider : IStateSnapshotProvider<GameSnapshot> {
        public void GetStateSnapshot(ref GameSnapshot snapshot) {}
    }

    public class DummyStageSnapshotProvider : IStateSnapshotProvider<StageState> {
        private float halfStageWidth;

        public DummyStageSnapshotProvider(float stageWidth) {
            this.halfStageWidth = stageWidth / 2;
        }

        public void GetStateSnapshot(ref StageState snapshot) {
            snapshot.leftEdge = -halfStageWidth;
            snapshot.rightEdge = halfStageWidth;
        }
    }

    public class DummyPlayerSnapshotProvider : IStateSnapshotProvider<PlayerSnapshot> {
        public float x;
        public float y;
        public float vx;
        public float vy;

        public void Move(float dx, float dy = 0) {
            this.x += dx;
            this.y += dy;
            this.vx = dx;
            this.vy = dy;
        }

        public void GetStateSnapshot(ref PlayerSnapshot snapshot) {
            snapshot.position.x = this.x;
            snapshot.position.y = this.y;
            snapshot.velocity.x = this.vx;
            snapshot.velocity.y = this.vy;
        }
    }
}