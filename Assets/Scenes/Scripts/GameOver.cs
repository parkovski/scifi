using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

using SciFi.Network;
using SciFi.Network.Web;

namespace SciFi.Scenes {
    /// Displays a win or lose screen depending
    /// on the value set in <see cref="TransitionParams" />.
    public class GameOver : MonoBehaviour {
        public Sprite winScreen;
        public Sprite loseScreen;
        public InputManager inputManager;
        public Text text;
        public RectTransform leaderboard;

        void Start() {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            if (TransitionParams.isWinner) {
                spriteRenderer.sprite = winScreen;
                text.text = "You win!";
            } else {
                spriteRenderer.sprite = loseScreen;
                text.text = "You lose.";
            }

            inputManager.ObjectSelected += ObjectSelected;

            StartCoroutine(PopulateLeaderboard());
        }

        void ObjectSelected(GameObject obj) {
            if (TransitionParams.gameType == GameType.Single) {
                SceneManager.LoadScene("TitleScreen");
            } else {
                //SceneManager.LoadScene("Lobby");
                if (NetworkServer.active) {
                    NetworkController.Instance.ServerReturnToLobby();
                }
            }
        }

        Text NewText(string s, RectTransform parent) {
            var go = new GameObject("Text");
            go.transform.parent = parent;
            go.transform.localScale = Vector3.one;
            var newText = go.AddComponent<Text>();
            newText.text = s;
            newText.font = text.font;
            newText.fontSize = 24;
            return newText;
        }

        IEnumerator PopulateLeaderboard() {
            var request = Leaderboard.GetCompetitorStatsRequest(1);
            if (request == null) {
                yield break;
            }
            yield return request;
            var result = Leaderboard.GetCompetitorStatsResult(request);
            if (result == null) {
                yield break;
            }
            var y = 0f;
            var textGenerator = new TextGenerator();
            var textGenerationSettings = new TextGenerationSettings {
                font = text.font,
                fontSize = 24,
            };

            foreach (var row in result) {
                var display = string.Format(
                    "{0}: {1}/{2} matches, {3} kills, {4} deaths",
                    row.name,
                    row.wins,
                    row.matches,
                    row.kills,
                    row.deaths
                );
                var text = NewText(display, leaderboard);
                var height = textGenerator.GetPreferredHeight(display, textGenerationSettings) * 2;
                text.rectTransform.sizeDelta = new Vector2(0, height);
                var pos = text.rectTransform.anchoredPosition;
                pos.y = y;
                y -= height;
                text.rectTransform.anchoredPosition = pos;
                text.rectTransform.anchorMin = new Vector2(0, 1);
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.pivot = new Vector2(0.5f, 1f);
            }
            leaderboard.sizeDelta = new Vector2(leaderboard.sizeDelta.x, -y);
        }
    }
}