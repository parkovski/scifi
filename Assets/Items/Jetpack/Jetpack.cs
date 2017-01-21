using UnityEngine;

using SciFi.Players;

namespace SciFi.Items {
    public class Jetpack : Item {
        public GameObject firePrefab;

        GameObject fire;

        float nextBoostTime;
        const float boostTimeout = .125f;
        float boostForce;
        float totalBoostTime;
        float lastTotalBoostTime;
        const float maxBoostTime = 8f;

        void Start() {
            BaseStart(canCharge: true);
        }

        void Update() {
            BaseUpdate();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        public override bool ShouldCharge() {
            return totalBoostTime < maxBoostTime;
        }

        public override bool ShouldThrow() {
            return totalBoostTime >= maxBoostTime;
        }

        protected override void OnBeginCharging() {
            var offset = new Vector3(-0.171f, -0.316f);
            fire = Instantiate(firePrefab, transform.position + offset, Quaternion.identity, transform);
            nextBoostTime = 0f;
            boostForce = eOwner.jumpForce * 10;

            // Turn movement back on while the jetpack is active
            eOwner.ResumeFeature(PlayerFeature.Movement);
            eOwner.SuspendFeature(PlayerFeature.Jump);
        }

        protected override void OnKeepCharging(float chargeTime) {
            totalBoostTime = lastTotalBoostTime + chargeTime;
            if (totalBoostTime >= maxBoostTime) {
                RequestCancel();
                return;
            }

            if (chargeTime > nextBoostTime && transform.position.y < 1) {
                nextBoostTime = chargeTime + boostTimeout;
                eOwner.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, boostForce));
            }
        }

        protected override void OnEndCharging(float chargeTime) {
            Destroy(fire);
            lastTotalBoostTime = totalBoostTime;

            // Return feature flags to their previous state
            eOwner.SuspendFeature(PlayerFeature.Movement);
            eOwner.ResumeFeature(PlayerFeature.Jump);
        }

        protected override void OnCancel() {
            if (IsCharging()) {
                Destroy(fire);
                eOwner.SuspendFeature(PlayerFeature.Movement);
                eOwner.ResumeFeature(PlayerFeature.Jump);
            }
        }
    }
}