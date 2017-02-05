using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.Util.Extensions;

namespace SciFi.Players {
    public class daVinci : Player {
        public GameObject boneArmPrefab;
        public GameObject flyingMachinePrefab;
        public GameObject paintbrushPrefab;
        public GameObject paintStreak;
        GameObject boneArm;
        GameObject paintbrush;

        void Start() {
            BaseStart();

            boneArm = Instantiate(boneArmPrefab, transform.position + GetBoneArmOffset(defaultDirection), Quaternion.identity);
            ReverseSprite(boneArm);
            paintbrush = Instantiate(paintbrushPrefab, transform.position + GetPaintbrushOffset(defaultDirection), Quaternion.identity);

            eAttack1 = new PaintbrushAttack(this, paintbrush.GetComponent<Paintbrush>());
            eAttack2 = new BoneArmAttack(this, boneArm.GetComponent<BoneArm>());
            eSpecialAttack = new FlyingMachineAttack(this, flyingMachinePrefab);
        }

        void Update() {
            boneArm.transform.position = transform.position + GetBoneArmOffset(eDirection);
            paintbrush.transform.position = transform.position + GetPaintbrushOffset(eDirection);
        }

        Vector3 GetBoneArmOffset(Direction direction) {
            return new Vector3(.7f, .2f).FlipDirection(direction);
        }

        Vector3 GetPaintbrushOffset(Direction direction) {
            return new Vector3(.35f, .3f).FlipDirection(direction);
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

        void ReverseTransform(Transform transform) {
            transform.localPosition = transform.localPosition.FlipX();
            for (var i = 0; i < transform.childCount; i++) {
                ReverseTransform(transform.GetChild(i));
            }
        }

        void ReverseSprite(GameObject sprite) {
            foreach (var sr in sprite.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = !sr.flipX;
            }
            for (var i = 0; i < sprite.transform.childCount; i++) {
                ReverseTransform(sprite.transform.GetChild(i));
            }
        }

        [ClientRpc]
        protected override void RpcChangeDirection(Direction direction) {
            ReverseSprite(gameObject);
            ReverseSprite(boneArm);
        }
    }
}