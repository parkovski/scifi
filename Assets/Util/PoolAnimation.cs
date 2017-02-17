using UnityEngine;

namespace SciFi.Util {
    public class PoolAnimation : MonoBehaviour, IPoolNotificationHandler {
        void IPoolNotificationHandler.OnAcquire() {
            GetComponent<Animator>().enabled = true;
        }

        void IPoolNotificationHandler.OnRelease() {
            GetComponent<Animator>().enabled = false;
        }
    }
}