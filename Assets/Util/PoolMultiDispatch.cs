using UnityEngine;
using System.Linq;

namespace SciFi.Util {
    public class PoolMultiDispatch : MonoBehaviour, IPoolNotificationHandler {
        public MonoBehaviour[] notificationHandlerComponents;

        IPoolNotificationHandler[] notificationHandlers;

        void Start() {
            notificationHandlers = notificationHandlerComponents.Cast<IPoolNotificationHandler>().ToArray();
            notificationHandlerComponents = null;
        }

        void IPoolNotificationHandler.OnAcquire() {
            foreach (var nh in notificationHandlers) {
                nh.OnAcquire();
            }
        }

        void IPoolNotificationHandler.OnRelease() {
            foreach (var nh in notificationHandlers) {
                nh.OnRelease();
            }
        }
    }
}