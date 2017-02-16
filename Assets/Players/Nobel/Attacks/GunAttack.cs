using UnityEngine;
using System.Collections;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class GunAttack : Attack {
        GameObject gun;
        GameObject bulletPrefab;
        SpriteRenderer gunRenderer;
        AudioSource audioSource;

        public GunAttack(Player player, GameObject gun, GameObject bulletPrefab)
            : base(player, .25f, false)
        {
            this.gun = gun;
            this.bulletPrefab = bulletPrefab;
            this.canFireDown = true;

            gunRenderer = gun.GetComponent<SpriteRenderer>();
            gunRenderer.enabled = false;
            audioSource = gun.GetComponent<AudioSource>();
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var rotation = Quaternion.identity;
            if (direction == Direction.Down) {
                player.StartCoroutine(ShowHideGunDown());
                rotation = Quaternion.Euler(0, 0, -90f);
            } else {
                player.StartCoroutine(ShowHideGun());
            }
            player.CmdSpawnProjectileFlipped(
                GameController.PrefabToIndex(bulletPrefab),
                gun.transform.position + GetBulletOffset(direction),
                rotation,
                GetBulletVelocity(direction),
                0f,
                direction == Direction.Left
            );
            audioSource.Play();
        }

        public override void OnCancel() {
        }

        Vector2 GetBulletVelocity(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector2(-10f, 0f);
            } else if (direction == Direction.Right) {
                return new Vector2(10f, 0f);
            } else {
                // Down
                return new Vector2(0f, -10f);
            }
        }

        Vector3 GetBulletOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.3f, -.1f);
            } else if (direction == Direction.Right) {
                return new Vector3(.3f, -.1f);
            } else {
                // Down
                return new Vector3(.3f, -.1f).FlipDirection(player.eDirection);
            }
        }

        IEnumerator ShowHideGun() {
            gunRenderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
            gunRenderer.enabled = false;
        }

        IEnumerator ShowHideGunDown() {
            gunRenderer.enabled = true;
            if (player.eDirection == Direction.Left) {
                gun.transform.rotation = Quaternion.Euler(0, 0, 90f);
            } else {
                gun.transform.rotation = Quaternion.Euler(0, 0, -90f);
            }
            yield return new WaitForSeconds(0.3f);
            gun.transform.rotation = Quaternion.identity;
            gunRenderer.enabled = false;
        }
    }
}