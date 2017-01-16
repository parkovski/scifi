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
            Add("UpButton2");
            Add("DownButton");
            Add("AttackButton1");
            Add("AttackButton2");
            Add("ItemButton");

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