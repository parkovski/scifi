using UnityEngine;

namespace SciFi.Items {
    public class FireSwordFlameAnimation : MonoBehaviour {
        public Sprite flame1FirstFrame;
        public Sprite flame1SecondFrame;
        public Sprite flame2FirstFrame;
        public Sprite flame2SecondFrame;
        public float flame1FrameTime;
        public float flame2FrameTime;

        SpriteRenderer flame1;
        SpriteRenderer flame2;

        int flame1Frame = 0;
        int flame2Frame = 0;
        float flame1NextFrameTime;
        float flame2NextFrameTime;

        void Start() {
            flame1 = transform.Find("Flame").GetComponent<SpriteRenderer>();
            flame2 = transform.Find("Flame2").GetComponent<SpriteRenderer>();
        }

        void Update() {
            if (Time.time > flame1NextFrameTime) {
                flame1NextFrameTime = Time.time + flame1FrameTime;
                if (flame1Frame == 0) {
                    flame1Frame = 1;
                    flame1.sprite = flame1SecondFrame;
                } else {
                    flame1Frame = 0;
                    flame1.sprite = flame1FirstFrame;
                }
            }

            if (Time.time > flame2NextFrameTime) {
                flame2NextFrameTime = Time.time + flame2FrameTime;
                if (flame2Frame == 0) {
                    flame2Frame = 1;
                    flame2.sprite = flame2SecondFrame;
                } else {
                    flame2Frame = 0;
                    flame2.sprite = flame2FirstFrame;
                }
            }
        }
    }
}
