using UnityEngine;
using UnityEngine.Networking;

public class Item : NetworkBehaviour {
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
        gameObject.GetComponent<ItemData>().item = this;
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