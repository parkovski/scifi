using UnityEngine;
using System.Collections.Generic;

namespace SciFi.UI {
    /// Updates the touch buttons UI in response to interactions
    /// and other game events. This does not handle input.
    /// <seealso cref="SciFi.InputManager"/>
    public class TouchButtons : MonoBehaviour {
        InputManager inputManager;
        public Sprite buttonInactive;
        public Sprite buttonActive;
        public Sprite leftButton;
        public Sprite rightButton;
        public Sprite downButton;
        public Sprite dodgeLeftButton;
        public Sprite dodgeRightButton;
        public Sprite attackButton1;
        public Sprite attackButton2;
        public Sprite specialAttackButton;
        public Sprite itemButton;
        public Sprite throwItemButton;

        /// Sprite renderers for button background images.
        Dictionary<string, SpriteRenderer> buttons;
        /// Sprite renderers for the graphics displayed on top of the buttons.
        Dictionary<string, SpriteRenderer> buttonGraphics;

        void Start() {
            buttons = new Dictionary<string, SpriteRenderer>();
            Add("LeftButton");
            Add("RightButton");
            Add("UpButton");
            Add("UpButton2");
            Add("DownButton");
            Add("AttackButton1");
            Add("AttackButton2");
            Add("AttackButton3");

            buttonGraphics = new Dictionary<string, SpriteRenderer>();
            AddGfx("LeftButton", "LeftArrow");
            AddGfx("RightButton", "RightArrow");
            AddGfx("DownButton", "DownArrow");
            AddGfx("AttackButton1", "Attack1");
            AddGfx("AttackButton2", "Attack2");
            AddGfx("AttackButton3", "Attack3");

            var gc = GameObject.Find("GameController");
            if (gc != null) {
                inputManager = gc.GetComponent<InputManager>();
                inputManager.TouchControlStateChanged += StateChanged;
            }
        }

        /// Adds the button labeled <c>name</c> to the background graphic list.
        void Add(string name) {
            buttons.Add(name, GameObject.Find(name).GetComponent<SpriteRenderer>());
        }

        /// Adds the graphic labeled <c>gfxName</c> under the button labeled <c>name</c>
        /// to the foreground graphic list.
        void AddGfx(string name, string gfxName) {
            buttonGraphics.Add(name, GameObject.Find(name).transform.Find(gfxName).GetComponent<SpriteRenderer>());
        }

        /// When a button is pressed that is a part of a combo,
        /// replace the graphic on the other button indicating
        /// that those two buttons together have a special purpose.
        void ReplaceComboButtons(string activeControl, bool reset) {
            if (activeControl == "DownButton") {
                buttonGraphics["LeftButton"].sprite = reset ? leftButton : dodgeLeftButton;
                buttonGraphics["RightButton"].sprite = reset ? rightButton : dodgeRightButton;
            }
        }

        /// Returns the name of the second button that is a part of
        /// a combo with <c>control</c>, or null if there is no combo.
        string GetSecondComboButton(string control) {
            switch (control) {
            case "LeftButton":
                return "DownButton";
            case "RightButton":
                return "DownButton";
            default:
                return null;
            }
        }

        /// Updates the background graphic for a button state change
        /// (press, release).
        void StateChanged(string control, bool active) {
            if (active) {
                buttons[control].sprite = buttonActive;
            } else {
                buttons[control].sprite = buttonInactive;
            }
            ReplaceComboButtons(control, !active);
        }

        /// If down is pressed, the item button will show the
        /// discard graphic, but it will revert back to whatever
        /// was last set using this method.
        public void SetItemButtonGraphic(Sprite graphic) {
            if (!inputManager.IsControlActive(Control.Down)) {
                buttonGraphics["ItemButton"].sprite = graphic;
            }
        }

        /// Resets the item button to show the generic item graphic.
        public void SetItemButtonToItemGraphic() {
            SetItemButtonGraphic(itemButton);
        }

        /// Changes the item button to show the throw item graphic.
        public void SetItemButtonToDiscardGraphic() {
            SetItemButtonGraphic(throwItemButton);
        }
    }
}