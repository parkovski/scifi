using UnityEngine;
using UnityEngine.Networking;

public class AppleBehavior : NetworkBehaviour {
    public GameObject explodingApple;

    [SyncVar]
    public NetworkInstanceId spawnedBy;
    [SyncVar]
    public NetworkInstanceId spawnedByExtra = NetworkInstanceId.Invalid;

    void Start () {
        Destroy(gameObject, 3f);
    }

    public override void OnStartClient() {
        // Don't let this object hit the player that created it.
        IgnoreCollisions(spawnedBy);

        if (spawnedByExtra != NetworkInstanceId.Invalid) {
            IgnoreCollisions(spawnedByExtra);
        }
    }

    void IgnoreCollisions(NetworkInstanceId id) {
        var obj = ClientScene.FindLocalObject(id);
        var projectileColliders = gameObject.GetComponents<Collider2D>();
        var objectColliders = obj.GetComponents<Collider2D>();
        foreach (var objColl in objectColliders) {
            foreach (var projColl in projectileColliders) {
                Physics2D.IgnoreCollision(objColl, projColl);
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