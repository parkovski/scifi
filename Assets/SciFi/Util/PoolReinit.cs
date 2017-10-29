using UnityEngine;

namespace SciFi.Util {
    public class PoolReinit : MonoBehaviour, IPoolNotificationHandler {
        void IPoolNotificationHandler.OnAcquire() {
            PooledObject.Enable(gameObject);
        }

        void IPoolNotificationHandler.OnRelease() {
            PooledObject.Disable(gameObject);
        }
    }
}