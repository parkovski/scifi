﻿using UnityEngine;
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
    public Direction defaultDirection;
    public float maxSpeed;
    public float walkForce;
    public float jumpForce;
    public float minDoubleJumpVelocity;
    public float attackCooldown;

    // Parameters for child classes to change behavior
    protected bool attack1CanCharge = false;
    private bool attack1IsCharging = false;
    protected bool attack2CanCharge = false;
    private bool attack2IsCharging = false;

    protected void BaseStart() {
        rb = GetComponent<Rigidbody2D>();
        data = GetComponent<PlayerData>();
        data.player = this;
        data.direction = defaultDirection;
        var gameControllerGo = GameObject.Find("GameController");
        inputManager = gameControllerGo.GetComponent<InputManager>();

        if (isLocalPlayer) {
            inputManager.ObjectSelected += ObjectSelected;
            inputManager.ControlCanceled += ControlCanceled;
        }

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

        var attack1Active = inputManager.IsControlActive(Control.Attack1);
        if (attack1CanCharge) {
            // For chargeable attacks, fire events for button state change.
            if (attack1IsCharging) {
                if (!attack1Active) {
                    attack1IsCharging = false;
                    EndChargingAttack1(inputManager.GetControlHoldTime(Control.Attack1));
                }
            } else {
                if (attack1Active) {
                    attack1IsCharging = true;
                    BeginChargingAttack1();
                }
            }
        } else {
            if (attack1Active) {
                // For non-chargeable attacks, invalidate the button
                // so you have to release and press it again to attack again.
                inputManager.InvalidateControl(Control.Attack1);
                if (Time.time > cooldownOver) {
                    cooldownOver = Time.time + attackCooldown;
                    Attack1();
                }
            }
        }

        var attack2Active = inputManager.IsControlActive(Control.Attack2);
        if (attack2CanCharge) {
            if (attack2IsCharging) {
                if (!attack2Active) {
                    attack2IsCharging = false;
                    EndChargingAttack2(inputManager.GetControlHoldTime(Control.Attack2));
                }
            } else {
                if (attack2Active) {
                    attack2IsCharging = true;
                    BeginChargingAttack2();
                }
            }
        } else {
            if (attack2Active) {
                inputManager.InvalidateControl(Control.Attack2);
                if (Time.time > cooldownOver) {
                    cooldownOver = Time.time + attackCooldown;
                    Attack2();
                }
            }
        }
    }

    void ObjectSelected(ObjectSelectedEventArgs args) {
        if (args.gameObject.name.StartsWith("Bomb")) {
            DebugPrinter.Instance.SetField(DebugPrinter.Instance.NewField(), "bomb selected");
        }
    }

    void ControlCanceled(ControlCanceledEventArgs args) {
        //
    }

    protected virtual void BeginChargingAttack1() {}
    protected virtual void EndChargingAttack1(float chargeTime) {}
    protected void CancelChargingAttack1() {
        inputManager.InvalidateControl(Control.Attack1);
        attack1IsCharging = false;
    }

    protected virtual void BeginChargingAttack2() {}
    protected virtual void EndChargingAttack2(float chargeTime) {}
    protected void CancelChargingAttack2() {
        inputManager.InvalidateControl(Control.Attack2);
        attack2IsCharging = false;
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

    // These methods are called only for attacks that don't charge.
    // For charging attacks, use the Begin/End versions above.
    protected virtual void Attack1() {}
    protected virtual void Attack2() {}
}