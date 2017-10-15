using UnityEngine;

namespace SciFi.Environment.State {
    public struct PlayerState {
        public short lives;
        public short damage;
        public sbyte magic;
        public const sbyte MaxMagic = 100;
    }

    public struct PlayerSnapshot {
        public short lives;
        public short damage;
        public sbyte magic;
        public Vector2 position;
        public Vector2 velocity;
        public AttackState attack1;
        public AttackState attack2;
        public AttackState attack3;
        public AttackState? item;
    }
}