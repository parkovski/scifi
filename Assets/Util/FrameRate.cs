#define DEBUG

using UnityEngine;

namespace SciFi.Util {
    public class FrameRate : MonoBehaviour {
#if DEBUG
        float frameRate;
        const float frameRateUpdateInterval = 1f;
        float lastFrameRateUpdateTime;
        int frameRateDebugField;

        void Update() {
            if (Time.time > lastFrameRateUpdateTime + frameRateUpdateInterval) {
                lastFrameRateUpdateTime = Time.time;
                frameRate = 1 / Time.deltaTime;
                DebugPrinter.Instance.SetField(frameRateDebugField, "FPS: " + frameRate.ToString());
            }
        }
#endif
    }
}