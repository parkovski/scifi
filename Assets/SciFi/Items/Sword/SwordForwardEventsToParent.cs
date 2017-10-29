using UnityEngine;

namespace SciFi.Items {
    public class SwordForwardEventsToParent : MonoBehaviour {
        public Sword sword;

        public void StartAttacking() {
            sword.StartAttacking();
        }

        public void StopAttacking() {
            sword.StopAttacking();
        }
    }
}