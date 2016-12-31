using UnityEngine;

public class Bow : Item {
    void Start() {
        BaseStart(aliveTime: 10f);
    }

    void Update() {
        BaseUpdate();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }
}