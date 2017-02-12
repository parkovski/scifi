using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.Util;

namespace SciFi.Players {
    public class Nobel : Player {
        public GameObject dynamitePrefab;
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
            eSpecialAttack = new DynamiteAttack(this);

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
            // Intentionally don't set spawnedBy so the player that
            // created it can push it around.
            dynamite.explodeCallback = OnDynamiteExploded;
            dynamite.destroyCallback = OnDynamiteDestroyed;
            NetworkServer.Spawn(dynamiteGo);
            RpcSetHasPlantedDynamite(true);
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
            RpcSetHasPlantedDynamite(false);
        }

        [ClientRpc]
        void RpcSetHasPlantedDynamite(bool hasPlantedDynamite) {
            ((DynamiteAttack)eSpecialAttack).SetHasPlantedDynamite(hasPlantedDynamite);
        }
    }
}