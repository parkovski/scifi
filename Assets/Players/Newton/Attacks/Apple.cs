using UnityEngine;
using UnityEngine.Networking;

public class Apple : Projectile {
    public GameObject explodingApple;

    void Start () {
        BaseStart();
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (!isServer) {
            return;
        }
        if (collision.gameObject.tag == "Player") {
            GameController.Instance.TakeDamage(collision.gameObject, 5);
            GameController.Instance.Knockback(gameObject, collision.gameObject, 5f);
            var exploding = Instantiate(explodingApple, gameObject.transform.position, gameObject.transform.rotation);
            NetworkServer.Spawn(exploding);
            Destroy(gameObject);
        }
    }
}