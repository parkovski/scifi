using UnityEngine;

namespace SciFi.Util {
    public class AnimationDestroy : MonoBehaviour {
        public void DestroyObject() {
            Destroy(gameObject);
        }
    }
}