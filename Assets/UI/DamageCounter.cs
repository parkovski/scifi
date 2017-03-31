using UnityEngine;
using UnityEngine.UI;

using SciFi.Players;

namespace SciFi.UI {
    /// Displays each player's lives and damage on the screen.
    public class DamageCounter : MonoBehaviour, IEnablableUIComponent {
        /// The ID of the player that this counter is tracking.
        /// <seealso cref="SciFi.Players.Player.eId" />
        public int player;
        int lives;
        int damage;

        Text text;

        void Start() {
            enabled = false;
            FindObjectOfType<EnableUI>().Register(this);
        }

        public void Enable() {
            text = GetComponent<Text>();
            GameController.Instance.EventLifeChanged += PlayerLifeChanged;
            GameController.Instance.EventDamageChanged += PlayerDamageChanged;

            GameController.Instance.PlayersInitialized += players => {
                if (player >= players.Length) {
                    return;
                }
                var p = players[player];
                if (p.eTeam == -1) {
                    text.color = Color.white;
                } else {
                    text.color = Player.TeamToColor(p.eTeam, false);
                }
                lives = p.eLives;
                damage = p.eDamage;
                UpdateInfo();
            };
        }

        void PlayerDamageChanged(int playerId, int newDamage) {
            if (playerId != player) {
                return;
            }
            damage = newDamage;
            UpdateInfo();
        }

        void PlayerLifeChanged(int playerId, int newLives) {
            if (playerId != player) {
                return;
            }
            lives = newLives;
            damage = 0;
            UpdateInfo();
        }

        void UpdateInfo() {
            text.text = string.Format("P{0}x{1} D: {2}", player + 1, lives, damage);
        }
    }
}