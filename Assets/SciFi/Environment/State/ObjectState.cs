using UnityEngine;

using SciFi.Players.Attacks;

namespace SciFi.Environment.State {
    public struct ObjectState {
        public Vector2 position;
        public Vector2 velocity;
        public short damage;
        public short maxDamage;
        public bool isFreeObject;
        public bool isDamageIncreasing;
        public bool isOneHitDestroy;
        public AttackProperty properties;
    }
}