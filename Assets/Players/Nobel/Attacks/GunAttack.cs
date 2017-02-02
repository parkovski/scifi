using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class GunAttack : Attack {
        GameObject gun;
        GameObject bulletPrefab;
        SpriteRenderer gunRenderer;
        AudioSource audioSource;

        public GunAttack(Player player, GameObject gun, GameObject bulletPrefab)
            : base(player, false)
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
                rotation = Quaternion.Euler(0, 0, 90f);
            } else {
                player.StartCoroutine(ShowHideGun());
            }
            var bulletGo = Object.Instantiate(bulletPrefab, gun.transform.position + GetBulletOffset(direction), Quaternion.identity);
            Effects.Smoke(bulletGo.transform.position);
            audioSource.Play();
            if (direction == Direction.Left) {
                bulletGo.GetComponent<SpriteRenderer>().flipX = true;
            }
            var bullet = bulletGo.GetComponent<Bullet>();
            bullet.AddInitialForce(GetBulletForce(direction));
            bullet.spawnedBy = player.netId;
            bullet.spawnedByExtra = player.GetItemNetId();
            NetworkServer.Spawn(bulletGo);
        }

        Vector2 GetBulletForce(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector2(-600f, 0f);
            } else if (direction == Direction.Right) {
                return new Vector2(600f, 0f);
            } else {
                // Down
                return new Vector2(0f, -600f);
            }
        }

        Vector3 GetBulletOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.3f, -.1f);
            } else if (direction == Direction.Right) {
                return new Vector3(.3f, -.1f);
            } else {
                // Down
                return new Vector3(-.3f, -.1f);
            }
        }

        IEnumerator ShowHideGun() {
            gunRenderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
            gunRenderer.enabled = false;
        }

        IEnumerator ShowHideGunDown() {
            gunRenderer.enabled = true;
            gun.transform.rotation = Quaternion.Euler(0, 0, 90f);
            yield return new WaitForSeconds(0.3f);
            gun.transform.rotation = Quaternion.identity;
            gunRenderer.enabled = false;
        }
    }
}