using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Telegraph : MonoBehaviour, IAttack {
        public GameObject spawnedBy;

        SpriteRenderer electricity1, electricity2;
        float swapTime;
        const float frameTime = .1f;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
            electricity1 = gameObject.transform.Find("Electricity1").GetComponent<SpriteRenderer>();
            electricity2 = gameObject.transform.Find("Electricity2").GetComponent<SpriteRenderer>();
            electricity2.enabled = false;
            swapTime = Time.time + frameTime;
        }

        void Update() {
            if (Time.time > swapTime) {
                swapTime = Time.time + frameTime;
                electricity1.enabled = !electricity1.enabled;
                electricity2.enabled = !electricity2.enabled;
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                GameController.Instance.Hit(collision.gameObject, this, gameObject, 5, 2f);
                Destroy(gameObject);
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.Electric; } }
    }
}