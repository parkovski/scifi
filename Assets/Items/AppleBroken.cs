using UnityEngine;

public class AppleBroken : MonoBehaviour {
    void Start() {
        GetComponent<Animator>().Play("AppleExplode");
    }
}