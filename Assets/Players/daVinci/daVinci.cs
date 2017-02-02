using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;

namespace SciFi.Players {
    public class daVinci : Player {
        public GameObject boneArmPrefab;
        GameObject boneArm;

        void Start() {
            BaseStart();

            boneArm = Instantiate(boneArmPrefab, transform.position, Quaternion.identity);
            ReverseSprite(boneArm);

            eAttack1 = new PaintbrushAttack(this);
            eAttack2 = new BoneArmAttack(this, boneArm);
            eSpecialAttack = new FlyingMachineAttack(this);
        }

        void Update() {
            boneArm.transform.position = transform.position + GetBoneArmOffset(eDirection);
        }

        Vector3 GetBoneArmOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.9f, .2f);
            } else {
                return new Vector3(.9f, .2f);
            }
        }

        void FixedUpdate() {
            if (!isLocalPlayer) {
                return;
            }

            BaseInput();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionExit2D(Collision2D collision) {
            BaseCollisionExit2D(collision);
        }

        void ReverseSprite(GameObject sprite) {
            foreach (var sr in sprite.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = !sr.flipX;
            }
            for (var i = 0; i < sprite.transform.childCount; i++) {
                var child = sprite.transform.GetChild(i);
                child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
            }
        }

        [ClientRpc]
        protected override void RpcChangeDirection(Direction direction) {
            ReverseSprite(gameObject);
            ReverseSprite(boneArm);
        }
    }
}