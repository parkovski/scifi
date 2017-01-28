using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Players;

namespace SciFi.Items {
    /// An item that spawns randomly and can be picked up and used by the player.
    public abstract class Item : NetworkBehaviour {
        bool pIsCharging = false;
        bool eCanCharge;
        protected Direction eDirection = Direction.Right;
        /// The layer the item should be on when it is not acting as a projectile.
        int eInitialLayer;
        /// Records whether a cancellation was requested.
        bool pShouldCancel = false;

        /// The item's owner, if any, that it will follow.
        protected GameObject eOwnerGo;
        /// The item's owner - null if the item is not owned.
        protected Player eOwner;
        [SyncVar]
        /// The offset from the owner that the item will follow,
        /// if the owner is set.
        protected Vector3 eOwnerOffset;

        /// How long the item will stay active on the screen.
        private float sAliveTime;
        /// The time that the item will be destroyed.
        private float sDestroyTime;
        /// Items won't destroy when they are owned, but
        /// if they are discarded, they will only stick around
        /// for this much time if their original lifetime has expired already.
        const float aliveTimeAfterPickup = 5f;

        /// How long before <see cref="sDestroyTime" /> the item will
        /// start blinking, indicating it is about to be destroyed.
        const float blinkTime = 3f;
        /// The time the item started blinking.
        float firstBlinkTime = 0f;
        protected SpriteRenderer spriteRenderer;

        /// A set of objects that the item has hit to make sure
        /// the item only hits once.
        private HashSet<GameObject> hitObjects;

        /// Initializes common item state.
        protected void BaseStart(bool canCharge, float aliveTime = 15f) {
            this.sAliveTime = aliveTime;
            this.sDestroyTime = Time.time + aliveTime;
            this.eCanCharge = canCharge;
            this.eInitialLayer = gameObject.layer;
            this.spriteRenderer = GetComponent<SpriteRenderer>();
            this.hitObjects = new HashSet<GameObject>();
        }

        /// Handles common item behaviour, including following the owner,
        /// destroying after a certain time, and blinking when the destroy
        /// time is close.
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

        /// Make the item flash between opaque and semi-transparent when it is about
        /// to be destroyed.
        void Blink() {
            var alpha = .5f + Mathf.Abs(Mathf.Cos((Time.time - firstBlinkTime) * 6 * Mathf.PI / 3)) / 2;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            OnBlink(alpha);
        }

        /// When the player grabs a blinking item, it should be made fully opaque.
        void RestoreAlpha() {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
            OnBlink(1f);
        }

        /// This can be used to apply the alpha to a child sprite
        /// when the item is blinking, indicating it is about to
        /// be destroyed.
        protected virtual void OnBlink(float alpha) {}

        /// Corrects the item's layer when it hits the ground if it was
        /// acting as a projectile.
        protected void BaseCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (collision.gameObject.tag == "Ground") {
                gameObject.layer = eInitialLayer;
            }
        }

        // Called on all clients when the object is picked up.
        protected virtual void OnPickup() {}

        // Called on all clients when the object is discarded.
        protected virtual void OnDiscard() {}

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
        /// Begins charging the attack. Calls <see cref="OnBeginCharging" />.
        public void BeginCharging() {
            pIsCharging = true;
            pShouldCancel = false;
            OnBeginCharging();
        }
        /// Continues charging the attack on each frame. Calls <see cref="OnKeepCharging" />.
        public void KeepCharging(float chargeTime) {
            OnKeepCharging(chargeTime);
        }
        /// Ends charging the attack. Calls <see cref="OnEndCharging" />.
        public void EndCharging(float chargeTime) {
            pIsCharging = false;
            OnEndCharging(chargeTime);
        }
        /// This should only be called when control feature flags
        /// are being reset, otherwise call <see cref="RequestCancel" />.
        public void Cancel() {
            pShouldCancel = false;
            OnCancel();
            pIsCharging = false;
        }

        /// Requests a cancellation. Since attacks and movement are
        /// suspended while the item is charging, this should be used
        /// in most cases so that the player features will be restored.
        public void RequestCancel() {
            pShouldCancel = true;
        }

        /// Returns true if a cancellation was requested.
        public bool ShouldCancel() {
            return pShouldCancel;
        }

        /// Called when the item button is first pressed if
        /// <see cref="ShouldCharge" /> returned true.
        [Client]
        protected virtual void OnBeginCharging() {}
        /// Called for each frame that the item button is held
        /// if <see cref="ShouldCharge" /> returned true.
        [Client]
        protected virtual void OnKeepCharging(float chargeTime) {}
        /// Called on the client when the player is done charging the item and it should fire.
        /// If the item does not charge, this is called when the item button is first pressed.
        [Client]
        protected virtual void OnEndCharging(float chargeTime) {}
        /// Called when the attack was canceled, either because the attack was hit
        /// by one with higher precedence or the command was canceled.
        protected virtual void OnCancel() {}
        /// For convenience, just calls EndCharging with chargeTime == 0f.
        [Client]
        public void Use() {
            OnEndCharging(0f);
        }

        /// Called when this item is attacked.
        public virtual void TakeDamage(int amount) {}

        /// Handles specific direction change behaviour, for example
        /// flipping and translating a child object.
        [Server]
        protected virtual void OnChangeDirection(Direction direction) { }

        /// Moves and flips an item to be on the appropriate side
        /// of the player when they change direction.
        [Server]
        public void ChangeDirection(Direction direction) {
            eDirection = direction;
            eOwnerOffset = GetOwnerOffset(direction);
            OnChangeDirection(direction);
        }

        /// Adds a force to this item and sets it to the projectiles layer.
        /// <param name="direction">The direction to throw - up, down, left or right.</param>
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

        /// Remember an object hit to avoid hitting it twice
        protected void LogHit(GameObject obj) {
            hitObjects.Add(obj);
        }

        /// Check if a hit was recorded with <see cref="LogHit" />
        protected bool DidHit(GameObject obj) {
            return hitObjects.Contains(obj);
        }

        /// Forget all objects saved with <see cref="LogHit" />
        protected void ClearHits() {
            hitObjects.Clear();
        }

        /// Ignore collisions from all colliders on <c>obj1</c> and <c>obj2</c>.
        /// <param name="ignore">If true, ignore collisions. If false, detect collisions.</param>
        public static void IgnoreCollisions(GameObject obj1, GameObject obj2, bool ignore = true) {
            var colls1 = obj1.GetComponents<Collider2D>();
            var colls2 = obj2.GetComponents<Collider2D>();
            foreach (var c1 in colls1) {
                foreach (var c2 in colls2) {
                    Physics2D.IgnoreCollision(c1, c2, ignore);
                }
            }
        }

        /// Ignore collisions from <c>coll</c> and all colliders on <c>obj</c>.
        /// <param name="ignore">If true, ignore collisions. If false, detect collisions.</param>
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
                gameObject.layer = Layers.items;
                RpcNotifyPickup(owner);
            } else {
                this.eOwner = null;
                sDestroyTime = Time.time + sAliveTime;
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                RpcNotifyDiscard();
            }
            return true;
        }

        [ClientRpc]
        void RpcNotifyPickup(GameObject newOwner) {
            this.eOwnerGo = newOwner;
            this.eOwner = newOwner.GetComponent<Player>();
            RestoreAlpha();
            OnPickup();
        }

        [ClientRpc]
        void RpcNotifyDiscard() {
            this.eOwnerGo = null;
            this.eOwner = null;
            OnDiscard();
        }

        /// Returns the offset relative to the owner that this item
        /// should appear when picked up.
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