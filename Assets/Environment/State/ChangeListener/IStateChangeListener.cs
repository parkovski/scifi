using UnityEngine;

namespace SciFi {
    public interface IStateChangeListener {
        void GameStarted();
        void GameEnded();
        void ControlStateChanged(int control, bool active);
        void ControlStateChanged(int control, float amount);
        void PlayerPositionChanged(int playerId, Vector2 position);
        void DamageChanged(int playerId, int newDamage);
        void LifeChanged(int playerId, int newLives);
        void ObjectCreated(GameObject obj);
        void ObjectWillBeDestroyed(GameObject obj);
    }
}