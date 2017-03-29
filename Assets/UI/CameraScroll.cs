using UnityEngine;

using SciFi.Players;

namespace SciFi.UI {
    public class CameraScroll : MonoBehaviour {
        public Player playerToFollow;
        public new Camera camera;
        public SpriteRenderer backgroundRenderer;

        Vector3 targetPosition;

        float minX;
        float maxX;
        float minY;
        float maxY;
        /// At 60fps
        const float maxDelta = 0.2f;
        const float refocusThreshold = 1f;
        /// If the background is within this much of the screen size,
        /// we won't allow scrolling in that direction.
        const float screenSizeTolerance = 0.1f;

        /// If this is true, the camera will aim for a smaller window
        /// to fit the player into.
        bool refocus = false;

        void Start() {
            float screenHeight = camera.orthographicSize * 2;
            float screenWidth = screenHeight * Screen.width / Screen.height;
            var bgExtents = backgroundRenderer.bounds.extents;
            var screenExtents = new Vector2(screenWidth, screenHeight) / 2;
            if (Mathf.Abs(bgExtents.x - screenExtents.x) < screenSizeTolerance) {
                maxX = minX = 0;
            } else {
                maxX = bgExtents.x - screenExtents.x;
                minX = -maxX;
            }
            if (Mathf.Abs(bgExtents.y - screenExtents.y) < screenSizeTolerance) {
                maxY = minY = 0;
            } else {
                maxY = bgExtents.y - screenExtents.y;
                minY = -maxY;
            }
        }

        const float xwin = 2f;
        const float ywin = 1.5f;
        const float smallXwin = 0.25f;
        const float smallYwin = 0.2f;
        void Update() {
            if (playerToFollow == null) {
                return;
            }
            var dp = playerToFollow.transform.position - camera.transform.position;

            if (refocus) {
                targetPosition.x = GetTargetPosition(camera.transform.position.x, smallXwin, dp.x);
                targetPosition.y = GetTargetPosition(camera.transform.position.y, smallYwin, dp.y);
                if (targetPosition.x == camera.transform.position.x && targetPosition.y == camera.transform.position.y) {
                    refocus = false;
                }
            } else {
                targetPosition.x = GetTargetPosition(camera.transform.position.x, xwin, dp.x);
                targetPosition.y = GetTargetPosition(camera.transform.position.y, ywin, dp.y);
            }

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            targetPosition.z = camera.transform.position.z;

            var newCameraPosition = targetPosition;
            var dx = targetPosition.x - camera.transform.position.x;
            var dy = targetPosition.y - camera.transform.position.y;
            var dmax = maxDelta * Time.deltaTime * 60f;
            if (dx > dmax) {
                newCameraPosition.x = camera.transform.position.x + dmax;
                if (dx > refocusThreshold) {
                    refocus = true;
                }
            } else if (dx < -dmax) {
                newCameraPosition.x = camera.transform.position.x - dmax;
                if (dx < -refocusThreshold) {
                    refocus = true;
                }
            }
            
            if (dy > dmax) {
                newCameraPosition.y = camera.transform.position.y + dmax;
                if (dy > refocusThreshold) {
                    refocus = true;
                }
            } else if (dy < -dmax) {
                newCameraPosition.y = camera.transform.position.y - dmax;
                if (dy < -refocusThreshold) {
                    refocus = true;
                }
            }

            camera.transform.position = newCameraPosition;
        }

        // offset = player - camera
        float GetTargetPosition(float position, float maxExtent, float offset, float windowOffset = 0) {
            if (offset >= maxExtent + windowOffset) {
                return position + offset - maxExtent - windowOffset;
            } else if (offset <= -maxExtent + windowOffset) {
                return position + offset + maxExtent - windowOffset;
            } else {
                return position;
            }
        }
    }
}