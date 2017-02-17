using UnityEngine;

using SciFi.Network;

namespace SciFi {
    /// A non-networked object managed by the object pool.
    /// Do not use GetComponent to get this. Use PooledObject.Get.
    public class PooledObject : MonoBehaviour, IPooledObject {
        /// The editor doesn't let you assign interfaces,
        /// so we have to do it this way - just make sure
        /// to assign something that's actually an IPoolNotificationHandler.
        public MonoBehaviour notificationHandlerComponent;

        IPoolNotificationHandler notificationHandler;

        /// Objects are considered acquired when first created,
        /// so OnAcquire is not called until it has been released first.
        bool isFree = false;

        void Start() {
            notificationHandler = (IPoolNotificationHandler)notificationHandlerComponent;
        }

        /// Marks the object as not free in the pool.
        public void Acquire() {
            if (!isFree) {
                return;
            }

            isFree = false;
            if (notificationHandler != null) {
                notificationHandler.OnAcquire();
            }
        }

        /// Marks the object as free in the pool.
        public void Release() {
            if (isFree) {
                return;
            }

            isFree = true;
            if (notificationHandler != null) {
                notificationHandler.OnRelease();
            }
        }

        public bool IsFree() {
            return isFree;
        }

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
    }

    public interface IPoolNotificationHandler {
        void OnAcquire();
        void OnRelease();
    }
}