using UnityEngine;

namespace SciFi.Items {
    public class JetpackFire : MonoBehaviour {
        public Sprite[] fires;

        SpriteRenderer spriteRenderer;
        const float fireLifetime = .1f;
        float changeFireTime;
        int fireIndex;

        void Start() {
            changeFireTime = Time.time + fireLifetime;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update() {
            if (Time.time > changeFireTime) {
                changeFireTime = Time.time + fireLifetime;
                if (++fireIndex >= fires.Length) {
                    fireIndex = 0;
                }
                spriteRenderer.sprite = fires[fireIndex];
            }
        }
    }
}