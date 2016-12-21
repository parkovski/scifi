using UnityEngine;

public class AppleBehavior : MonoBehaviour {
    public GameObject explodingApple;
    public AnimationClip explodeAnimation;

    void Start () {
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerProxy>().TakeDamage(5);
            var exploding = Instantiate(explodingApple, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);
            exploding.GetComponent<Animator>().Play("AppleExplode");
        }
    }
}
