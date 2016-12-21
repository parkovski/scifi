using UnityEngine;
using UnityEngine.UI;

public class DamageCounter : MonoBehaviour {
    public int player;

    Text text;

    void Start() {
        var controller = GameObject.Find("GameController").GetComponent<GameController>();
        controller.HealthChanged += PlayerDamageChanged;
        text = GetComponent<Text>();
    }

    void PlayerDamageChanged(object sender, DamageChangedEventArgs args) {
        if (args.Player.Id != player) {
            return;
        }
        UpdateInfo(args.Player.Id, args.Player.Lives, args.Player.Damage);
    }

    void UpdateInfo(int id, int lives, int damage) {
        text.text = string.Format("P{0}x{1} D: {2}", id+1, lives, damage);
    }
}