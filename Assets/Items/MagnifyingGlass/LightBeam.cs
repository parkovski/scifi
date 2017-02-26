using UnityEngine;

using SciFi.Players.Attacks;

namespace SciFi.Items {
    public class LightBeam : MonoBehaviour, IAttackSource {
        float startTime;
        float stopTime;
        bool hasSetStopTime = false;
        float nextHitTime;
        const float hitTimeout = .1f;
        public GameObject magnifyingGlassGo;
        MagnifyingGlass magnifyingGlass;
        public bool backwards;
        Vector3 offset;
        SpriteRenderer spriteRenderer;

        void Start() {
            startTime = Time.time;
            stopTime = startTime + .15f;
            offset = gameObject.transform.position - magnifyingGlassGo.transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
            magnifyingGlass = magnifyingGlassGo.GetComponent<MagnifyingGlass>();
        }

        void Update() {
            var time = Time.time;
            if (time > startTime + .5f) {
                Destroy(gameObject);
                return;
            }

            if (time > stopTime) {
                time = stopTime;
            }

            var scale = (time - startTime) * 2000;
            transform.localScale = new Vector3(scale, transform.localScale.y, 1f);
            transform.position = magnifyingGlassGo.transform.position + offset;

            // Use Time.time here, because time is locked at first hit.
            if (nextHitTime < Time.time) {
                var distance = Mathf.Sqrt(Mathf.Pow(spriteRenderer.bounds.extents.x, 2f) + Mathf.Pow(spriteRenderer.bounds.extents.y, 2f));
                var zRot = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                Vector2 angle;
                if (backwards) {
                    angle = new Vector2(-Mathf.Cos(zRot), Mathf.Sin(zRot));
                } else {
                    angle = new Vector2(Mathf.Cos(zRot), Mathf.Sin(zRot));
                }
                var hit = Physics2D.Raycast(
                    transform.position,
                    angle,
                    distance,
                    Attack.LayerMask,
                    0f
                );

                if (hit) {
                    if (!hasSetStopTime) {
                        hasSetStopTime = true;
                        stopTime = time + .02f;
                    }
                    nextHitTime = Time.time + hitTimeout;
                    magnifyingGlass.Hit(hit.collider.gameObject, this);
                }
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.LightBeam; } }
    }
}