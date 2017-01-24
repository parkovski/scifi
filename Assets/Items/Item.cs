using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;

namespace SciFi.Items {
    public abstract class Item : NetworkBehaviour {
        bool pIsCharging = false;
        bool eCanCharge;
        protected Direction eDirection = Direction.Right;
        int eInitialLayer;
        bool pShouldCancel = false;

        /// The item's owner, if any, that it will follow.
        protected GameObject eOwnerGo;
        protected Player eOwner;
        [SyncVar]
        protected Vector3 eOwnerOffset;

        private float sAliveTime;
        private float sDestroyTime;
        /// Items won't destroy when they are owned, but
        /// if they are discarded, they will only stick around
        /// for this much time if their original lifetime has expired already.
        const float aliveTimeAfterPickup = 5f;

        const float blinkTime = 3f;
        float firstBlinkTime = 0f;
        protected SpriteRenderer spriteRenderer;

        protected void BaseStart(bool canCharge, float aliveTime = 15f) {
            this.sAliveTime = aliveTime;
            this.sDestroyTime = Time.time + aliveTime;
            this.eCanCharge = canCharge;
            this.eInitialLayer = gameObject.layer;
            this.spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected void BaseUpdate() {
            // If there is an owner, all copies update their position independently
            // based on the owner's position.
            if (eOwnerGo != null) {
                gameObject.transform.position = eOwnerGo.transform.position + eOwnerOffset;
            }

            if (!isServer) {
                return;
            }

            // An unowned item will self-destruct after a certain time.
            // An owned item whose timer expires will just reset it to a shorter
            // timer which starts after it is discarded.
            if (this.sDestroyTime < Time.time) {
                if (eOwnerGo == null) {
                    Destroy(gameObject);
                } else {
                    sAliveTime = aliveTimeAfterPickup;
                }
            } else if (this.sDestroyTime < Time.time + blinkTime && eOwnerGo == null) {
                if (firstBlinkTime == 0f) {
                    firstBlinkTime = Time.time;
                }
                Blink();
            }
        }

        void Blink() {
            var alpha = .5f + Mathf.Abs(Mathf.Cos((Time.time - firstBlinkTime) * 6 * Mathf.PI / 3)) / 2;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            OnBlink(alpha);
        }

        void RestoreAlpha() {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
            OnBlink(1f);
        }

        /// This can be used to apply the alpha to a child sprite
        /// when the item is blinking, indicating it is about to
        /// be destroyed.
        protected virtual void OnBlink(float alpha) {}

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
            gameObject.layer = eInitialLayer;
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
        public bool CanCharge() {
            return eCanCharge;
        }
        /// Only valid on the client w/ local authority over the owner.
        [Client]
        public bool IsCharging() {
            return pIsCharging;
        }
        public void BeginCharging() {
            pIsCharging = true;
            pShouldCancel = false;
            OnBeginCharging();
        }
        public void KeepCharging(float chargeTime) {
            OnKeepCharging(chargeTime);
        }
        public void EndCharging(float chargeTime) {
            pIsCharging = false;
            OnEndCharging(chargeTime);
        }
        /// This should only be called when control feature flags
        /// are being reset, otherwise call RequestCancel.
        public void Cancel() {
            pShouldCancel = false;
            OnCancel();
            pIsCharging = false;
        }

        public void RequestCancel() {
            pShouldCancel = true;
        }

        public bool ShouldCancel() {
            return pShouldCancel;
        }

        [Client]
        protected virtual void OnBeginCharging() {}
        [Client]
        protected virtual void OnKeepCharging(float chargeTime) {}
        /// Called on the client when the player is done charging the item and it should fire.
        [Client]
        protected virtual void OnEndCharging(float chargeTime) {}
        /// For convenience, just calls EndCharging with chargeTime == 0f.
        protected virtual void OnCancel() {}
        [Client]
        public void Use() {
            OnEndCharging(0f);
        }

        public virtual void TakeDamage(int amount) {}

        [Server]
        protected virtual void OnChangeDirection(Direction direction) { }

        [Server]
        public void ChangeDirection(Direction direction) {
            eDirection = direction;
            eOwnerOffset = GetOwnerOffset(direction);
            OnChangeDirection(direction);
        }

        [Server]
        public void Throw(Direction direction) {
            Vector2 force;
            switch (direction) {
            case Direction.Up:
                force = new Vector2(0f, 300f);
                break;
            case Direction.Down:
                force = new Vector2(0f, -300f);
                break;
            case Direction.Left:
                force = new Vector2(-300f, 100f);
                break;
            case Direction.Right:
                force = new Vector2(300f, 100f);
                break;
            default:
                return;
            }
            GetComponent<Rigidbody2D>().AddForce(force);
            gameObject.layer = Layers.projectiles;
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
            if (this.eOwnerGo != null && owner != null) {
                return false;
            }
            this.eOwnerGo = owner;
            if (owner != null) {
                this.eOwner = owner.GetComponent<Player>();
                EnablePhysics(false);
                RpcNotifyPickup(owner);
            } else {
                this.eOwner = null;
                EnablePhysics(true);
                sDestroyTime = Time.time + sAliveTime;
                RpcNotifyDiscard();
            }
            return true;
        }

        [ClientRpc]
        void RpcNotifyPickup(GameObject newOwner) {
            this.eOwnerGo = newOwner;
            this.eOwner = newOwner.GetComponent<Player>();
            EnablePhysics(false);
            RestoreAlpha();
            OnPickup();
        }

        [ClientRpc]
        void RpcNotifyDiscard() {
            this.eOwnerGo = null;
            this.eOwner = null;
            EnablePhysics(true);
            OnDiscard();
        }

        protected virtual Vector3 GetOwnerOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-1, 0);
            } else {
                return new Vector3(1, 0);
            }
        }

        /// An item held by a player should not be affected by physics.
        void EnablePhysics(bool enable) {
            GetComponent<Rigidbody2D>().isKinematic = !enable;
        }
    }
}