using UnityEngine;

namespace SciFi.Items {
    public class PotionJuice : Projectile {
        public Sprite[] juiceStages;
        public Sprite[] redJuiceStages;
        [HideInInspector]
        public bool isRedPotion;
        SpriteRenderer spriteRenderer;
        int stage = 0;
        float nextStageTime = Mathf.Infinity;
        const float changeStageTime = 0.02f;
        const float stayOnGroundTime = 0.3f;
        bool fading;
        int alpha;
        const float maxAlpha = 30f;

        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (isRedPotion) {
                spriteRenderer.sprite = redJuiceStages[0];
            }
        }

        void Update() {
            if (Time.time > nextStageTime) {
                ++stage;
                if (fading) {
                    if (--alpha == 0) {
                        Destroy(gameObject);
                    } else {
                        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, ((float)alpha) / maxAlpha);
                        nextStageTime = Time.time + changeStageTime;
                    }
                } else {
                    var stages = isRedPotion ? redJuiceStages : juiceStages;
                    if (stage < stages.Length) {
                        spriteRenderer.sprite = stages[stage];
                        nextStageTime = Time.time + changeStageTime;
                    } else {
                        nextStageTime = Time.time + stayOnGroundTime;
                        alpha = (int)maxAlpha;
                        fading = true;
                    }
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
            GameController.Instance.Hit(collision.gameObject, this, gameObject, 10, 3f);
        }
    }
}