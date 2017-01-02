using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using SciFi.Util;

namespace SciFi.UI {
    public class Countdown : MonoBehaviour {
        public Cues cues;
        public Text text;

        void Start() {
            cues.Add(2f, () => ChangeText("3"));
            cues.Add(2.5f, () => ChangeText("2"));
            cues.Add(3f, () => ChangeText("1"));
            cues.Add(3.5f, () => ChangeText("Go!"));
            cues.Add(4.5f, () => ChangeText(""));

            text = GetComponents<Text>().Single(t => t.name == "Countdown");
        }

        void ChangeText(string newText) {
            text.text = newText;
        }
    }
}