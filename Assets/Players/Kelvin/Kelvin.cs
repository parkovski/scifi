using UnityEngine;
using UnityEngine.Networking;

public class Kelvin : Player {
    public GameObject iceBall;
    public GameObject fireBall;

    const float iceBallHorizontalForce = 200f;
    const float iceBallVerticalForce = 100f;
    const float iceBallTorqueRange = 10f;

    const float fireBallHorizontalForce = 50f;

    void Start() {
        BaseStart();
    }

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        BaseInput();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision) {
        BaseCollisionExit2D(collision);
    }

    [Command]
    protected override void CmdChangeDirection(Direction direction) {
        data.direction = direction;
    }

    [Command]
    void CmdSpawnIceBall(NetworkInstanceId netId, bool down) {
        Vector2 force;
        if (down) {
            force = -transform.up * iceBallHorizontalForce;
        } else {
            force = transform.right;
            if (data.direction == Direction.Left) {
                force *= -iceBallHorizontalForce;
            } else {
                force *= iceBallHorizontalForce;
            }
            force += (Vector2)transform.up * iceBallVerticalForce;
        }
        var torque = Random.Range(-iceBallTorqueRange, iceBallTorqueRange);
        SpawnProjectile(netId, iceBall, gameObject.transform.position, force, torque);
    }

    [Command]
    void CmdSpawnFireBall(NetworkInstanceId netId) {
        var force = transform.right;
        if (data.direction == Direction.Left) {
            force *= -fireBallHorizontalForce;
        } else {
            force *= fireBallHorizontalForce;
        }
        SpawnProjectile(netId, fireBall, gameObject.transform.position, force, 0f);
    }

    [ClientRpc]
    public override void RpcKnockback(Vector2 force) {
        if (!isLocalPlayer) {
            return;
        }
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    protected override void Attack1() {
        CmdSpawnIceBall(netId, inputManager.IsControlActive(Control.Down));
    }

    protected override void Attack2() {
        CmdSpawnFireBall(netId);
    }
}