using UnityEngine;

using SciFi.Environment.Effects;

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
                Effects.Smoke(transform.position + GetRandomOffset());
            }
        }

        Vector3 GetRandomOffset() {
            var x = Random.Range(-.1f, .1f);
            var y = Random.Range(-.2f, 0f);
            return new Vector3(x, y, 0f);
        }
    }
}