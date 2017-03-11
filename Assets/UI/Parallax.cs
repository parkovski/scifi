using UnityEngine;

namespace SciFi.UI {
    public class Parallax : MonoBehaviour {
        public bool scrollX;
        public bool scrollY;

        public new Camera camera;
        public GameObject foreground;
        public GameObject[] background;
        public float[] backgroundDistance;

        Vector3 oldCameraPosition;

        void Start() {
            for (int i = 0; i < background.Length; i++) {
                backgroundDistance[i] = 1 / backgroundDistance[i];
            }
            oldCameraPosition = camera.transform.position;
        }

        void Update() {
            var cameraPosition = camera.transform.position;
            if (cameraPosition == oldCameraPosition) {
                return;
            }
            var cameraDelta = oldCameraPosition - cameraPosition;
            oldCameraPosition = cameraPosition;

            for (int i = 0; i < background.Length; i++) {
                var scale = backgroundDistance[i];

                Vector2 position = background[i].transform.position;
                if (scrollX) {
                    position.x = cameraPosition.x * scale;
                }
                if (scrollY) {
                    position.y = cameraPosition.y * scale;
                }
                background[i].transform.position = position;
            }
        }
    }
}