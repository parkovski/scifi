using UnityEngine;

public class DeathZone : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag != "Player") {
            Destroy(collider.gameObject);
            return;
        }
        GameController.Instance.CmdDie(collider.gameObject);
    }
}