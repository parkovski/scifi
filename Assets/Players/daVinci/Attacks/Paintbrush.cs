using UnityEngine;

namespace SciFi.Players.Attacks {
    public class Paintbrush : MonoBehaviour, IAttackSource {
        public GameObject paintDropPrefab;
        [HideInInspector]
        public daVinci player;
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

        public void Show() {
            spriteRenderer.enabled = true;
        }

        public void Hide() {
            spriteRenderer.enabled = false;
        }

        public void ThrowPaint() {
            player.CmdSpawnPaintDrops(colors[Random.Range(0, colors.Length)]);
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.None; } }
    }
}