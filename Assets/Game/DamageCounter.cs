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
        UpdateInfo(args.Player.Lives, args.Player.Damage);
    }

    void UpdateInfo(int lives, int damage) {
        text.text = string.Format("P1x{0} D: {1}", lives, damage);
    }
}