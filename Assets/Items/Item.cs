using UnityEngine;
using UnityEngine.Networking;

public class Item : NetworkBehaviour {
    public bool isBeingThrown = false;

    /// The item's owner, if any, that it will follow.
    private GameObject owner;
    private Vector3 ownerOffset;

    protected void BaseStart() {
        gameObject.GetComponent<ItemData>().item = this;
    }

    protected void BaseUpdate() {
        if (owner != null) {
            gameObject.transform.position = owner.transform.position + ownerOffset;
        }
    }

    public void SetOwner(GameObject owner) {
        this.owner = owner;
        if (owner == null) {
            return;
        }
        this.ownerOffset = gameObject.transform.position - owner.transform.position;
    }

    public void EnablePhysics(bool enable) {
        GetComponent<Rigidbody2D>().isKinematic = !enable;
    }
}