using UnityEngine;

using SciFi.Network;

namespace SciFi {
    /// A non-networked object managed by the object pool.
    /// Do not use GetComponent to get this. Use PooledObject.Get.
    public class PooledObject : MonoBehaviour, IPooledObject {
        public IPoolNotificationHandler notificationHandler;

        /// Objects are considered acquired when first created,
        /// so OnAcquire is not called until it has been released first.
        bool isFree = false;

        /// Marks the object as not free in the pool.
        public void Acquire() {
            isFree = false;
            if (notificationHandler != null) {
                notificationHandler.OnAcquire();
            }
        }

        /// Marks the object as free in the pool.
        public void Release() {
            isFree = true;
            if (notificationHandler != null) {
                notificationHandler.OnRelease();
            }
        }

        public bool IsFree() {
            return isFree;
        }

        public GameObject GameObject { get { return gameObject; } }

        /// Gets the NetworkPooledObject or PooledObject component,
        /// whichever is attached.
        public static IPooledObject Get(GameObject obj) {
            var netPool = obj.GetComponent<NetworkPooledObject>();
            if (netPool != null) {
                return netPool;
            }
            return obj.GetComponent<PooledObject>();
        }
    }

    public interface IPooledObject {
        void Acquire();
        void Release();
        bool IsFree();
        GameObject GameObject { get; }
    }

    public interface IPoolNotificationHandler {
        void OnAcquire();
        void OnRelease();
    }
}