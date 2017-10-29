using UnityEngine;

namespace SciFi.Util {
    public class AnimationStart : MonoBehaviour {
        public string triggerName;

        void Start() {
            GetComponent<Animator>().SetTrigger(triggerName);
        }
    }
}