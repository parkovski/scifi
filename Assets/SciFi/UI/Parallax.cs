using UnityEngine;

namespace SciFi.UI {
    public class Parallax : MonoBehaviour {
        public bool scrollX;
        public bool scrollY;

        public new Camera camera;
        public GameObject foreground;
        public GameObject[] background;
        public float[] backgroundDistance;
        
        float[] xOffset;
        float[] yOffset;

        Vector3 oldCameraPosition;

        void Start() {
            xOffset = new float[backgroundDistance.Length];
            yOffset = new float[backgroundDistance.Length];
            for (int i = 0; i < background.Length; i++) {
                backgroundDistance[i] = 1 / backgroundDistance[i];
                xOffset[i] = background[i].transform.position.x;
                yOffset[i] = background[i].transform.position.y;
            }
            oldCameraPosition = camera.transform.position;
        }

        void LateUpdate() {
            var cameraPosition = camera.transform.position;
            if (cameraPosition == oldCameraPosition) {
                return;
            }
            oldCameraPosition = cameraPosition;

            for (int i = 0; i < background.Length; i++) {
                var scale = backgroundDistance[i];

                Vector2 position = background[i].transform.position;
                if (scrollX) {
                    position.x = xOffset[i] + cameraPosition.x * scale;
                } else {
                    position.x = xOffset[i] + cameraPosition.x;
                }
                if (scrollY) {
                    position.y = yOffset[i] + cameraPosition.y * scale;
                } else {
                    position.y = yOffset[i] + cameraPosition.y;
                }
                background[i].transform.position = position;
            }
        }
    }
}