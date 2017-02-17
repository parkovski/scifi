using UnityEngine;

namespace SciFi.Util {
    public class PoolAnimation : MonoBehaviour, IPoolNotificationHandler {
        void IPoolNotificationHandler.OnAcquire() {
            var animator = GetComponent<Animator>();
            animator.enabled = true;
            animator.Rebind();
        }

        void IPoolNotificationHandler.OnRelease() {
            var animator = GetComponent<Animator>();
            animator.Stop();
            animator.enabled = false;
        }
    }
}