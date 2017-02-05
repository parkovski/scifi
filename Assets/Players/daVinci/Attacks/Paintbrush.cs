using UnityEngine;
using System.Collections.Generic;

namespace SciFi.Players.Attacks {
    public class Paintbrush : MonoBehaviour, IAttack {
        public GameObject paintStreakPrefab;
        Direction direction = Direction.Left;
        bool isAttacking = false;
        HashSet<GameObject> hitObjects = new HashSet<GameObject>();
        SpriteRenderer spriteRenderer;

        static readonly Color[] colors = new[] {
            new Color(0.0980f, 0.1019f, 0.6392f, 1f),
            new Color(0.0705f, 0.3882f, 0.1137f, 1f),
            new Color(0.2274f, 0.5176f, 0.5568f, 1f),
            new Color(0.6039f, 0.4196f, 0.0274f, 1f),
            new Color(0.5019f, 0.0784f, 0.0078f, 1f),
            new Color(0.3490f, 0.0274f, 0.4392f, 1f),
            new Color(0.2862f, 0.5372f, 0.0431f, 1f),
        };

        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = false;
        }

        public void StartAttacking() {
            isAttacking = true;
            spriteRenderer.enabled = true;
        }

        public void StopAttacking() {
            isAttacking = false;
            spriteRenderer.enabled = false;
            hitObjects.Clear();
        }

        public void SetDirection(Direction direction) {
            this.direction = direction;
        }

        public Quaternion GetStreakRotation() {
            if (direction == Direction.Left) {
                return Quaternion.Euler(0f, 0f, -30f);
            } else {
                return Quaternion.Euler(0f, 0f, 30f);
            }
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (!isAttacking) {
                return;
            }

            if (hitObjects.Contains(collider.gameObject)) {
                return;
            }
            hitObjects.Add(collider.gameObject);

            var point = collider.bounds.ClosestPoint(transform.position);
            var streak = Instantiate(paintStreakPrefab, point, GetStreakRotation());
            var material = streak.GetComponent<SpriteRenderer>().material;
            material.SetColor("_Color", colors[Random.Range(0, colors.Length)]);
            material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
            StartCoroutine(DestroyStreak(streak));
        }

        System.Collections.IEnumerator DestroyStreak(GameObject streak) {
            yield return new WaitForSeconds(2f);
            Destroy(streak);
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}