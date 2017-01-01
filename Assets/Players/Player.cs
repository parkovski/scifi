using UnityEngine;
using UnityEngine.Networking;
using System;

public enum Direction {
    Left,
    Right,
    Up,
    Down,
}

public enum PlayerFeature {
    Movement,
    Attack,
    Damage,
    Knockback,
}

public enum PlayerAttack {
    Attack1,
    Attack2,
    SpecialAttack,
    Item,
}

public abstract class Player : NetworkBehaviour {
    [SyncVar]
    public int id;
    [SyncVar]
    public string displayName;
    [SyncVar]
    public int lives;
    [SyncVar]
    public int damage;
    [SyncVar]
    public Direction direction;

    protected Rigidbody2D rb;
    protected InputManager inputManager;
    private int groundCollisions;
    protected bool canJump;
    protected bool canDoubleJump;
    protected GameObject item;
    private OneWayPlatform currentOneWayPlatform;
    private int[] featureLockout;

    // Unity editor parameters
    public Direction defaultDirection;
    public float maxSpeed;
    public float walkForce;
    public float jumpForce;
    public float minDoubleJumpVelocity;

    // Parameters for child classes to change behavior
    protected Attack attack1;
    protected Attack attack2;
    protected Attack specialAttack;
    //protected Attack superAttack;

    protected void BaseStart() {
        rb = GetComponent<Rigidbody2D>();
        direction = Direction.Right;
        lives = 3;
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

    public void SuspendFeature(PlayerFeature feature) {
        ++featureLockout[(int)feature];
    }

    public void ResumeFeature(PlayerFeature feature) {
        if (featureLockout[(int)feature] > 0) {
            --featureLockout[(int)feature];
        }
    }

    public bool FeatureEnabled(PlayerFeature feature) {
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
            if (this.direction != direction) {
                this.direction = direction;
                CmdChangeDirection(direction);
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

        attack1.UpdateState(inputManager, Control.Attack1);
        attack2.UpdateState(inputManager, Control.Attack2);
        specialAttack.UpdateState(inputManager, Control.SpecialAttack);
    }

    public NetworkInstanceId GetItemNetId() {
        return item == null ? NetworkInstanceId.Invalid : item.GetComponent<Item>().netId;
    }

    void UseItem() {
        var i = item.GetComponent<Item>();
        if (i.ShouldThrow()) {
            CmdThrowItem(item);
            item = null;
        } else if (i.ShouldCharge()) {
            // TODO: begin charging item
            i.Use(direction, netId);
        } else {
            i.Use(direction, netId);
        }
    }

    [Command]
    void CmdThrowItem(GameObject item) {
        RpcItemDiscard(item);
        item.layer = Layers.projectiles;

        Vector2 force;
        if (direction == Direction.Left) {
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
        if (direction == Direction.Left) {
            position.x -= 1f;
        } else {
            position.x += 1f;
        }
        item.transform.position = position;
        RpcItemPickup(item);
    }

    [ClientRpc]
    void RpcItemPickup(GameObject item) {
        var i = item.GetComponent<Item>();
        i.EnablePhysics(false);
        i.SetOwner(gameObject);
        i.OnPickup(this);
    }

    [ClientRpc]
    void RpcItemDiscard(GameObject item) {
        var i = item.GetComponent<Item>();
        i.SetOwner(null);
        i.EnablePhysics(true);
        i.OnDiscard(this);
    }

    [Server]
    void MoveItemForChangeDirection(GameObject item, Direction direction) {
        float dx;
        if (direction == Direction.Left) {
            dx = -2f;
        } else {
            dx = 2f;
        }
        item.GetComponent<Item>().UpdateOwnerOffset(dx, 0f);
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
        this.direction = direction;
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
        if (!FeatureEnabled(PlayerFeature.Knockback)) {
            return;
        }
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}