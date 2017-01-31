using UnityEngine;
using System.Collections;

using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class GunAttack : Attack {
        GameObject gun;
        GameObject bulletPrefab;
        SpriteRenderer gunRenderer;

        public GunAttack(Player player, GameObject gun, GameObject bulletPrefab)
            : base(player, false)
        {
            this.gun = gun;
            this.bulletPrefab = bulletPrefab;

            gunRenderer = gun.GetComponent<SpriteRenderer>();
            gunRenderer.enabled = false;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            player.StartCoroutine(ShowHideGun());
            var bulletGo = Object.Instantiate(bulletPrefab, gun.transform.position + GetBulletOffset(direction), Quaternion.identity);
            Effects.Smoke(bulletGo.transform.position);
            if (direction == Direction.Left) {
                bulletGo.GetComponent<SpriteRenderer>().flipX = true;
            }
            bulletGo.GetComponent<Rigidbody2D>().AddForce(GetBulletForce(direction));
            var bullet = bulletGo.GetComponent<Bullet>();
            bullet.spawnedBy = player.netId;
            bullet.spawnedByExtra = player.GetItemNetId();
        }

        Vector2 GetBulletForce(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector2(-600f, 0f);
            } else {
                return new Vector2(600f, 0f);
            }
        }

        Vector3 GetBulletOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.3f, -.1f);
            } else {
                return new Vector3(.3f, -.1f);
            }
        }

        IEnumerator ShowHideGun() {
            gunRenderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
            gunRenderer.enabled = false;
        }
    }
}