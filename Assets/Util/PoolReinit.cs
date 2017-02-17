using UnityEngine;

namespace SciFi.Util {
    public class PoolReinit : MonoBehaviour, IPoolNotificationHandler {
        public enum ComponentLocation {
            None,
            OnObject,
            InChildren,
        }

        public ComponentLocation spriteRendererLocation = ComponentLocation.OnObject;
        public ComponentLocation colliderLocation = ComponentLocation.OnObject;

        void SetState(bool enabled) {
            if (spriteRendererLocation == ComponentLocation.InChildren) {
                var srs = GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in srs) {
                    sr.enabled = enabled;
                }
            } else if (spriteRendererLocation == ComponentLocation.OnObject) {
                GetComponent<SpriteRenderer>().enabled = enabled;
            }

            if (colliderLocation == ComponentLocation.InChildren) {
                var colls = GetComponentsInChildren<Collider2D>();
                foreach (var coll in colls) {
                    coll.enabled = enabled;
                }
            } else if (colliderLocation == ComponentLocation.OnObject) {
                var colls = GetComponents<Collider2D>();
                foreach (var coll in colls) {
                    coll.enabled = enabled;
                }
            }

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.isKinematic = !enabled;
                if (!enabled) {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0;
                }
            }
        }

        void IPoolNotificationHandler.OnAcquire() {
            SetState(true);
        }

        void IPoolNotificationHandler.OnRelease() {
            SetState(false);
        }
    }
}