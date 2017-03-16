using UnityEngine;

namespace SciFi.Players.Attacks {
    public class BoneArmForwardCollision : MonoBehaviour {
        public BoneArm boneArm;

        void OnTriggerEnter2D(Collider2D collider) {
            boneArm.ChildCollide(gameObject, collider);
        }

        void OnTriggerExit2D(Collider2D collider) {
            boneArm.ChildEndCollide(gameObject, collider);
        }
    }
}