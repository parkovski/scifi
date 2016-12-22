using UnityEngine;
using UnityEngine.UI;

public class DamageCounter : MonoBehaviour {
    public int player;

    Text text;

    void Start() {
        GameController.Instance.EventHealthChanged += PlayerDamageChanged;
        text = GetComponent<Text>();
    }

    void PlayerDamageChanged(DamageChangedEventArgs args) {
        if (args.player.id != player) {
            return;
        }
        UpdateInfo(args.player.id, args.player.lives, args.player.damage);
    }

    void UpdateInfo(int id, int lives, int damage) {
        text.text = string.Format("P{0}x{1} D: {2}", id, lives, damage);
    }
}