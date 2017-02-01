using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;

namespace SciFi.Players {
    public class Nobel : Player {
        public GameObject dynamitePrefab;
        public GameObject dynamiteFragmentPrefab;
        public GameObject gunPrefab;
        public GameObject bulletPrefab;
        public GameObject gelignitePrefab;

        GameObject gun;

        void Start() {
            BaseStart();

            gun = Instantiate(gunPrefab, transform.position + GetGunOffset(defaultDirection), Quaternion.identity);

            eAttack1 = new GunAttack(this, gun, bulletPrefab);
            eAttack2 = new GeligniteAttack(this, gelignitePrefab);
            eSpecialAttack = new DynamiteAttack(this, new[] { dynamitePrefab, dynamiteFragmentPrefab });
        }

        Vector3 GetGunOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.5f, .2f);
            } else {
                return new Vector3(.5f, .2f);
            }
        }

        void Update() {
            gun.transform.position = transform.position + GetGunOffset(eDirection);
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


        [ClientRpc]
        protected override void RpcChangeDirection(Direction direction) {
            var gunSr = gun.GetComponent<SpriteRenderer>();
            gunSr.flipX = !gunSr.flipX;
            foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = !sr.flipX;
            }
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
            }
        }
    }
}