using UnityEngine;
using System;

namespace SciFi.Items {
    public class CalcBook : MonoBehaviour {
        public GameObject spawnedBy;
        public int power;
        public Action finishAttack;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, power * 2);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 1f * power);
                finishAttack();
            }
        }
    }
}