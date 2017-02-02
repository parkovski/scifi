using UnityEngine;

namespace SciFi.Items {
    public class PotionJuice : Projectile {
        public Sprite[] juiceStages;
        SpriteRenderer spriteRenderer;
        int stage = 0;
        float nextStageTime = Mathf.Infinity;
        const float changeStageTime = 0.02f;

        void Start() {
            BaseStart();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update() {
            if (Time.time > nextStageTime) {
                ++stage;
                if (stage < juiceStages.Length) {
                    spriteRenderer.sprite = juiceStages[stage];
                    nextStageTime = Time.time + changeStageTime;
                } else {
                    nextStageTime = Mathf.Infinity;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (stage == 0 && collision.gameObject.tag == "Ground") {
                nextStageTime = Time.time;
            }

            if (!isServer) {
                return;
            }
            GameController.Instance.TakeDamage(collision.gameObject, 10);
            GameController.Instance.Knockback(gameObject, collision.gameObject, 3f);
        }
    }
}