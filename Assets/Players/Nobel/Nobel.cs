using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.Environment.Effects;

namespace SciFi.Players {
    public class Nobel : Player {
        public GameObject dynamitePrefab;
        public GameObject dynamiteFragmentPrefab;
        public GameObject gunPrefab;
        public GameObject bulletPrefab;
        public GameObject gelignitePrefab;

        GameObject gunGo;
        GameObject dynamiteGo;

        void Start() {
            BaseStart();

            gunGo = Instantiate(gunPrefab, transform.position + GetGunOffset(defaultDirection), Quaternion.identity);

            eAttack1 = new GunAttack(this, gunGo, bulletPrefab);
            eAttack2 = new GeligniteAttack(this, gelignitePrefab);
            eSpecialAttack = new DynamiteAttack(this);
        }

        Vector3 GetGunOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.5f, .2f);
            } else {
                return new Vector3(.5f, .2f);
            }
        }

        void Update() {
            gunGo.transform.position = transform.position + GetGunOffset(eDirection);
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
            var gunSr = gunGo.GetComponent<SpriteRenderer>();
            gunSr.flipX = !gunSr.flipX;
            foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = !sr.flipX;
            }
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
            }
        }

        [Command]
        public void CmdPlantOrExplodeDynamite() {
            if (dynamiteGo != null) {
                ExplodeDynamite();
            } else {
                PlantDynamite();
            }
        }

        [Server]
        void PlantDynamite() {
            var position = transform.position;
            if (eDirection == Direction.Left) {
                position += new Vector3(-1f, -.5f);
            } else {
                position += new Vector3(1f, -.5f);
            }
            dynamiteGo = Object.Instantiate(dynamitePrefab, position, Quaternion.identity);
            var dynamite = dynamiteGo.GetComponent<Dynamite>();
            dynamite.spawnedBy = netId;
            dynamite.spawnedByExtra = GetItemNetId();
            dynamite.explodeCallback = OnDynamiteExploded;
            dynamite.destroyCallback = OnDynamiteDestroyed;
            NetworkServer.Spawn(dynamiteGo);
            RpcSetDynamiteShouldCharge(false);
        }

        [Server]
        void ExplodeDynamite() {
            dynamiteGo.GetComponent<Dynamite>().Explode();
        }

        [Server]
        void OnDynamiteExploded() {
            var fragGo = Object.Instantiate(dynamiteFragmentPrefab, dynamiteGo.transform.position, Quaternion.identity);
            var frag = fragGo.GetComponent<DynamiteFragment>();
            frag.spawnedBy = netId;
            frag.spawnedByExtra = GetItemNetId();
            NetworkServer.Spawn(fragGo);
        }

        [Server]
        void OnDynamiteDestroyed() {
            RpcSetDynamiteShouldCharge(true);
        }

        [ClientRpc]
        void RpcSetDynamiteShouldCharge(bool shouldCharge) {
            ((DynamiteAttack)eSpecialAttack).SetShouldCharge(shouldCharge);
        }
    }
}