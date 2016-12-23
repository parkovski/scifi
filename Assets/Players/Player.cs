using UnityEngine;
using UnityEngine.Networking;

public enum Direction {
    Left,
    Right,
}

public abstract class Player : NetworkBehaviour {
    protected PlayerData data;
    protected Rigidbody2D rb;
    protected InputManager inputManager;
    private int groundCollisions;
    protected bool canJump;
    protected bool canDoubleJump;
    private float cooldownOver = 0f;

    // Unity editor parameters
    public float maxSpeed;
    public float walkForce;
    public float jumpForce;
    public float minDoubleJumpVelocity;
    public float attackCooldown;

    protected void BaseStart() {
        rb = GetComponent<Rigidbody2D>();
        data = GetComponent<PlayerData>();
        data.player = this;
        var gameControllerGo = GameObject.Find("GameController");
        inputManager = gameControllerGo.GetComponent<InputManager>();

        // TODO: Remove when GameController manages players
        GameController.Instance.RegisterNewPlayer(gameObject);
    }

    protected void BaseCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            ++groundCollisions;
            canJump = true;
            canDoubleJump = false;
        }
    }

    protected void BaseCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            if (--groundCollisions == 0) {
                canJump = false;
                canDoubleJump = true;
            }
        }
    }

    protected void BaseInput() {
        if (inputManager.IsControlActive(Control.Left)) {
            if (rb.velocity.x > -maxSpeed) {
                rb.AddForce(transform.right * -walkForce);
            }
            if (data.direction == Direction.Right) {
                CmdChangeDirection(Direction.Left);
            }
        }
        if (inputManager.IsControlActive(Control.Right)) {
            if (rb.velocity.x < maxSpeed) {
                rb.AddForce(transform.right * walkForce);
            }
            if (data.direction == Direction.Left) {
                CmdChangeDirection(Direction.Right);
            }
        }
        if (inputManager.IsControlActive(Control.Up)) {
            inputManager.InvalidateControl(Control.Up);
            if (canJump) {
                canJump = false;
                canDoubleJump = true;
                rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            } else if (canDoubleJump) {
                canDoubleJump = false;
                if (rb.velocity.y < minDoubleJumpVelocity) {
                    rb.velocity = new Vector2(rb.velocity.x, minDoubleJumpVelocity);
                }
                rb.AddForce(transform.up * jumpForce / 2, ForceMode2D.Impulse);
            }
        }

        if (inputManager.IsControlActive(Control.Attack)) {
            inputManager.InvalidateControl(Control.Attack);
            if (Time.time > cooldownOver) {
                cooldownOver = Time.time + attackCooldown;
                Attack1();
            }
        }

        if (inputManager.IsControlActive(Control.Attack2)) {
            inputManager.InvalidateControl(Control.Attack2);
            if (Time.time > cooldownOver) {
                cooldownOver = Time.time + attackCooldown;
                Attack2();
            }
        }
    }

    [Server]
    protected void SpawnProjectile(NetworkInstanceId netId, GameObject prefab, Vector2 position, Vector2 force, float torque) {
        var projectile = Instantiate(prefab, position, Quaternion.identity);
        projectile.GetComponent<AppleBehavior>().spawnedBy = netId;
        var projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.AddForce(force);
        projectileRb.AddTorque(torque);
        NetworkServer.Spawn(projectile);
    }

    protected abstract void CmdChangeDirection(Direction direction);
    public abstract void RpcKnockback(Vector2 force);
    protected abstract void Attack1();
    protected abstract void Attack2();
}