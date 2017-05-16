using UnityEngine;

namespace SciFi {
    public class EmptyStateChangeListener : IStateChangeListener {
        public virtual void GameStarted() {}
        public virtual void GameEnded() {}
        public virtual void ControlStateChanged(int control, bool active) {}
        public virtual void ControlStateChanged(int control, float amount) {}
        public virtual void PlayerPositionChanged(int playerId, Vector2 position) {}
        public virtual void DamageChanged(int playerId, int newDamage) {}
        public virtual void LifeChanged(int playerId, int newLives) {}
        public virtual void ObjectCreated(GameObject obj) {}
        public virtual void ObjectWillBeDestroyed(GameObject obj) {}
    }
}