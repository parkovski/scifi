using UnityEngine;

using SciFi.Players;

namespace SciFi.Items {
    public class MagnifyingGlass : Item {
        public GameObject lightBeam;
        int uses;
        const int maxUses = 7;

        void Start() {
            BaseStart(false, aliveTime: 10);
        }

        void Update() {
            BaseUpdate();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        public override bool ShouldThrow() {
            return uses >= maxUses;
        }

        public override bool ShouldCharge() {
            return false;
        }

        protected override void OnEndCharging(float chargeTime) {
            ++uses;

            var offset = eDirection == Direction.Left ? new Vector3(-.1f, .15f) : new Vector3(.2f, .2f);
            var y = eDirection == Direction.Left ? 180f : 0f;
            var lbObj = Instantiate(lightBeam, transform.position + offset, Quaternion.Euler(0f, y, -15f));
            var lb = lbObj.GetComponent<LightBeam>();
            lb.magnifyingGlassGo = gameObject;
            lb.backwards = eDirection == Direction.Left;
        }

        public void Hit(GameObject obj) {
            if (isServer) {
                GameController.Instance.TakeDamage(obj, 1);
            }
        }
    }
}