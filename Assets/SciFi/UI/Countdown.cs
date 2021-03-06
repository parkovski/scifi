using UnityEngine;
using UnityEngine.UI;

using SciFi.Util;

namespace SciFi.UI {
    /// Displays the 3, 2, 1, Go! countdown before the game.
    /// Also fast-forwards the song to line up with the countdown.
    public class Countdown : MonoBehaviour {
        public Cues cues;
        public Text text;

        public delegate void OnFinishedHandler(object sender);
        public event OnFinishedHandler OnFinished;

        public void Setup(AudioClip song, uint bpm, uint beatOffset) {
            float beat = 60f / bpm;
            var music = GameObject.Find("Music").GetComponent<AudioSource>();
            music.clip = song;
            music.time = beat * beatOffset;
            cues.Add(beat,     () => ChangeText("3"));
            cues.Add(beat * 2, () => ChangeText("2"));
            cues.Add(beat * 3, () => ChangeText("1"));
            cues.Add(beat * 4, () => {
                ChangeText("Go!");
                OnFinished(this);
            });
            cues.Add(beat * 6, () => ChangeText(""));
        }

        /// Begins the countdown.
        public void Start() {
            cues.Reset();
            cues.Resume();
            GameObject.Find("Music").GetComponent<AudioSource>().Play();
        }

        void ChangeText(string newText) {
            text.text = newText;
        }
    }
}