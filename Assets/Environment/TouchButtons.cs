using UnityEngine;
using System.Collections.Generic;

namespace SciFi.UI {
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

        Dictionary<string, SpriteRenderer> buttons;
        Dictionary<string, SpriteRenderer> buttonGraphics;

        /// The item button is a special case. Consider this scenario:
        /// The button has a regular graphic. The user picks up an item,
        /// and the graphic changes to show you can use the item. Then
        /// you press down, and the graphic changes to the discard graphic.
        /// Then you release down, and the graphic changes back to...
        /// So we need to cache the old graphic.
        Sprite oldItemButtonGraphic;

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

            buttonGraphics = new Dictionary<string, SpriteRenderer>();
            AddGfx("LeftButton", "LeftArrow");
            AddGfx("RightButton", "RightArrow");
            AddGfx("DownButton", "DownArrow");
            AddGfx("AttackButton1", "Attack1");
            AddGfx("AttackButton2", "Attack2");
            AddGfx("ItemButton", "Item");

            oldItemButtonGraphic = buttonGraphics["ItemButton"].sprite;

            var gc = GameObject.Find("GameController");
            if (gc != null) {
                inputManager = gc.GetComponent<InputManager>();
                inputManager.TouchControlStateChanged += StateChanged;
            }
        }

        void Add(string name) {
            buttons.Add(name, GameObject.Find(name).GetComponent<SpriteRenderer>());
        }

        void AddGfx(string name, string gfxName) {
            buttonGraphics.Add(name, GameObject.Find(name).transform.Find(gfxName).GetComponent<SpriteRenderer>());
        }

        void ReplaceComboButtons(string activeControl, bool reset) {
            if (activeControl == "DownButton") {
                buttonGraphics["LeftButton"].sprite = reset ? leftButton : dodgeLeftButton;
                buttonGraphics["RightButton"].sprite = reset ? rightButton : dodgeRightButton;
                buttonGraphics["ItemButton"].sprite = reset ? oldItemButtonGraphic : throwItemButton;
            } else if (activeControl == "AttackButton1") {
                buttonGraphics["AttackButton2"].sprite = reset ? attackButton2 : specialAttackButton;
            } else if (activeControl == "AttackButton2") {
                buttonGraphics["AttackButton1"].sprite = reset ? attackButton1 : specialAttackButton;
            }
        }

        string GetSecondComboButton(string control) {
            switch (control) {
            case "LeftButton":
                return "DownButton";
            case "RightButton":
                return "DownButton";
            case "DownButton":
            default:
                return null;
            }
        }

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
            oldItemButtonGraphic = graphic;
            if (!inputManager.IsControlActive(Control.Down)) {
                buttonGraphics["ItemButton"].sprite = graphic;
            }
        }

        public void SetItemButtonToItemGraphic() {
            SetItemButtonGraphic(itemButton);
        }

        public void SetItemButtonToDiscardGraphic() {
            SetItemButtonGraphic(throwItemButton);
        }
    }
}