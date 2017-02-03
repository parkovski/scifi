using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class FlyingMachine : Projectile {
        public Sprite unmovingProp;
        public Sprite[] movingProps;

        [HideInInspector]
        public float dx;
        [HideInInspector]
        public float y;
        Rigidbody2D rb;
        float initialTime;
        SpriteRenderer spriteRenderer;
        const float movingPropSpriteTime = .03f;
        float changePropSpriteTime;
        int propIndex = 0;

        void Start() {
            BaseStart();
            y = transform.position.y;
            initialTime = Time.time;
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = transform.Find("FlyingMachine_Prop").GetComponent<SpriteRenderer>();
            changePropSpriteTime = Time.time + .3f;
            Destroy(gameObject, 5f);
        }

        void Update() {
            /// For x=t and y=(t-.5)^4, set velocity to their derivatives.
            float t = (Time.time - initialTime) * 4;
            rb.velocity = new Vector2(t*dx, Mathf.Pow((t - 1.25f) * .8f, 3) / 3f);

            if (Time.time > changePropSpriteTime) {
                changePropSpriteTime = Time.time + movingPropSpriteTime;
                spriteRenderer.sprite = movingProps[propIndex];
                if (++propIndex >= movingProps.Length) {
                    propIndex = 0;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (collider.gameObject.layer == Layers.players) {
                print("hitting player");
            }
        }
    }
}