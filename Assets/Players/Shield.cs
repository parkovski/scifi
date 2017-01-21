using UnityEngine;

namespace SciFi.Players {
    public class Shield : MonoBehaviour {
        public Player owner;
        public GameObject brokenShield;

        SpriteRenderer spriteRenderer;
        new Collider2D collider;

        bool active;

        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            collider = GetComponent<PolygonCollider2D>();

            Deactivate();
        }

        public void Activate() {
            spriteRenderer.enabled = true;
            collider.enabled = true;
            active = true;
        }

        public void Deactivate() {
            spriteRenderer.enabled = false;
            collider.enabled = false;
            active = false;
        }

        public bool IsActive() {
            return active;
        }
    }
}