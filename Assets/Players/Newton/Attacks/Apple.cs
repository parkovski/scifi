using UnityEngine;
using UnityEngine.Networking;

public class Apple : Projectile {
    public GameObject explodingApple;

    /// After the apple hits the ground, it causes less damage.
    bool hasHitGround = false;

    void Start () {
        BaseStart();
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (!isServer) {
            return;
        }
        if (collision.gameObject.tag == "Ground") {
            hasHitGround = true;
        } else if (collision.gameObject.tag == "Player") {
            var damage = hasHitGround ? 1 : 5;
            var knockback = hasHitGround ? 1f : 3f;
            GameController.Instance.TakeDamage(collision.gameObject, damage);
            GameController.Instance.Knockback(gameObject, collision.gameObject, knockback);
            var exploding = Instantiate(explodingApple, gameObject.transform.position, gameObject.transform.rotation);
            NetworkServer.Spawn(exploding);
            Destroy(gameObject);
        }
    }
}