using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    /// A networked object managed by the pool.
    /// Do not use GetComponent to get this - use PooledObject.Get.
    public class NetworkPooledObject : NetworkBehaviour, IPooledObject {
        /// The editor doesn't let you assign interfaces,
        /// so we have to do it this way - just make sure
        /// to assign something that's actually an IPoolNotificationHandler.
        public MonoBehaviour notificationHandlerComponent;

        IPoolNotificationHandler notificationHandler;

        bool isFree = true;

        void Start() {
            notificationHandler = (IPoolNotificationHandler)notificationHandlerComponent;
        }

        /// Marks the object as not free in the pool.
        /// Does nothing when called not on the server.
        /// Define DEBUG_NETPOOL to see warnings when this happens.
        public void Acquire() {
            if (!isServer) {
#if DEBUG_NETPOOL
                Debug.LogWarning("Acquire called not on server");
#endif
                return;
            }
            if (!isFree) {
                return;
            }
            isFree = false;
            if (notificationHandler != null) {
                notificationHandler.OnAcquire();
            }
            RpcAcquire();
        }

        /// Marks the object as free in the pool.
        /// Does nothing when called not on the server.
        /// Define DEBUG_NETPOOL to see warnings when this happens.
        public void Release() {
            if (!isServer) {
#if DEBUG_NETPOOL
                Debug.LogWarning("Release called not on server");
#endif
                return;
            }
            if (isFree) {
                return;
            }
            isFree = true;
            if (notificationHandler != null) {
                notificationHandler.OnRelease();
            }
            RpcRelease();
        }

        [ClientRpc]
        void RpcAcquire() {
            if (isServer) {
                return;
            }

            isFree = false;
            if (notificationHandler != null) {
                notificationHandler.OnAcquire();
            }
        }

        [ClientRpc]
        void RpcRelease() {
            if (isServer) {
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
    }
}