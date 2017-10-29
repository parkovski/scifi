using UnityEngine;

namespace SciFi.Players.Attacks {
    public class DynamiteFragmentForwardCollision : MonoBehaviour {
        public DynamiteFragment parent;

        void OnCollisionEnter2D(Collision2D collision) {
            parent.ChildCollide(gameObject, collision);
        }
    }
}