using UnityEngine;
using System.Collections.Generic;

namespace SciFi.UI {
    public class TouchButtons : MonoBehaviour {
        public InputManager inputManager;
        public Sprite buttonInactive;
        public Sprite buttonActive;

        Dictionary<string, SpriteRenderer> buttons;

        void Start() {
            buttons = new Dictionary<string, SpriteRenderer>();
            Add("LeftButton");
            Add("RightButton");
            Add("UpButton");
            Add("DownButton1");
            Add("AttackButton1");
            Add("AttackButton2");
            Add("ItemButton");
            Add("DownButton2");

            var gc = GameObject.Find("GameController");
            if (gc != null) {
                inputManager = gc.GetComponent<InputManager>();
                inputManager.TouchControlStateChanged += StateChanged;
            }
        }

        void Add(string name) {
            buttons.Add(name, GameObject.Find(name).GetComponent<SpriteRenderer>());
        }

        void StateChanged(string control, bool active) {
            if (active) {
                buttons[control].sprite = buttonActive;
            } else {
                buttons[control].sprite = buttonInactive;
            }
        }
    }
}