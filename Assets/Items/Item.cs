using UnityEngine;
using UnityEngine.Networking;

public abstract class Item : NetworkBehaviour {
    public bool isBeingThrown = false;

    /// The item's owner, if any, that it will follow.
    private GameObject owner;
    private Player playerOwner;
    [SyncVar]
    private Vector3 ownerOffset;

    private float aliveTime;
    [SyncVar]
    private float destroyTime;
    /// Items won't destroy when they are owned, but
    /// if they are discarded, they will only stick around
    /// for this much time if their original lifetime has expired already.
    const float aliveTimeAfterPickup = 3.5f;

    protected void BaseStart(float aliveTime = 5f) {
        this.aliveTime = aliveTime;
        this.destroyTime = Time.time + aliveTime;
    }

    protected void BaseUpdate() {
        if (!isServer) {
            return;
        }
        if (owner != null) {
            gameObject.transform.position = owner.transform.position + ownerOffset;
        }

        if (this.destroyTime < Time.time) {
            if (owner == null) {
                Destroy(gameObject);
            } else {
                aliveTime = aliveTimeAfterPickup;
            }
        }
    }

    protected void BaseCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            gameObject.layer = LayerMask.NameToLayer("Items");
        }
    }

    public virtual void OnPickup(Player player) {
        playerOwner = player;
    }

    public virtual void OnDiscard(Player player) {
        playerOwner = null;
    }

    /// True if the player should throw the item, false if he should call
    /// EndCharging/Use instead.
    public abstract bool ShouldThrow();
    /// True if the attack should charge and fire when the button is released,
    /// false to fire immediately.
    public abstract bool ShouldCharge();
    /// Called when the player is done charging the item and it should fire.
    public virtual void EndCharging(float chargeTime, Direction direction, NetworkInstanceId playerNetId) {}
    /// For convenience, just calls EndCharging with chargeTime == 0f.
    public void Use(Direction direction, NetworkInstanceId playerNetId) {
        EndCharging(0f, direction, playerNetId);
    }

    public static void IgnoreCollisions(GameObject obj1, GameObject obj2, bool ignore = true) {
        var colls1 = obj1.GetComponents<Collider2D>();
        var colls2 = obj2.GetComponents<Collider2D>();
        foreach (var c1 in colls1) {
            foreach (var c2 in colls2) {
                Physics2D.IgnoreCollision(c1, c2, ignore);
            }
        }
    }

    public static void IgnoreCollisions(GameObject obj, Collider2D coll, bool ignore = true) {
        var colls = obj.GetComponents<Collider2D>();
        foreach (var c in colls) {
            Physics2D.IgnoreCollision(c, coll, ignore);
        }
    }

    /// An item with an owner will not self destruct,
    /// and it will follow the owner around the screen.
    public void SetOwner(GameObject owner) {
        this.owner = owner;
        if (owner == null) {
            destroyTime = Time.time + aliveTime;
            return;
        }
        this.ownerOffset = gameObject.transform.position - owner.transform.position;
    }

    /// When the player changes direction, the item needs
    /// to switch to the opposite side.
    [Server]
    public void UpdateOwnerOffset(float dx, float dy) {
        this.ownerOffset.x += dx;
        this.ownerOffset.y += dy;
    }

    /// An item held by a player should not be affected by physics.
    public void EnablePhysics(bool enable) {
        GetComponent<Rigidbody2D>().isKinematic = !enable;
    }
}