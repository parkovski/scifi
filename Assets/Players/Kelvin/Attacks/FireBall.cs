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

        void Start() {
            BaseStart();
            Destroy(gameObject, 3f);
        }

        void Update() {
            if (!isServer) {
                return;
            }

            if (targetPlayer != null) {
                gameObject.transform.position = targetPlayer.transform.position + targetOffset;

                if (Time.time > nextDamageTime) {
                    nextDamageTime = Time.time + nextDamageWait;
                    Attack();
                }
            }
        }

        void StartAttacking() {
            rounds = Random.Range(minRounds, maxRounds + 1);
            var player = targetPlayer.GetComponent<Player>();
            player.SuspendFeature(PlayerFeature.Movement);
            player.SuspendFeature(PlayerFeature.Attack);
            nextDamageTime = Time.time + nextDamageWait;
            Attack();
        }

        void Attack() {
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
            if (collision.gameObject.tag == "Player") {
                targetPlayer = collision.gameObject;
                targetOffset = gameObject.transform.position - targetPlayer.transform.position;
                StartAttacking();
            } else {
                Destroy(gameObject);
            }
        }
    }
}