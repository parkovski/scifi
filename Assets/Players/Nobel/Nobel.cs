using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.Players {
    public class Nobel : Player {
        public GameObject dynamitePrefab;
        public GameObject dynamite2Prefab;
        public GameObject dynamite3Prefab;
        public GameObject dynamiteFragmentPrefab;
        public GameObject gunPrefab;
        public GameObject bulletPrefab;
        public GameObject gelignitePrefab;

        GameObject gunGo;
        GameObject dynamiteGo;

        private CompoundSpriteFlip spriteFlip;

        protected override void OnInitialize() {
            gunGo = Instantiate(gunPrefab, transform.position + GetGunOffset(defaultDirection), Quaternion.identity);

            eAttack1 = new GunAttack(this, gunGo, bulletPrefab);
            eAttack2 = new GeligniteAttack(this, gelignitePrefab);
            eAttack3 = new DynamiteAttack(this);

            spriteFlip = new CompoundSpriteFlip(gameObject, defaultDirection);
        }

        Vector3 GetGunOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.5f, .2f);
            } else {
                return new Vector3(.5f, .2f);
            }
        }

        new void Update() {
            base.Update();
            if (gunGo == null) {
                return;
            }
            gunGo.transform.position = transform.position + GetGunOffset(eDirection);
        }

        void FixedUpdate() {
            BaseInput();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionExit2D(Collision2D collision) {
            BaseCollisionExit2D(collision);
        }


        protected override void OnChangeDirection() {
            var gunSr = gunGo.GetComponent<SpriteRenderer>();
            gunSr.flipX = eDirection == Direction.Left;
            spriteFlip.Flip(eDirection);
        }

        [Command]
        public void CmdPlantOrExplodeDynamite(int sticks) {
            var position = transform.position + new Vector3(1f, -.5f).FlipDirection(eDirection);
            var velocity = Vector2.zero;
            GameObject prefab;
            switch (sticks) {
            case 1:
                prefab = dynamitePrefab;
                break;
            case 2:
                prefab = dynamite2Prefab;
                break;
            case 3:
                prefab = dynamite3Prefab;
                break;
            default:
                ExplodeDynamite();
                return;
            }
            if (dynamiteGo != null) {
                position = dynamiteGo.transform.position;
                velocity = dynamiteGo.GetComponent<Rigidbody2D>().velocity;
                Destroy(dynamiteGo);
            }
            dynamiteGo = Object.Instantiate(prefab, position, Quaternion.identity);
            dynamiteGo.GetComponent<Rigidbody2D>().velocity = velocity;
            var dynamite = dynamiteGo.GetComponent<Dynamite>();
            // Intentionally don't set spawnedBy so the player that
            // created it can push it around.
            dynamite.explodeCallback = OnDynamiteExploded;
            dynamite.destroyCallback = OnDynamiteDestroyed;
            NetworkServer.Spawn(dynamiteGo);
            RpcSetHasPlantedDynamite(true);
        }

        [Server]
        void ExplodeDynamite() {
            if (dynamiteGo != null) {
                dynamiteGo.GetComponent<Dynamite>().Explode();
            }
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
        void OnDynamiteDestroyed(GameObject objectBeingDestroyed) {
            // Don't want to send this when the dynamite has been
            // replaced with a more powerful one.
            if (dynamiteGo == null || dynamiteGo == objectBeingDestroyed) {
                RpcSetHasPlantedDynamite(false);
            }
        }

        [ClientRpc]
        void RpcSetHasPlantedDynamite(bool hasPlantedDynamite) {
            ((DynamiteAttack)eAttack3).SetHasPlantedDynamite(hasPlantedDynamite);
        }
    }
}