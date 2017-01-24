using UnityEngine;
using UnityEngine.Networking;
using System;

using SciFi.Environment;
using SciFi.Players.Attacks;
using SciFi.Items;
using SciFi.UI;

namespace SciFi.Players {
    public enum Direction {
        Left,
        Right,
        Up,
        Down,
        Invalid,
    }

    public enum PlayerFeature {
        Movement,
        Attack,
        Damage,
        Knockback,
        Jump,
    }

    public enum PlayerAttack {
        Attack1,
        Attack2,
        SpecialAttack,
        Item,
    }

    public abstract class Player : NetworkBehaviour {
        public GameObject shieldPrefab;

        // Gameplay data
        [SyncVar, HideInInspector]
        public int eId;
        [SyncVar, HideInInspector]
        public string eDisplayName;
        [SyncVar, HideInInspector]
        public int eLives;
        [SyncVar, HideInInspector]
        public int eDamage;
        [SyncVar, HideInInspector]
        public Direction eDirection;
        [SyncVar, HideInInspector]
        public bool eShouldFallThroughOneWayPlatform;

        protected Rigidbody2D lRb;
        protected InputManager pInputManager;
        private int pGroundCollisions;
        protected bool pCanJump;
        protected bool pCanDoubleJump;
        [SyncVar]
        protected GameObject eItem;
        private OneWayPlatform pCurrentOneWayPlatform;
        private int[] featureLockout;

        // Unity editor parameters
        public Direction defaultDirection;
        public float maxSpeed;
        public float walkForce;
        public float jumpForce;
        public float minDoubleJumpVelocity;
        /// Max time that a direction can be held before pressing
        /// item to throw it, so you can use an item while moving.
        const float throwItemHoldTime = .15f;

        MultiPressControl pLeftControl;
        MultiPressControl pRightControl;
        TouchButtons pTouchButtons;

        // Parameters for child classes to change behavior
        protected Attack eAttack1;
        protected Attack eAttack2;
        protected Attack eSpecialAttack;
        //protected Attack eSuperAttack;
        protected Shield eShield;

        void Awake() {
            featureLockout = new int[Enum.GetNames(typeof(PlayerFeature)).Length];
        }

        protected void BaseStart() {
            lRb = GetComponent<Rigidbody2D>();
            eDirection = Direction.Right;
            var gameControllerGo = GameObject.Find("GameController");
            pInputManager = gameControllerGo.GetComponent<InputManager>();
            pLeftControl = new MultiPressControl(pInputManager, Control.Left, .4f);
            pRightControl = new MultiPressControl(pInputManager, Control.Right, .4f);

            if (isLocalPlayer) {
                pInputManager.ObjectSelected += ObjectSelected;
                pInputManager.ControlCanceled += ControlCanceled;
                var leftButton = GameObject.Find("LeftButton");
                if (leftButton != null) {
                    pTouchButtons = leftButton.GetComponent<TouchButtons>();
                }
            }

            var shieldObj = Instantiate(shieldPrefab, transform.position + new Vector3(.6f, 0f), Quaternion.identity, transform);
            eShield = shieldObj.GetComponent<Shield>();
        }

        protected void BaseCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Ground") {
                ++pGroundCollisions;
                pCanJump = true;
                pCanDoubleJump = false;

                var oneWay = collision.gameObject.GetComponent<OneWayPlatform>();
                if (oneWay != null) {
                    pCurrentOneWayPlatform = oneWay;
                }
            }
        }

        protected void BaseCollisionExit2D(Collision2D collision) {
            if (collision.gameObject.tag == "Ground") {
                if (--pGroundCollisions == 0) {
                    pCanJump = false;
                    pCanDoubleJump = true;
                }

                var oneWay = collision.gameObject.GetComponent<OneWayPlatform>();
                if (oneWay != null) {
                    pCurrentOneWayPlatform = null;
                }
            }
        }

        public void SuspendFeature(PlayerFeature feature) {
            ++featureLockout[(int)feature];
        }

        public void ResumeFeature(PlayerFeature feature, bool force = false) {
            if (force) {
                featureLockout[(int)feature] = 0;
            } else if (featureLockout[(int)feature] > 0) {
                --featureLockout[(int)feature];
            }
        }

        public void SuspendAllFeatures() {
            SuspendFeature(PlayerFeature.Attack);
            SuspendFeature(PlayerFeature.Damage);
            SuspendFeature(PlayerFeature.Knockback);
            SuspendFeature(PlayerFeature.Movement);
        }

        public void ResumeAllFeatures(bool force = false) {
            ResumeFeature(PlayerFeature.Attack, force);
            ResumeFeature(PlayerFeature.Damage, force);
            ResumeFeature(PlayerFeature.Knockback, force);
            ResumeFeature(PlayerFeature.Movement, force);
        }

        public bool FeatureEnabled(PlayerFeature feature) {
            return featureLockout[(int)feature] == 0;
        }

        void HandleLeftRightInput(MultiPressControl control, Direction direction, bool backwards) {
            bool canSpeedUp;
            Vector3 force;
            bool halfForce = control.GetPresses() == 1;
            float localMaxSpeed = maxSpeed;
            float localWalkForce = walkForce;
            if (halfForce) {
                localMaxSpeed /= 2f;
                localWalkForce /= 2f;
            }

            if (backwards) {
                canSpeedUp = lRb.velocity.x > -localMaxSpeed;
                force = transform.right * -localWalkForce;
            } else {
                canSpeedUp = lRb.velocity.x < localMaxSpeed;
                force = transform.right * localWalkForce;
            }

            if (control.IsActive() && FeatureEnabled(PlayerFeature.Movement)) {
                if (canSpeedUp) {
                    lRb.AddForce(force);
                }
                // Without the cached parameter, this will get triggered
                // multiple times until the direction has had a chance to sync.
                if (this.eDirection != direction) {
                    this.eDirection = direction;
                    CmdChangeDirection(direction);
                }
            }
        }

        void AddDampingForce() {
            if (lRb.velocity.x < .1f && lRb.velocity.x > -.1f) {
                lRb.velocity = new Vector2(0f, lRb.velocity.y);
                return;
            }

            float force;
            if (lRb.velocity.x < 0) {
                force = walkForce / 2;
            } else {
                force = -walkForce / 2;
            }
            lRb.AddForce(new Vector3(force, 0f));
        }

        protected void BaseInput() {
            pLeftControl.Update();
            pRightControl.Update();

            HandleLeftRightInput(pLeftControl, Direction.Left, true);
            HandleLeftRightInput(pRightControl, Direction.Right, false);
            if (!pLeftControl.IsActive() && !pRightControl.IsActive()) {
                AddDampingForce();
            }

            if (pInputManager.IsControlActive(Control.Up) && FeatureEnabled(PlayerFeature.Movement)) {
                pInputManager.InvalidateControl(Control.Up);
                if (pCanJump) {
                    pCanJump = false;
                    pCanDoubleJump = true;
                    lRb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
                } else if (pCanDoubleJump) {
                    pCanDoubleJump = false;
                    if (lRb.velocity.y < minDoubleJumpVelocity) {
                        lRb.velocity = new Vector2(lRb.velocity.x, minDoubleJumpVelocity);
                    }
                    lRb.AddForce(transform.up * jumpForce / 2, ForceMode2D.Impulse);
                }
            }

            if (pInputManager.IsControlActive(Control.Down) && FeatureEnabled(PlayerFeature.Movement)) {
                eShouldFallThroughOneWayPlatform = true;
                if (pCurrentOneWayPlatform != null) {
                    pCurrentOneWayPlatform.CmdFallThrough(gameObject);
                    // Forget the platform so we don't keep sending messages.
                    pCurrentOneWayPlatform = null;
                } else {
                    if (!eShield.IsActive()) {
                        eShield.Activate();
                    }
                }
            } else {
                eShouldFallThroughOneWayPlatform = false;
                if (eShield.IsActive()) {
                    eShield.Deactivate();
                }
            }

            UpdateItemControl(pInputManager.IsControlActive(Control.Item));

            eAttack1.UpdateState(pInputManager, Control.Attack1);
            eAttack2.UpdateState(pInputManager, Control.Attack2);
            eSpecialAttack.UpdateState(pInputManager, Control.SpecialAttack);
        }

        /// Spawns a projectile, ignoring collisions
        /// with the player and his/her item.
        [Command]
        public void CmdSpawnProjectile(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 force,
            float torque)
        {
            var obj = Instantiate(GameController.IndexToPrefab(prefabIndex), position, rotation);
            var projectile = obj.GetComponent<Projectile>();
            projectile.spawnedBy = netId;
            projectile.spawnedByExtra = GetItemNetId();
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.AddForce(force);
                rb.AddTorque(torque);
            }
            NetworkServer.Spawn(obj);
        }

        /// Spawns a projectile for a pre-instantiated object
        /// so it can be customized beforehand.
        [Command]
        public void CmdSpawnCustomProjectile(
            GameObject projectile,
            Vector2 force,
            float torque)
        {
            var projectileComponent = projectile.GetComponent<Projectile>();
            projectileComponent.spawnedBy = netId;
            projectileComponent.spawnedByExtra = GetItemNetId();
            var rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.AddForce(force);
                rb.AddTorque(torque);
            }
            NetworkServer.Spawn(projectile);
        }

        public NetworkInstanceId GetItemNetId() {
            return eItem == null ? NetworkInstanceId.Invalid : eItem.GetComponent<Item>().netId;
        }

        Direction GetControlDirection() {
            if (pInputManager.IsControlActive(Control.Down)) {
                return Direction.Down;
            }
            if (pInputManager.IsControlActive(Control.Up)) {
                return Direction.Up;
            }
            if (pInputManager.IsControlActive(Control.Left)) {
                return Direction.Left;
            }
            if (pInputManager.IsControlActive(Control.Right)) {
                return Direction.Right;
            }
            return Direction.Invalid;
        }

        float GetDirectionHoldTime(Direction direction) {
            var control = GetDirectionControl(direction);
            if (control == -1) {
                return 0f;
            }

            return pInputManager.GetControlHoldTime(control);
        }

        int GetDirectionControl(Direction direction) {
            switch (direction) {
            case Direction.Left:
                return Control.Left;
            case Direction.Right:
                return Control.Right;
            case Direction.Up:
                return Control.Up;
            case Direction.Down:
                return Control.Down;
            default:
                return -1;
            }
        }

        void UpdateItemControl(bool active) {
            if (eItem == null) {
                if (active) {
                    pInputManager.InvalidateControl(Control.Item);
                    PickUpItem();
                }
                return;
            }

            var i = eItem.GetComponent<Item>();
            if (i.IsCharging()) {
                if (active) {
                    if (i.ShouldCancel()) {
                        pInputManager.InvalidateControl(Control.Item);
                        i.Cancel();
                        UpdateItemControlGraphic();
                        ResumeFeature(PlayerFeature.Attack);
                        ResumeFeature(PlayerFeature.Movement);
                    } else {
                        i.KeepCharging(pInputManager.GetControlHoldTime(Control.Item));
                    }
                } else {
                    i.EndCharging(pInputManager.GetControlHoldTime(Control.Item));
                    UpdateItemControlGraphic();
                    ResumeFeature(PlayerFeature.Attack);
                    ResumeFeature(PlayerFeature.Movement);
                }
            } else if (active) {
                var direction = GetControlDirection();
                var control = GetDirectionControl(direction);
                var holdTime = GetDirectionHoldTime(direction);
                if (direction != Direction.Invalid && holdTime < throwItemHoldTime && FeatureEnabled(PlayerFeature.Attack)) {
                    pInputManager.InvalidateControl(Control.Item);
                    pInputManager.InvalidateControl(control);
                    CmdLoseOwnershipOfItem(direction);
                    eItem = null;
                } else if (i.ShouldCharge() && FeatureEnabled(PlayerFeature.Attack)) {
                    SuspendFeature(PlayerFeature.Attack);
                    SuspendFeature(PlayerFeature.Movement);
                    i.BeginCharging();
                } else if (i.ShouldThrow() && FeatureEnabled(PlayerFeature.Attack)) {
                    pInputManager.InvalidateControl(Control.Item);
                    CmdLoseOwnershipOfItem(eDirection);
                    eItem = null;
                } else if (!i.CanCharge() && FeatureEnabled(PlayerFeature.Attack)) {
                    pInputManager.InvalidateControl(Control.Item);
                    i.Use();
                    UpdateItemControlGraphic();
                }
                // If none of the above conditions were true, the item
                // is chargeable, but it can't charge right now (in cooldown)
                // so we just do nothing.
            }
        }

        void UseItem() {
            var i = eItem.GetComponent<Item>();
            if (i.ShouldThrow()) {
                CmdLoseOwnershipOfItem(eDirection);
                eItem = null;
            } else if (i.ShouldCharge()) {
                // TODO: begin charging item
                // TODO: run this on server
                i.BeginCharging();
                i.Use();
            } else {
                i.Use();
            }
        }

        void PickUpItem(GameObject item = null) {
            item = CircleCastForItem(item);
            if (item != null) {
                CmdTakeOwnershipOfItem(item);
            }
        }

        [ClientRpc]
        void RpcUpdateItemControlGraphic() {
            UpdateItemControlGraphic();
        }

        void UpdateItemControlGraphic() {
            if (!hasAuthority) {
                return;
            }

            if (pTouchButtons == null) {
                return;
            }

            if (eItem == null) {
                pTouchButtons.SetItemButtonToItemGraphic();
            } else if (eItem.GetComponent<Item>().ShouldThrow()) {
                pTouchButtons.SetItemButtonToDiscardGraphic();
            } else {
                pTouchButtons.SetItemButtonGraphic(eItem.GetComponent<SpriteRenderer>().sprite);
            }
        }

        [Command]
        void CmdTakeOwnershipOfItem(GameObject item) {
            var itemComponent = item.GetComponent<Item>();
            if (!itemComponent.SetOwner(gameObject)) {
                return;
            }
            this.eItem = item;
            RpcUpdateItemControlGraphic();

            itemComponent.ChangeDirection(eDirection);
            item.GetComponent<NetworkTransform>().enabled = false;
        }

        [Command]
        void CmdLoseOwnershipOfItem(Direction direction) {
            var item = this.eItem;
            this.eItem = null;

            var itemComponent = item.GetComponent<Item>();
            itemComponent.SetOwner(null);
            itemComponent.Throw(direction);

            RpcUpdateItemControlGraphic();

            var networkTransform = item.GetComponent<NetworkTransform>();
            networkTransform.enabled = true;
        }

        [Server]
        void MoveItemForChangeDirection(Direction direction) {
            var item = eItem.GetComponent<Item>();
            item.ChangeDirection(direction);
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
                1 << Layers.items | 1 << Layers.noncollidingItems);
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
            if (args.gameObject == this.eItem) {
                UseItem();
                return;
            }

            // We can only hold one item at a time.
            if (eItem != null) {
                return;
            }
            if (args.gameObject.layer == Layers.items || args.gameObject.layer == Layers.noncollidingItems) {
                if (CircleCastForItem(args.gameObject) == args.gameObject) {
                    PickUpItem(args.gameObject);
                }
            }
        }

        void ControlCanceled(ControlCanceledEventArgs args) {
            //
        }

        [ClientRpc]
        public void RpcRespawn(Vector3 position) {
            if (!hasAuthority) {
                return;
            }
            if (eItem != null) {
                Destroy(eItem);
                eItem = null;
            }
            transform.position = position;
            lRb.velocity = new Vector2(0f, 0f);
        }

        [Command]
        void CmdChangeDirection(Direction direction) {
            this.eDirection = direction;
            if (eItem != null) {
                MoveItemForChangeDirection(direction);
                RpcChangeItemDirection(direction);
            }
            RpcChangeDirection(direction);
        }

        [ClientRpc]
        protected virtual void RpcChangeDirection(Direction direction) {}

        [ClientRpc]
        void RpcChangeItemDirection(Direction direction) {
            if (eItem != null) {
                eItem.GetComponent<Item>().ChangeDirection(direction);
            }
        }

        [ClientRpc]
        public void RpcKnockback(Vector2 force) {
            if (!hasAuthority) {
                return;
            }
            if (!FeatureEnabled(PlayerFeature.Knockback)) {
                return;
            }
            lRb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}