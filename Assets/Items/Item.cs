using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;

namespace SciFi.Items {
    public abstract class Item : NetworkBehaviour {
        public bool isBeingThrown = false;

        /// The item's owner, if any, that it will follow.
        protected GameObject ownerGo;
        protected Player owner;
        protected Vector3 ownerOffset;

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
            // If there is an owner, all copies update their position independently
            // based on the owner's position.
            if (ownerGo != null) {
                gameObject.transform.position = ownerGo.transform.position + ownerOffset;
            }

            if (!isServer) {
                return;
            }

            // An unowned item will self-destruct after a certain time.
            // An owned item whose timer expires will just reset it to a shorter
            // timer which starts after it is discarded.
            if (this.destroyTime < Time.time) {
                if (ownerGo == null) {
                    Destroy(gameObject);
                } else {
                    aliveTime = aliveTimeAfterPickup;
                }
            }
        }

        protected void BaseCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (collision.gameObject.tag == "Ground") {
                RpcSetToItemsLayer();
            }
        }

        [ClientRpc]
        void RpcSetToItemsLayer() {
            gameObject.layer = Layers.items;
        }

        // Called on all clients when the object is picked up.
        public virtual void OnPickup() {}

        // Called on all clients when the object is discarded.
        public virtual void OnDiscard() {}

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

        /// Returns true if the owner was set,
        /// false if there was already a different owner.
        [Server]
        public bool SetOwner(GameObject owner) {
            if (this.ownerGo != null && owner != null) {
                return false;
            }
            this.ownerGo = owner;
            if (owner != null) {
                this.owner = owner.GetComponent<Player>();
                EnablePhysics(false);
                RpcNotifyPickup(owner);
            } else {
                this.owner = null;
                EnablePhysics(true);
                destroyTime = Time.time + aliveTime;
                RpcNotifyDiscard();
            }
            return true;
        }

        [ClientRpc]
        void RpcNotifyPickup(GameObject newOwner) {
            this.ownerGo = newOwner;
            this.owner = newOwner.GetComponent<Player>();
            EnablePhysics(false);
            OnPickup();
        }

        [ClientRpc]
        void RpcNotifyDiscard() {
            this.ownerGo = null;
            this.owner = null;
            EnablePhysics(true);
            OnDiscard();
        }

        /// When the player changes direction, the item needs
        /// to switch to the opposite side.
        [Server]
        public void SetOwnerOffset(float x, float y) {
            this.ownerOffset.x = x;
            this.ownerOffset.y = y;
            RpcUpdateOwnerOffset(this.ownerOffset);
        }

        [ClientRpc]
        void RpcUpdateOwnerOffset(Vector3 offset) {
            this.ownerOffset = offset;
        }

        /// An item held by a player should not be affected by physics.
        void EnablePhysics(bool enable) {
            GetComponent<Rigidbody2D>().isKinematic = !enable;
        }
    }
}