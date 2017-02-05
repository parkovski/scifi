using UnityEngine;
using System.Collections.Generic;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class Paintbrush : MonoBehaviour, IAttack {
        public GameObject paintStreakPrefab;
        Direction direction = Direction.Left;
        int power = 1;
        bool isAttacking = false;
        HashSet<GameObject> hitObjects = new HashSet<GameObject>();
        SpriteRenderer spriteRenderer;
        Collider2D brushCollider;
        Vector3 paintHeightOffset;

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
            brushCollider = GetComponent<Collider2D>();
            paintHeightOffset = new Vector3(0f, -paintStreakPrefab.GetComponent<SpriteRenderer>().bounds.extents.y);
        }

        public void StartAttacking() {
            isAttacking = true;
            spriteRenderer.enabled = true;
        }

        public void StopAttacking() {
            isAttacking = false;
            hitObjects.Clear();
        }

        public void Hide() {
            spriteRenderer.enabled = false;
        }

        public void SetDirection(Direction direction) {
            this.direction = direction;
        }

        /// Power is from 0-10.
        public void SetPower(int power) {
            this.power = power;
        }

        public Quaternion GetStreakRotation() {
            if (direction == Direction.Left) {
                return Quaternion.Euler(0f, 0f, -30f);
            } else {
                return Quaternion.Euler(0f, 0f, 30f);
            }
        }

        Vector3 GetPaintbrushTipOffset() {
            return new Vector3(.2f, 0f).FlipDirection(direction);
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (!isAttacking) {
                return;
            }

            if (hitObjects.Contains(collider.gameObject)) {
                return;
            }
            hitObjects.Add(collider.gameObject);

            var point = brushCollider.bounds.center + paintHeightOffset + GetPaintbrushTipOffset();
            var streak = Instantiate(paintStreakPrefab, point, GetStreakRotation());
            streak.GetComponent<PaintStreak>().paintedObject = collider.gameObject;
            var material = streak.GetComponent<SpriteRenderer>().material;
            material.SetColor("_Color", colors[Random.Range(0, colors.Length)]);
            material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
            material.SetFloat("_Width", .025f * power + .05f);
            if (power < 4) {
                material.SetInt("_Peaks", 3);
            } else if (power > 6) {
                material.SetInt("_Peaks", 7);
            }

            GameController.Instance.Hit(collider.gameObject, this, gameObject, power, power);
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}