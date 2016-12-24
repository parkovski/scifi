using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class Newton : Player {
    public GameObject apple;

    // Physics parameters
    const float appleHorizontalForce = 50f;
    const float appleVerticalForce = 20f;
    // Torque is random from (-appleTorqueRange, appleTorqueRange).
    const float appleTorqueRange = 5f;

    void Start() {
        BaseStart();
    }

    public override void OnStartLocalPlayer() {
        //GetComponent<SpriteRenderer>().color = new Color(.8f, .9f, 1f, .8f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision) {
        BaseCollisionExit2D(collision);
    }

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        BaseInput();
    }

    [Command]
    protected override void CmdChangeDirection(Direction direction) {
        data.direction = direction;
        //RpcChangeDirection();
        //gameObject.transform.rotation.SetFromToRotation(Vector3.right, Vector3.left);
        //gameObject.transform.rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.left);
        foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
            sr.flipX = !sr.flipX;
        }
        for (var i = 0; i < gameObject.transform.childCount; i++) {
            var child = gameObject.transform.GetChild(i);
            child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
        }
    }

    [ClientRpc]
    void RpcChangeDirection() {
        if (!isLocalPlayer) {
            return;
        }
    }

    [Command]
    void CmdSpawnApple(NetworkInstanceId netId, bool down) {
        var newApple = Instantiate(apple, gameObject.transform.position, Quaternion.identity);
        newApple.GetComponent<AppleBehavior>().spawnedBy = netId;
        var appleRb = newApple.GetComponent<Rigidbody2D>();
        var force = transform.right * appleHorizontalForce;
        if (down) {
            force = -transform.up * appleHorizontalForce;
        } else {
            if (data.direction == Direction.Left) {
                force = -force;
            }
            appleRb.AddForce(transform.up * appleVerticalForce);
        }
        appleRb.AddForce(force);
        appleRb.AddTorque(Random.Range(-appleTorqueRange, appleTorqueRange));
        NetworkServer.Spawn(newApple);
    }

    [ClientRpc]
    public override void RpcKnockback(Vector2 force) {
        if (!hasAuthority) {
            return;
        }
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    protected override void Attack1() {
        CmdSpawnApple(netId, inputManager.IsControlActive(Control.Down));
    }

    protected override void Attack2() {
        //
    }
}