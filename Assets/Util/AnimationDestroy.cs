using UnityEngine;

namespace SciFi.Util {
    /// Add a reference to <see cref="DestroyObject" /> as an animation
    /// event to destroy an object automatically.
    public class AnimationDestroy : MonoBehaviour {
        /// Destroy the object this is attached to.
        public void DestroyObject() {
            Destroy(gameObject);
        }
    }
}