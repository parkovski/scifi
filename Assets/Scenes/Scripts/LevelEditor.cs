using UnityEngine;
using System.Collections.Generic;

namespace SciFi.Scenes {
    class ButtonGroup {
        public string activeButton;
        public HashSet<string> buttons;

        public ButtonGroup(string activeButton, params string[] otherButtons) {
            this.activeButton = activeButton;
            this.buttons = new HashSet<string>();
            this.buttons.Add(activeButton);
            foreach (var btn in otherButtons) {
                this.buttons.Add(btn);
            }
        }
    }

    public class LevelEditor : MonoBehaviour {
        public Sprite buttonUnpressed;
        public Sprite buttonPressed;
        public InputManager inputManager;

        ButtonGroup objectsGroup;
        ButtonGroup toolsGroup;

        void Start() {
            inputManager.ObjectSelected += ObjectSelected;
            inputManager.ObjectDeselected += ObjectDeselected;

            objectsGroup = new ButtonGroup("GroundBtn", "HazardsBtn", "PlatformsBtn", "MarkersBtn");
            toolsGroup = new ButtonGroup("DrawBtn", "FillBtn", "EraseBtn", "SelectBtn");
        }

        void ObjectSelected(GameObject go) {
            if (HandleButtonGroup(objectsGroup, go.name)) {
                return;
            }
            if (HandleButtonGroup(toolsGroup, go.name)) {
                return;
            }

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = buttonPressed;
        }

        void ObjectDeselected(GameObject go) {
            if (objectsGroup.buttons.Contains(go.name) || toolsGroup.buttons.Contains(go.name)) {
                return;
            }
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = buttonUnpressed;
        }

        bool HandleButtonGroup(ButtonGroup group, string name) {
            if (group.buttons.Contains(name)) {
                GameObject.Find(group.activeButton).GetComponent<SpriteRenderer>().sprite = buttonUnpressed;
                GameObject.Find(name).GetComponent<SpriteRenderer>().sprite = buttonPressed;
                group.activeButton = name;
                return true;
            }
            return false;
        }
    }
}