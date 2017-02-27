using UnityEngine;
using UnityEngine.UI;

using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.UI {
    /// A label above the player, by default showing "P1", etc. but
    /// may be overridden by setting a nickname.
    public class PlayerLabels : MonoBehaviour, IEnablableUIComponent {
        RectTransform[] panels;
        Text[] labels;
        Player[] players;

        /// Callback when the game is started and player display names are set.
        void Init(Player[] players) {
            this.players = players;
            print(players.Length);
            var i = 0;
            while (i < players.Length) {
                if (players[i].eTeam == -1) {
                    panels[i].GetComponent<Image>().color = Color.black.WithAlpha(0.4f);
                } else {
                    panels[i].GetComponent<Image>().color = Player.TeamToColor(players[i].eTeam, true).WithAlpha(0.4f);
                }
                labels[i].text = players[i].eDisplayName;
                i++;
            }
            while (i < panels.Length) {
                Destroy(panels[i].gameObject);
                Destroy(labels[i].gameObject);
                i++;
            }
        }

        void Start() {
            this.enabled = false;
            FindObjectOfType<EnableUI>().Register(this);
        }

        public void Enable() {
            panels = new RectTransform[4];
            labels = new Text[4];

            for (var i = 0; i < panels.Length; i++) {
                panels[i] = transform.Find("P" + (i+1) + "LabelPanel").GetComponent<RectTransform>();
                labels[i] = transform.Find("P" + (i+1) + "Label").GetComponent<Text>();
            }

            players = new Player[0];
            GameController.Instance.PlayersInitialized += Init;
        }

        void LateUpdate() {
            if (!GameController.Instance.IsPlaying()) {
                return;
            }

            for (var i = 0; i < players.Length; i++) {
                var player = players[i];
                var pos = player.transform.position;
                pos.y += 1f;
                pos.y += Mathf.Sin(Time.time * 10f) / 50f;
                panels[i].position = pos;
                labels[i].rectTransform.position = pos;
            }
        }
    }
}