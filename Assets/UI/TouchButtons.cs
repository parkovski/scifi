using UnityEngine;
using System.Collections.Generic;

namespace SciFi.UI {
    /// Updates the touch buttons UI in response to interactions
    /// and other game events. This does not handle input.
    /// <seealso cref="SciFi.InputManager"/>
    public class TouchButtons : MonoBehaviour {
        public InputManager inputManager;
        public Sprite buttonInactive;
        public Sprite buttonActive;
        public Sprite leftButton;
        public Sprite rightButton;
        public Sprite downButton;
        public Sprite dodgeLeftButton;
        public Sprite dodgeRightButton;
        public Sprite itemButton;
        public Sprite throwItemButton;

        Sprite itemButtonGraphic;

        /// Sprite renderers for button background images.
        Dictionary<string, SpriteRenderer> buttons;
        /// Sprite renderers for the graphics displayed on top of the buttons.
        Dictionary<string, SpriteRenderer> buttonGraphics;

        void Start() {
            buttons = new Dictionary<string, SpriteRenderer>();
            Add("ItemButton");
            Add("AttackButton1");
            Add("AttackButton2");
            Add("AttackButton3");
            Add("UpButton");

            buttonGraphics = new Dictionary<string, SpriteRenderer>();
            AddGfx("AttackButton1", "Attack1");
            AddGfx("AttackButton2", "Attack2");
            AddGfx("AttackButton3", "Attack3");
            AddGfx("ItemButton", "Item");

            inputManager.TouchControlStateChanged += StateChanged;
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

        /// Change the item button to show the graphic for the
        /// current item.
        public void SetItemButtonGraphic(Sprite graphic) {
            buttonGraphics["ItemButton"].sprite = graphic;
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