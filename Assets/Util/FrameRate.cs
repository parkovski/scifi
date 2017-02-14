#define DEBUG

using UnityEngine;

namespace SciFi.Util {
    public class FrameRate : MonoBehaviour {
#if DEBUG
        float frameRate;
        float frameRateUpdateInterval = 1f;
        float lastFrameRateUpdateTime;
        int frameRateDebugField;
        const int currentFrameRateWeight = 10;

        void Start() {
            frameRate = 1 / Time.deltaTime;
            lastFrameRateUpdateTime = Time.time;
            frameRateDebugField = DebugPrinter.Instance.NewField();
            DebugPrinter.Instance.SetField(frameRateDebugField, "FPS: " + frameRate.ToString());
        }

        void Update() {
            if (Time.time > lastFrameRateUpdateTime + frameRateUpdateInterval) {
                lastFrameRateUpdateTime = Time.time;
                frameRate = ((currentFrameRateWeight - 1) * frameRate + (1 / Time.deltaTime)) / currentFrameRateWeight;
                DebugPrinter.Instance.SetField(frameRateDebugField, "FPS: " + frameRate.ToString());
            }
        }
#endif
    }
}