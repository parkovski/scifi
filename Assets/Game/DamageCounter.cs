using UnityEngine;
using UnityEngine.UI;

public class DamageCounter : MonoBehaviour {
    public int player;
    int lives;
    int damage;

    Text text;

    void Start() {
        GameController.Instance.EventHealthChanged += PlayerDamageChanged;
        text = GetComponent<Text>();

        lives = 1;
        damage = 0;
    }

    void PlayerDamageChanged(DamageChangedEventArgs args) {
        if (args.playerId != player) {
            return;
        }
        damage = args.newDamage;
        UpdateInfo();
    }

    void UpdateInfo() {
        text.text = string.Format("P{0}x{1} D: {2}", player + 1, lives, damage);
    }
}