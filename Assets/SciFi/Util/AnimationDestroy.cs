using UnityEngine;

namespace SciFi.Util {
    /// Add a reference to <see cref="DestroyObject" /> as an animation
    /// event to destroy an object automatically.
    public class AnimationDestroy : MonoBehaviour {
        /// Destroy the object this is attached to. If it is
        /// in an object pool, release it instead of destroying it.
        public void DestroyObject() {
            var pooledObject = PooledObject.Get(gameObject);
            if (pooledObject != null) {
                pooledObject.Release();
            } else {
                Destroy(gameObject);
            }
        }

        public void HideObject() {
            GetComponent<SpriteRenderer>().enabled = false;
        }

        public void HideChildren() {
            for (var i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}