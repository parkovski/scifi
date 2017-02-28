using UnityEngine;

using SciFi.Players;

namespace SciFi.UI {
    public class CameraScroll : MonoBehaviour {
        public Player playerToFollow;
        public new Camera camera;
        public SpriteRenderer backgroundRenderer;

        float minX;
        float maxX;
        float minY;
        float maxY;

        void Start() {
            float screenHeight = camera.orthographicSize * 2;
            float screenWidth = screenHeight * Screen.width / Screen.height;
            maxX = backgroundRenderer.bounds.extents.x - screenWidth / 2;
            minX = -maxX;
            maxY = backgroundRenderer.bounds.extents.y - screenHeight / 2;
            minY = -maxY;
        }

        void Update() {
            if (playerToFollow == null) {
                return;
            }
            var position = playerToFollow.transform.position;
            position.z = camera.transform.position.z;
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            camera.transform.position = position;
        }
    }
}