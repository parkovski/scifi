using UnityEngine;
using UnityEngine.Networking;

public class AppleBehavior : NetworkBehaviour {
    public GameObject explodingApple;

    [SyncVar]
    public NetworkInstanceId spawnedBy;

    void Start () {
        Destroy(gameObject, 3f);
    }

    public override void OnStartClient() {
        // Don't let this object hit the player that created it.
        var player = ClientScene.FindLocalObject(spawnedBy);
        var appleColliders = gameObject.GetComponents<Collider2D>();
        var playerColliders = player.GetComponents<Collider2D>();
        foreach (var playerColl in playerColliders) {
            foreach (var appleColl in appleColliders) {
                Physics2D.IgnoreCollision(playerColl, appleColl);
            }
        }
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