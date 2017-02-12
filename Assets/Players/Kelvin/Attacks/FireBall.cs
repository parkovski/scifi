using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

using SciFi.Items;
using SciFi.Players.Modifiers;

namespace SciFi.Players.Attacks {
    public class FireBall : Projectile {
        /// How many times the player gets hit by this attack
        int rounds;
        const int minRounds = 3;
        const int maxRounds = 5;

        /// How long to wait between damage rounds
        float nextDamageTime;
        const float nextDamageWait = .1f;

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

        [Server]
        void StartAttacking() {
            rounds = Random.Range(minRounds, maxRounds + 1);
            var player = targetPlayer.GetComponent<Player>();
            player.AddModifier(Modifier.CantMove);
            player.AddModifier(Modifier.CantAttack);
            player.AddModifier(Modifier.OnFire);
            nextDamageTime = Time.time + nextDamageWait;
            DoAttack();
        }

        [Server]
        void DoAttack() {
            if (rounds <= 0) {
                StopAttacking();
                return;
            }

            --rounds;
            GameController.Instance.Hit(targetPlayer, this, gameObject, 3, 3f);
        }

        [Server]
        void StopAttacking() {
            var player = targetPlayer.GetComponent<Player>();
            player.RemoveModifier(Modifier.CantMove);
            player.RemoveModifier(Modifier.CantAttack);
            player.RemoveModifier(Modifier.OnFire);

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

        public override AttackProperty Properties {
            get {
                return AttackProperty.OnFire;
            }
        }
    }
}