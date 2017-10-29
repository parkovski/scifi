using UnityEngine;

namespace SciFi.Util {
    public class CopyStartPosition : MonoBehaviour {
        public GameObject positionSource;

        void Start() {
            transform.position = positionSource.transform.position;
        }
    }
}