using UnityEngine;
using UnityEngine.Networking;
using System;

public enum Direction {
    Left,
    Right,
}

public enum PlayerFeature {
    Movement,
    Attack,
    Damage,
    Knockback,
}

public abstract class Player : NetworkBehaviour {
    protected PlayerData data;
    protected Rigidbody2D rb;
    protected InputManager inputManager;
    private int groundCollisions;
    protected bool canJump;
    protected bool canDoubleJump;
    private float cooldownOver = 0f;
    private Direction cachedDirection;
    protected GameObject item;
    private OneWayPlatform currentOneWayPlatform;
    private int[] featureLockout;

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
        data.direction = cachedDirection = defaultDirection;
        data.lives = 3;
        var gameControllerGo = GameObject.Find("GameController");
        inputManager = gameControllerGo.GetComponent<InputManager>();

        if (isLocalPlayer) {
            inputManager.ObjectSelected += ObjectSelected;
            inputManager.ControlCanceled += ControlCanceled;
        }

        featureLockout = new int[Enum.GetNames(typeof(PlayerFeature)).Length];
    }

    protected void BaseCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            ++groundCollisions;
            canJump = true;
            canDoubleJump = false;

            var oneWay = collision.gameObject.GetComponent<OneWayPlatform>();
            if (oneWay != null) {
                currentOneWayPlatform = oneWay;
            }
        }
    }

    protected void BaseCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            if (--groundCollisions == 0) {
                canJump = false;
                canDoubleJump = true;
            }

            var oneWay = collision.gameObject.GetComponent<OneWayPlatform>();
            if (oneWay != null) {
                currentOneWayPlatform = null;
            }
        }
    }

    protected void SuspendFeature(PlayerFeature feature) {
        ++featureLockout[(int)feature];
    }

    protected void ResumeFeature(PlayerFeature feature) {
        if (featureLockout[(int)feature] > 0) {
            --featureLockout[(int)feature];
        }
    }

    private bool FeatureEnabled(PlayerFeature feature) {
        return featureLockout[(int)feature] == 0;
    }

    void HandleLeftRightInput(int control, Direction direction, bool backwards) {
        bool canSpeedUp;
        Vector3 force;
        if (backwards) {
            canSpeedUp = rb.velocity.x > -maxSpeed;
            force = transform.right * -walkForce;
        } else {
            canSpeedUp = rb.velocity.x < maxSpeed;
            force = transform.right * walkForce;
        }

        if (inputManager.IsControlActive(control) && FeatureEnabled(PlayerFeature.Movement)) {
            if (canSpeedUp) {
                rb.AddForce(force);
            }
            // Without the cached parameter, this will get triggered
            // multiple times until the direction has had a chance to sync.
            if (data.direction != direction && cachedDirection != direction) {
                cachedDirection = direction;
                CmdChangeDirection(direction);
            }
        }
    }

    private void HandleAttackInput(bool inputActive, bool canCharge, ref bool isCharging, int attackNumber) {
        Action beginCharging;
        Action<float> keepCharging;
        Action<float> endCharging;
        Action attack;
        int control;
        if (attackNumber == 1) {
            beginCharging = BeginChargingAttack1;
            keepCharging = KeepChargingAttack1;
            endCharging = EndChargingAttack1;
            attack = Attack1;
            control = Control.Attack1;
        } else if (attackNumber == 2) {
            beginCharging = BeginChargingAttack2;
            keepCharging = KeepChargingAttack2;
            endCharging = EndChargingAttack2;
            attack = Attack2;
            control = Control.Attack2;
        } else {
            throw new ArgumentException("attackNumber must be a valid attack input", "attackNumber");
        }

        if (canCharge) {
            if (isCharging) {
                if (!inputActive) {
                    isCharging = false;
                    endCharging(inputManager.GetControlHoldTime(control));
                    ResumeFeature(PlayerFeature.Attack);
                } else {
                    keepCharging(inputManager.GetControlHoldTime(control));
                }
            } else {
                if (inputActive && FeatureEnabled(PlayerFeature.Attack)) {
                    isCharging = true;
                    SuspendFeature(PlayerFeature.Attack);
                    beginCharging();
                }
            }
        } else {
            if (inputActive) {
                inputManager.InvalidateControl(control);
                if (Time.time > cooldownOver && FeatureEnabled(PlayerFeature.Attack)) {
                    cooldownOver = Time.time + attackCooldown;
                    attack();
                }
            }
        }
    }

    protected void BaseInput() {
        HandleLeftRightInput(Control.Left, Direction.Left, true);
        HandleLeftRightInput(Control.Right, Direction.Right, false);
        if (inputManager.IsControlActive(Control.Up) && FeatureEnabled(PlayerFeature.Movement)) {
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
        if (inputManager.IsControlActive(Control.Down) && FeatureEnabled(PlayerFeature.Movement)) {
            if (currentOneWayPlatform != null) {
                currentOneWayPlatform.FallThrough(gameObject);
            }
        }

        if (inputManager.IsControlActive(Control.Item) && FeatureEnabled(PlayerFeature.Attack)) {
            inputManager.InvalidateControl(Control.Item);
            if (item != null) {
                UseItem();
            } else {
                PickUpItem();
            }
        }

        var attack1Active = inputManager.IsControlActive(Control.Attack1);
        var attack2Active = inputManager.IsControlActive(Control.Attack2);
        HandleAttackInput(attack1Active, attack1CanCharge, ref attack1IsCharging, 1);
        HandleAttackInput(attack2Active, attack2CanCharge, ref attack2IsCharging, 2);
    }

    void UseItem() {
        CmdThrowItem(item);
        item = null;
    }

    [Command]
    void CmdThrowItem(GameObject item) {
        LoseOwnershipOfItem(item);
        item.layer = Layers.projectiles;

        Vector2 force;
        if (cachedDirection == Direction.Left) {
            force = new Vector2(-200f, 150f);
        } else {
            force = new Vector2(200f, 150f);
        }
        item.GetComponent<Rigidbody2D>().AddForce(force);
    }

    void PickUpItem(GameObject item = null) {
        this.item = CircleCastForItem(item);
        if (this.item != null) {
            CmdTakeOwnershipOfItem(this.item);
        }
    }

    [Command]
    void CmdTakeOwnershipOfItem(GameObject item) {
        var position = gameObject.transform.position;
        if (cachedDirection == Direction.Left) {
            position.x -= 1f;
        } else {
            position.x += 1f;
        }
        item.transform.position = position;
        var i = item.GetComponent<ItemData>().item;
        i.EnablePhysics(false);
        i.SetOwner(gameObject);
    }

    [Server]
    void MoveItemForChangeDirection(GameObject item, Direction direction) {
        float dx;
        if (direction == Direction.Left) {
            dx = -2f;
        } else {
            dx = 2f;
        }
        item.GetComponent<ItemData>().item.UpdateOwnerOffset(dx, 0f);
    }

    [Server]
    void LoseOwnershipOfItem(GameObject item) {
        var i = item.GetComponent<ItemData>().item;
        i.SetOwner(null);
        i.EnablePhysics(true);
    }

    /// If an item is passed, this function will return it
    /// only if it falls in the circle cast.
    /// If no item was passed, it will return the first item
    /// hit by the circle cast.
    GameObject CircleCastForItem(GameObject item) {
        var hits = Physics2D.CircleCastAll(
            gameObject.transform.position,
            1f,
            Vector2.zero,
            Mathf.Infinity,
            1 << Layers.items);
        if (hits.Length == 0) {
            return null;
        }

        if (item == null) {
            return hits[0].collider.gameObject;
        }

        foreach (var hit in hits) {
            if (hit.collider.gameObject == item) {
                return item;
            }
        }
        return null;
    }

    void ObjectSelected(ObjectSelectedEventArgs args) {
        if (args.gameObject == this.item) {
            UseItem();
            return;
        }

        // We can only hold one item at a time.
        if (item != null) {
            return;
        }
        if (args.gameObject.layer == Layers.items) {
            if (CircleCastForItem(args.gameObject) == args.gameObject) {
                PickUpItem(args.gameObject);
            }
        }
    }

    void ControlCanceled(ControlCanceledEventArgs args) {
        //
    }

    [Command]
    void CmdFallThrough() {
        RpcFallThrough();
    }

    [Server]
    void RpcFallThrough() {
        //
    }

    protected virtual void BeginChargingAttack1() {}
    protected virtual void KeepChargingAttack1(float chargeTime) {}
    protected virtual void EndChargingAttack1(float chargeTime) {}
    protected void CancelChargingAttack1() {
        inputManager.InvalidateControl(Control.Attack1);
        attack1IsCharging = false;
    }

    protected virtual void BeginChargingAttack2() {}
    protected virtual void KeepChargingAttack2(float chargeTime) {}
    protected virtual void EndChargingAttack2(float chargeTime) {}
    protected void CancelChargingAttack2() {
        inputManager.InvalidateControl(Control.Attack2);
        attack2IsCharging = false;
    }

    [Server]
    protected void SpawnProjectile(
        NetworkInstanceId netId,
        NetworkInstanceId extraNetId,
        GameObject prefab,
        Vector2 position,
        Vector2 force,
        float torque)
    {
        var projectile = Instantiate(prefab, position, Quaternion.identity);
        projectile.GetComponent<AppleBehavior>().spawnedBy = netId;
        projectile.GetComponent<AppleBehavior>().spawnedByExtra = extraNetId;
        var projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.AddForce(force);
        projectileRb.AddTorque(torque);
        NetworkServer.Spawn(projectile);
    }

    [ClientRpc]
    public void RpcRespawn(Vector3 position) {
        if (!hasAuthority) {
            return;
        }
        transform.position = position;
        rb.velocity = new Vector2(0f, 0f);
    }

    [Command]
    void CmdChangeDirection(Direction direction) {
        data.direction = direction;
        if (item != null) {
            MoveItemForChangeDirection(item, direction);
        }
        RpcChangeDirection(direction);
    }

    [ClientRpc]
    protected virtual void RpcChangeDirection(Direction direction) {}

    [ClientRpc]
    public void RpcKnockback(Vector2 force) {
        if (!hasAuthority) {
            return;
        }
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    // These methods are called only for attacks that don't charge.
    // For charging attacks, use the Begin/End versions above.
    protected virtual void Attack1() {}
    protected virtual void Attack2() {}
}