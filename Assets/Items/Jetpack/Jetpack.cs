using UnityEngine;

using SciFi.Players;
using SciFi.Players.Attacks;
using SciFi.Players.Modifiers;

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
            BaseStart();
        }

        void Update() {
            BaseUpdate();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        protected override Vector3 GetOwnerOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(.6f, 0f);
            } else {
                return new Vector3(-.6f, 0f);
            }
        }

        Vector3 GetFireOffset() {
            if (eDirection == Direction.Left) {
                return new Vector3(0.171f, -0.316f);
            } else {
                return new Vector3(-0.171f, -0.316f);
            }
        }

        protected override void OnChangeDirection(Direction direction) {
            spriteRenderer.flipX = direction == Direction.Left;
            if (fire != null) {
                fire.transform.localPosition = GetFireOffset();
            }
        }

        public override bool ShouldCharge() {
            return totalBoostTime < maxBoostTime;
        }

        public override bool ShouldThrow() {
            return totalBoostTime >= maxBoostTime;
        }

        protected override void OnBeginCharging() {
            fire = Instantiate(firePrefab, transform.position + GetFireOffset(), Quaternion.identity, transform);
            nextBoostTime = 0f;
            boostForce = eOwner.jumpForce * 10;

            // Turn movement back on while the jetpack is active
            eOwner.RemoveModifier(Modifier.CantMove);
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
            fire = null;
            lastTotalBoostTime = totalBoostTime;

            // Return feature flags to their previous state
            eOwner.AddModifier(Modifier.CantMove);
        }

        protected override void OnCancel() {
            if (IsCharging()) {
                Destroy(fire);
                fire = null;
                eOwner.AddModifier(Modifier.CantMove);
            }
        }

        public override AttackType Type { get { return AttackType.Inert; } }
    }
}