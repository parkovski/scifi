#define DEBUG

using UnityEngine;

namespace SciFi.Util {
    public class FrameRate : MonoBehaviour {
#if DEBUG
        float frameRate;
        const float frameRateUpdateInterval = 1f;
        float lastFrameRateUpdateTime;
        DebugField frameRateDebugField;

        void Start() {
            frameRateDebugField = new DebugField();
        }

        void Update() {
            if (Time.time > lastFrameRateUpdateTime + frameRateUpdateInterval) {
                lastFrameRateUpdateTime = Time.time;
                frameRate = 1 / Time.deltaTime;
                frameRateDebugField.Set("FPS: " + frameRate.ToString());
            }
        }
#endif
    }
}