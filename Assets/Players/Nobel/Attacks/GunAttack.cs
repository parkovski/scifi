using UnityEngine;
using System.Collections;

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
        }

        IEnumerator ShowHideGun() {
            gunRenderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
            gunRenderer.enabled = false;
        }
    }
}