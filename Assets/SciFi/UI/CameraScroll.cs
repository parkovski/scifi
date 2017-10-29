// God help you if you have to edit this.
// I'm talking to you future Parker.

using UnityEngine;

using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.UI {
    public class CameraScroll : MonoBehaviour {
        public Player playerToFollow;
        public new Camera camera;
        public SpriteRenderer backgroundRenderer;

        /// The position in the player window where we want the camera to end up.
        Vector2 targetPosition;

        /// The camera's maximum allowed x position based on the screen size.
        /// The minimum is always `-maxX`.
        float maxX;
        /// The camera's maximum allowed y position based on the screen size.
        /// The minimum is always `-maxY`.
        float maxY;
        /// The last x velocity / change in x position.
        float lastVx;
        /// The last y velocity / change in y position.
        float lastVy;
        /// At 60fps, the maximum delta position (velocity) that the camera
        /// can move per frame.
        const float maxV = 1.5f;
        /// At 60fps, the maximum acceleration allowed per frame.
        const float maxA = 0.015f;
        /// Max braking acceleration.
        const float maxAb = 0.04f;
        /// Velocity where hard braking stops.
        const float vSoftBrake = 0.35f;
        /// The maximum difference allowed between the camera and the
        /// tracking target position before a refocus is triggered.
        const float refocusThreshold = 1f;
        /// If the background is within this much of the screen size,
        /// we won't allow scrolling in that direction.
        const float screenSizeTolerance = 0.1f;
        /// This really messes with me because of the frames per second thing
        /// so I'm putting it behind a const so I stop trying to figure it out.
        const float timeMultiplier = 60.0f;
        const float invTimeMultiplier = 0.01666667f;

        /// If this is true, the camera will aim for a smaller window
        /// to fit the player into.
        bool refocus = false;

        void Start() {
            float screenHeight = camera.orthographicSize * 2;
            float screenWidth = screenHeight * Screen.width / Screen.height;
            var bgExtents = backgroundRenderer.bounds.extents;
            var screenExtents = new Vector2(screenWidth, screenHeight) / 2;
            if (Mathf.Abs(bgExtents.x - screenExtents.x) < screenSizeTolerance) {
                maxX = 0;
            } else {
                maxX = bgExtents.x - screenExtents.x;
            }
            if (Mathf.Abs(bgExtents.y - screenExtents.y) < screenSizeTolerance) {
                maxY = 0;
            } else {
                maxY = bgExtents.y - screenExtents.y;
            }
        }

        /// The length of one side (half-width) of the tracking tolerance window.
        const float xwin = 2f;
        /// The length of one side (half-height) of the tracking tolerance window.
        const float ywin = 1.5f;
        /// Half-width of the refocus tracking tolerance window.
        const float smallXwin = 0.25f;
        /// Half-height of the refocus tracking tolerance window.
        const float smallYwin = 0.2f;
        /// Units on either side of the window edge to snap the camera to.
        const float stickyEdgeSize = 0.25f;

        void LateUpdate() {
            if (playerToFollow == null) {
                return;
            }
            Vector2 dp = playerToFollow.transform.position - camera.transform.position;

            var cx = camera.transform.position.x;
            var cy = camera.transform.position.y;
            var cz = camera.transform.position.z;

            // If either edge snapped, there's no use bothering with calculations for it.
            bool xSnapped = false;
            bool ySnapped = false;

            if (refocus) {
                targetPosition.x = GetTargetPosition(cx, 0, dp.x);
                targetPosition.x = Mathf.Clamp(targetPosition.x, -maxX, maxX);
                targetPosition.y = GetTargetPosition(cy, 0, dp.y);
                targetPosition.y = Mathf.Clamp(targetPosition.y, -maxY, maxY);
                var xofs = Mathf.Abs(targetPosition.x - cx);
                var yofs = Mathf.Abs(targetPosition.y - cy);
                if (xofs < smallXwin && yofs < smallYwin) {
                    refocus = false;
                }
            } else {
                targetPosition.x = GetTargetPosition(cx, xwin, dp.x);
                targetPosition.x = Mathf.Clamp(targetPosition.x, -maxX, maxX);
                targetPosition.y = GetTargetPosition(cy, ywin, dp.y);
                targetPosition.y = Mathf.Clamp(targetPosition.y, -maxY, maxY);

                // Sticky window edges - if we're close to the edge, snap to it.
                // This should get rid of jitter (fingers crossed).
                if (Mathf.Abs(cx - targetPosition.x) <= stickyEdgeSize) {
                    cx = targetPosition.x;
                    xSnapped = true;
                }
                if (Mathf.Abs(cy - targetPosition.y) <= stickyEdgeSize) {
                    cy = targetPosition.y;
                    if (xSnapped) {
                        // In this case we just say that velocities were the same
                        // and skip all the calculations.
                        camera.transform.position = new Vector3(cx, cy, cz);
                        return;
                    }
                    ySnapped = true;
                }
            }

            // Sec / frame. The 60 is misleading, ignore it.
            // Multiply by this to convert frame time to seconds time,
            // or to convert 1/seconds to 1/frames. Or I think so at least.
            // I'm still not entirely sure if frame rate messes with this.
            var timeScale = Time.deltaTime * timeMultiplier;
            var invTimeScale = Time.deltaTime * invTimeMultiplier;
            // units/sec (units/sec^2 * sec/1 frame)
            var amax = maxA * timeScale;

            // Skip snapped values.
            if (!xSnapped) {
                // Units
                var dx = targetPosition.x - camera.transform.position.x;
                // Seconds
                var dxUnscaled = dx * invTimeScale;
                // Unscaled values (uses less multiplications that way).
                float dxmax, dxmin;

                // Allow more acceleration for braking so the camera doesn't keep
                // swinging back and forth.
                if (lastVx > vSoftBrake) {
                    dxmax = Mathf.Min(maxV, amax + lastVx);
                    dxmin = (lastVx - maxAb * timeScale);
                } else if (lastVx < -vSoftBrake) {
                    dxmax = (lastVx + maxAb * timeScale);
                    dxmin = Mathf.Max(-maxV, -amax + lastVx);
                } else {
                    dxmax = Mathf.Min(maxV, amax + lastVx);
                    dxmin = Mathf.Max(-maxV, -amax + lastVx);
                }

                if (dxUnscaled > dxmax) {
                    if (dx > refocusThreshold) {
                        refocus = true;
                    }
                    dxUnscaled = dxmax;
                    dx = dxmax * timeScale;
                } else if (dxUnscaled < dxmin) {
                    if (dx < -refocusThreshold) {
                        refocus = true;
                    }
                    dxUnscaled = dxmin;
                    dx = dxmin * timeScale;
                }

                cx += dx;
                lastVx = dxUnscaled;
            }

            if (!ySnapped) {
                // Units
                var dy = targetPosition.y - camera.transform.position.y;
                // Seconds
                var dyUnscaled = dy * invTimeScale;
                float dymax, dymin;

                if (lastVy > vSoftBrake) {
                    dymax = Mathf.Min(maxV, amax + lastVy);
                    dymin = (lastVy - maxAb * timeScale);
                } else if (lastVy < -vSoftBrake) {
                    dymax = (lastVy + maxAb * timeScale);
                    dymin = Mathf.Max(-maxV, -amax + lastVy);
                } else {
                    dymax = Mathf.Min(maxV, amax + lastVy);
                    dymin = Mathf.Max(-maxV, -amax + lastVy);
                }

                if (dyUnscaled > dymax) {
                    if (dy > refocusThreshold) {
                        refocus = true;
                    }
                    dyUnscaled = dymax;
                    dy = dymax * timeScale;
                } else if (dyUnscaled < dymin) {
                    if (dy < -refocusThreshold) {
                        refocus = true;
                    }
                    dyUnscaled = dymin;
                    dy = dymin * timeScale;
                }

                cy += dy;
                lastVy = dyUnscaled;
            }

            camera.transform.position = new Vector3(cx, cy, cz);
        }

        /// Returns the closest position within the given window.
        /// <param name="position">
        ///     The current camera position.
        /// </param>
        /// <param name="maxExtent">
        ///     The maximum allowed difference between the tracked position
        ///     and the camera position.
        /// </param>
        /// <param name="offset">
        ///     The current difference between the tracked and camera positions.
        /// </param>
        /// <param name="windowOffset">
        ///     The offset from center of the buffer window.
        /// </param>
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