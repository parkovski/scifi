using UnityEngine;
using Random = UnityEngine.Random;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class FireBall : Projectile {
        /// How many times the player gets hit by this attack
        int rounds;
        const int minRounds = 3;
        const int maxRounds = 5;

        /// How long to wait between damage rounds
        float nextDamageTime;
        const float nextDamageWait = .5f;

        GameObject targetPlayer;
        Vector3 targetOffset;
        /// If the fireball is being held and charging, it can't
        /// be destroyed - only when it is flying.
        bool canDestroy = false;
        float destroyTime;

        void Start() {
            BaseStart();
        }

        void Update() {
            if (!isServer) {
                return;
            }

            if (canDestroy && Time.time > destroyTime) {
                Destroy(gameObject);
                return;
            }

            if (targetPlayer != null) {
                gameObject.transform.position = targetPlayer.transform.position + targetOffset;

                if (Time.time > nextDamageTime) {
                    nextDamageTime = Time.time + nextDamageWait;
                    DoAttack();
                }
            }
        }

        public void SetCanDestroy(bool canDestroy) {
            this.canDestroy = canDestroy;
            if (canDestroy) {
                destroyTime = Time.time + 3f;
            }
        }

        void StartAttacking() {
            rounds = Random.Range(minRounds, maxRounds + 1);
            var player = targetPlayer.GetComponent<Player>();
            player.SuspendFeature(PlayerFeature.Movement);
            player.SuspendFeature(PlayerFeature.Attack);
            nextDamageTime = Time.time + nextDamageWait;
            DoAttack();
        }

        void DoAttack() {
            if (rounds <= 0) {
                StopAttacking();
                return;
            }

            --rounds;
            GameController.Instance.TakeDamage(targetPlayer, 3);
            GameController.Instance.Knockback(gameObject, targetPlayer, 3f);
        }

        void StopAttacking() {
            var player = targetPlayer.GetComponent<Player>();
            player.ResumeFeature(PlayerFeature.Movement);
            player.ResumeFeature(PlayerFeature.Attack);

            targetPlayer = null;
            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                targetPlayer = collision.gameObject;
                targetOffset = gameObject.transform.position - targetPlayer.transform.position;
                StartAttacking();
            } else if (canDestroy) {
                Destroy(gameObject);
            }
        }
    }
}