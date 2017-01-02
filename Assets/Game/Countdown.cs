using UnityEngine;
using UnityEngine.UI;

using SciFi.Util;

namespace SciFi.UI {
    public class Countdown : MonoBehaviour {
        public Cues cues;
        public Text text;

        /// The battle song is 112bpm - sync the text changes to the beat.
        const float beat = 0.5357f;

        void Start() {
            cues.Add(beat * 10, () => ChangeText("3"));
            cues.Add(beat * 12, () => ChangeText("2"));
            cues.Add(beat * 14, () => ChangeText("1"));
            cues.Add(beat * 16, () => ChangeText("Go!"));
            cues.Add(beat * 18, () => ChangeText(""));
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