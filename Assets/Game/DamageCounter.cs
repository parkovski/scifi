using UnityEngine;
using UnityEngine.UI;

public class DamageCounter : MonoBehaviour {
    public int player;
    int lives;
    int damage;

    Text text;

    void Start() {
        text = GetComponent<Text>();
        GameController.Instance.EventLifeChanged += PlayerLifeChanged;
        GameController.Instance.EventDamageChanged += PlayerDamageChanged;
    }

    void PlayerDamageChanged(DamageChangedEventArgs args) {
        if (args.playerId != player) {
            return;
        }
        damage = args.newDamage;
        UpdateInfo();
    }

    void PlayerLifeChanged(LifeChangedEventArgs args) {
        if (args.playerId != player) {
            return;
        }
        lives = args.newLives;
        damage = 0;
        UpdateInfo();
    }

    void UpdateInfo() {
        text.text = string.Format("P{0}x{1} D: {2}", player + 1, lives, damage);
    }
}