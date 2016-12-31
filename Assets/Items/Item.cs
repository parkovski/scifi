using UnityEngine;
using UnityEngine.Networking;

public abstract class Item : NetworkBehaviour {
    public bool isBeingThrown = false;

    /// The item's owner, if any, that it will follow.
    private GameObject owner;
    private Vector3 ownerOffset;

    private float aliveTime;
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

    protected abstract bool ShouldThrow();
    protected abstract bool ShouldCharge();
    protected virtual void EndCharging(float chargeTime, Direction direction) {}

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

    public void SetOwner(GameObject owner) {
        this.owner = owner;
        if (owner == null) {
            destroyTime = Time.time + aliveTime;
            return;
        }
        this.ownerOffset = gameObject.transform.position - owner.transform.position;
    }

    [Server]
    public void UpdateOwnerOffset(float dx, float dy) {
        this.ownerOffset.x += dx;
        this.ownerOffset.y += dy;
    }

    public void EnablePhysics(bool enable) {
        GetComponent<Rigidbody2D>().isKinematic = !enable;
    }
}