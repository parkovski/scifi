using UnityEngine;
using UnityEngine.Networking;

public class Kelvin : Player {
    public GameObject iceBall;
    public GameObject fireBall;
    public GameObject fireBallInactive;

    private GameObject chargingFireBall;

    const float iceBallHorizontalForce = 200f;
    const float iceBallVerticalForce = 100f;
    const float iceBallTorqueRange = 10f;

    const float fireBallHorizontalForce = 50f;

    void Start() {
        attack2CanCharge = true;

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
    void CmdSpawnChargingFireBall() {
        var offset = data.direction == Direction.Left ? new Vector3(-.5f, .4f) : new Vector3(.5f, .4f);
        chargingFireBall = Instantiate(fireBallInactive, gameObject.transform.position + offset, Quaternion.identity);
        chargingFireBall.transform.parent = gameObject.transform;
        NetworkServer.Spawn(chargingFireBall);
    }

    [Command]
    void CmdDestroyChargingFireBall() {
        Destroy(chargingFireBall);
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

    protected override void Attack1() {
        CmdSpawnIceBall(netId, inputManager.IsControlActive(Control.Down));
    }

    protected override void BeginChargingAttack2() {
        CmdSpawnChargingFireBall();
    }

    protected override void EndChargingAttack2(float chargeTime) {
        CmdDestroyChargingFireBall();
    }
}