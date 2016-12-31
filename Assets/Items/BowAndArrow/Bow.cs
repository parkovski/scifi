using UnityEngine;

public class Bow : Item {
    int arrows = 5;

    void Start() {
        BaseStart(aliveTime: 10f);
    }

    void Update() {
        BaseUpdate();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }

    protected override bool ShouldThrow() {
        return arrows == 0;
    }

    protected override bool ShouldCharge() {
        return arrows > 0;
    }

    protected override void EndCharging(float chargeTime, Direction direction) {
        --arrows;
    }
}