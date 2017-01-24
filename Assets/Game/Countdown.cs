using UnityEngine;
using UnityEngine.UI;

using SciFi.Util;

namespace SciFi.UI {
    public class Countdown : MonoBehaviour {
        public Cues cues;
        public Text text;

        /// The battle song is 112bpm - sync the text changes to the beat.
        const float beat = 0.5357f;

        public delegate void OnFinishedHandler(object sender);
        public event OnFinishedHandler OnFinished;

        void Start() {
            cues.Add(beat,     () => ChangeText("3"));
            cues.Add(beat * 2, () => ChangeText("2"));
            cues.Add(beat * 3, () => ChangeText("1"));
            cues.Add(beat * 4, () => {
                ChangeText("Go!");
                OnFinished(this);
            });
            cues.Add(beat * 6, () => ChangeText(""));
        }

        public void StartGame() {
            cues.Reset();
            cues.Resume();
        }

        void ChangeText(string newText) {
            text.text = newText;
        }
    }
}