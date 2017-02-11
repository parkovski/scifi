using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.Collections.Generic;

using SciFi.Environment;
using SciFi.Players.Attacks;
using SciFi.Players.Modifiers;
using SciFi.Items;
using SciFi.UI;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.Players {
    public enum Direction {
        Left,
        Right,
        Up,
        Down,
        Invalid,
    }

    public abstract class Player : NetworkBehaviour, IInteractable {
        public static readonly Color blueTeamColor = new Color(0, 0, 0.235f, 1f);
        public static readonly Color redTeamColor = new Color(0.235f, 0, 0, 1f);
        public static readonly Color greenTeamColor = new Color(0, 0.235f, 0, 1f);
        public static readonly Color yellowTeamColor = new Color(0.235f, 0.235f, 0, 1f);

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
        [SyncVar(hook = "ChangeDirection"), HideInInspector]
        public Direction eDirection;
        [SyncVar, HideInInspector]
        public bool eShouldFallThroughOneWayPlatform;

        private bool lInitialized = false;
        protected Rigidbody2D lRb;
        protected InputManager pInputManager;
        private int pGroundCollisions;
        protected bool pCanJump;
        protected bool pCanDoubleJump;
        protected GameObject eItemGo;
        protected Item eItem;
        private OneWayPlatform pCurrentOneWayPlatform;
        private List<uint> sModifiers;
        [SyncVar]
        private uint eModifierState;
        private int pModifiersDebugField;
        private uint pOldModifierState;
        private List<NetworkAttack> lNetworkAttacks;
        private float sKnockbackLockoutEndTime = float.PositiveInfinity;

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

        public delegate void AttackHitHandler(AttackType type, AttackProperty properties);
        /// Emitted on the server when an attack hits.
        public event AttackHitHandler sAttackHit;

        public override void OnStartServer() {
            sModifiers = new List<uint>();
            Modifier.Initialize(sModifiers);
        }

        void Start() {
            lRb = GetComponent<Rigidbody2D>();
            eDirection = Direction.Right;

            var shieldObj = Instantiate(shieldPrefab, transform.position + new Vector3(.6f, .1f), Quaternion.identity, transform);
            eShield = shieldObj.GetComponent<Shield>();

            lNetworkAttacks = new List<NetworkAttack>();

            pModifiersDebugField = DebugPrinter.Instance.NewField();

            if (pInputManager != null) {
                OnInitialize();
                lInitialized = true;
            }
        }

        protected abstract void OnInitialize();

        public void GameControllerReady(GameController gameController) {
            // This is called twice on a host - once when the client game
            // starts and once when the server game starts.
            if (pInputManager != null) {
                return;
            }

            pInputManager = gameController.GetComponent<InputManager>();
            if (isLocalPlayer) {
                pLeftControl = new MultiPressControl(pInputManager, Control.Left, .4f);
                pRightControl = new MultiPressControl(pInputManager, Control.Right, .4f);

                pInputManager.ObjectSelected += ObjectSelected;
                pInputManager.ControlCanceled += ControlCanceled;
                var leftButton = GameObject.Find("LeftButton");
                if (leftButton != null) {
                    pTouchButtons = leftButton.GetComponent<TouchButtons>();
                }
            }

            if (lRb != null) {
                OnInitialize();
                lInitialized = true;
            }
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

        /// Must be called on an authoritative copy
        [Server]
        public void AddModifier(Modifier modifier) {
            modifier.Add(sModifiers, ref eModifierState);
        }

        /// Must be called on an authoritative copy
        [Server]
        public void RemoveModifier(Modifier modifier) {
            modifier.Remove(sModifiers, ref eModifierState);
        }

        public bool IsModifierEnabled(Modifier modifier) {
            return modifier.IsEnabled(eModifierState);
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
            Modifier.Slow.Modify(eModifierState, ref localMaxSpeed, ref localWalkForce);
            Modifier.Fast.Modify(eModifierState, ref localMaxSpeed, ref localWalkForce);

            if (backwards) {
                canSpeedUp = lRb.velocity.x > -localMaxSpeed;
                force = new Vector2(-localWalkForce, 0f);
            } else {
                canSpeedUp = lRb.velocity.x < localMaxSpeed;
                force = new Vector2(localWalkForce, 0f);
            }

            if (control.IsActive()) {
                if (canSpeedUp) {
                    Modifier.CantMove.TryMove(eModifierState, lRb, force);
                }
                // Without the cached parameter, this will get triggered
                // multiple times until the direction has had a chance to sync.
                if (this.eDirection != direction && !Modifier.CantMove.IsEnabled(eModifierState)) {
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
            if (pInputManager == null) {
                return;
            }
            if (!hasAuthority) {
                eAttack1.UpdateStateNonAuthoritative();
                eAttack2.UpdateStateNonAuthoritative();
                eSpecialAttack.UpdateStateNonAuthoritative();
                return;
            }

            pLeftControl.Update();
            pRightControl.Update();

            HandleLeftRightInput(pLeftControl, Direction.Left, true);
            HandleLeftRightInput(pRightControl, Direction.Right, false);
            if (!pLeftControl.IsActive() && !pRightControl.IsActive()) {
                // TODO: Only do this when no knockback is active.
                //AddDampingForce();
            }

            if (pInputManager.IsControlActive(Control.Up) && !Modifier.CantMove.IsEnabled(eModifierState)) {
                pInputManager.InvalidateControl(Control.Up);
                if (pCanJump) {
                    pCanJump = false;
                    pCanDoubleJump = true;
                    lRb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                } else if (pCanDoubleJump) {
                    pCanDoubleJump = false;
                    if (lRb.velocity.y < minDoubleJumpVelocity) {
                        lRb.velocity = new Vector2(lRb.velocity.x, minDoubleJumpVelocity);
                    }
                    lRb.AddForce(new Vector2(0f, jumpForce / 2), ForceMode2D.Impulse);
                }
            }

            if (pInputManager.IsControlActive(Control.Down) && !Modifier.CantMove.IsEnabled(eModifierState)) {
                eShouldFallThroughOneWayPlatform = true;
                if (pCurrentOneWayPlatform != null) {
                    CmdFallThroughOneWayPlatform();
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

#if UNITY_EDITOR
            if (eModifierState != pOldModifierState) {
                pOldModifierState = eModifierState;
                DebugPrinter.Instance.SetField(pModifiersDebugField, Modifier.GetDebugString(eModifierState));
            }
#endif
        }

        [Command]
        void CmdFallThroughOneWayPlatform() {
            if (pCurrentOneWayPlatform != null) {
                pCurrentOneWayPlatform.FallThrough(gameObject);
            }
        }

        protected void Update() {
            if (!isServer) {
                return;
            }

            if (Modifier.InKnockback.IsEnabled(eModifierState) && Time.time > sKnockbackLockoutEndTime) {
                RemoveModifier(Modifier.InKnockback);
                RemoveModifier(Modifier.CantAttack);
                RemoveModifier(Modifier.CantMove);
                sKnockbackLockoutEndTime = float.PositiveInfinity;
            }
        }

        /// Spawns a projectile, ignoring collisions
        /// with the player and his/her item.
        [Command]
        public void CmdSpawnProjectile(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 force,
            float torque
        ) {
            SpawnProjectile(
                GameController.IndexToPrefab(prefabIndex),
                position,
                rotation,
                force,
                torque,
                false
            );
        }

        [Command]
        public void CmdSpawnProjectileFlipped(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 force,
            float torque,
            bool flipX
        ) {
            SpawnProjectile(
                GameController.IndexToPrefab(prefabIndex),
                position,
                rotation,
                force,
                torque,
                flipX
            );
        }

        [Server]
        void SpawnProjectile(
            GameObject prefab,
            Vector2 position,
            Quaternion rotation,
            Vector2 force,
            float torque,
            bool flipX
        ) {
            var obj = Instantiate(prefab, position, rotation);
            var projectile = obj.GetComponent<Projectile>();
            projectile.spawnedBy = netId;
            projectile.spawnedByExtra = GetItemNetId();
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null) {
                projectile.AddInitialForce(force);
                rb.AddTorque(torque);
            }
            if (flipX) {
                projectile.GetComponent<SpriteRenderer>().flipX = true;
            }
            NetworkServer.Spawn(obj);
        }

        public NetworkInstanceId GetItemNetId() {
            return eItem == null ? NetworkInstanceId.Invalid : eItem.netId;
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
            if (eItemGo == null) {
                if (active) {
                    pInputManager.InvalidateControl(Control.Item);
                    PickUpItem();
                }
                return;
            }

            if (eItem.IsCharging()) {
                if (active) {
                    if (eItem.ShouldCancel()) {
                        pInputManager.InvalidateControl(Control.Item);
                        eItem.Cancel();
                        UpdateItemControlGraphic();
                        RemoveModifier(Modifier.CantAttack);
                        RemoveModifier(Modifier.CantMove);
                    } else {
                        eItem.KeepCharging(pInputManager.GetControlHoldTime(Control.Item));
                    }
                } else {
                    eItem.EndCharging(pInputManager.GetControlHoldTime(Control.Item));
                    UpdateItemControlGraphic();
                    RemoveModifier(Modifier.CantAttack);
                    RemoveModifier(Modifier.CantMove);
                }
            } else if (active && !Modifier.CantAttack.IsEnabled(eModifierState)) {
                var direction = GetControlDirection();
                var control = GetDirectionControl(direction);
                var holdTime = GetDirectionHoldTime(direction);
                if (direction != Direction.Invalid && holdTime < throwItemHoldTime) {
                    pInputManager.InvalidateControl(Control.Item);
                    pInputManager.InvalidateControl(control);
                    CmdLoseOwnershipOfItem(direction);
                    eItemGo = null;
                } else if (eItem.ShouldCharge()) {
                    AddModifier(Modifier.CantAttack);
                    AddModifier(Modifier.CantMove);
                    eItem.BeginCharging();
                } else if (eItem.ShouldThrow()) {
                    pInputManager.InvalidateControl(Control.Item);
                    CmdLoseOwnershipOfItem(eDirection);
                    eItemGo = null;
                } else {
                    pInputManager.InvalidateControl(Control.Item);
                    eItem.Use();
                    UpdateItemControlGraphic();
                }
                // If none of the above conditions were true, the item
                // is chargeable, but it can't charge right now (in cooldown)
                // so we just do nothing.
            }
        }

        void UseItem() {
            if (eItem.ShouldThrow()) {
                CmdLoseOwnershipOfItem(eDirection);
                eItemGo = null;
            } else if (eItem.ShouldCharge()) {
                // TODO: begin charging item
                // TODO: run this on server
                eItem.BeginCharging();
                eItem.Use();
            } else {
                eItem.Use();
            }
        }

        void PickUpItem(GameObject item = null) {
            item = CircleCastForItem(item);
            if (item != null) {
                if (item.GetComponent<Item>() == null) {
                    item = item.transform.parent.gameObject;
                }
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

            if (eItemGo == null) {
                pTouchButtons.SetItemButtonToItemGraphic();
            } else if (eItem.ShouldThrow()) {
                pTouchButtons.SetItemButtonToDiscardGraphic();
            } else {
                var graphic = eItem.itemButtonGraphic;
                if (graphic != null) {
                    pTouchButtons.SetItemButtonGraphic(graphic);
                }
            }
        }

        [Command]
        void CmdTakeOwnershipOfItem(GameObject itemGo) {
            var item = itemGo.GetComponent<Item>();
            if (!item.SetOwner(gameObject)) {
                return;
            }
            this.eItemGo = itemGo;
            this.eItem = item;
            RpcSetItem(itemGo);
            RpcUpdateItemControlGraphic();

            item.ChangeDirection(eDirection);
            itemGo.GetComponent<NetworkTransform>().enabled = false;
        }

        [Command]
        void CmdLoseOwnershipOfItem(Direction direction) {
            var itemGo = this.eItemGo;
            var item = this.eItem;
            this.eItemGo = null;
            this.eItem = null;
            RpcSetItem(null);

            item.SetOwner(null);
            item.Throw(direction);

            RpcUpdateItemControlGraphic();

            var networkTransform = itemGo.GetComponent<NetworkTransform>();
            networkTransform.enabled = true;
        }

        [ClientRpc]
        void RpcSetItem(GameObject itemGo) {
            this.eItemGo = itemGo;
            if (itemGo == null) {
                this.eItem = null;
            } else {
                this.eItem = itemGo.GetComponent<Item>();
            }
        }

        [Server]
        void MoveItemForChangeDirection(Direction direction) {
            eItem.ChangeDirection(direction);
        }

        /// If an item is passed, this function will return it
        /// only if it falls in the circle cast.
        /// If no item was passed, it will return the first item
        /// hit by the circle cast.
        /// For some items this may return a child GameObject.
        /// The caller is expected to check this.
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

        void ObjectSelected(GameObject gameObject) {
            if (gameObject == this.eItemGo) {
                UseItem();
                return;
            }

            // We can only hold one item at a time.
            if (eItemGo != null) {
                return;
            }
            if (gameObject.layer == Layers.items || gameObject.layer == Layers.noncollidingItems) {
                if (CircleCastForItem(gameObject) == gameObject) {
                    PickUpItem(gameObject);
                }
            }
        }

        void ControlCanceled(int control) {
            //
        }

        [ClientRpc]
        public void RpcRespawn(Vector3 position) {
            if (!hasAuthority) {
                return;
            }
            if (eItemGo != null) {
                Destroy(eItemGo);
                eItemGo = null;
            }
            transform.position = position;
            lRb.velocity = new Vector2(0f, 0f);
        }

        [Command]
        void CmdChangeDirection(Direction direction) {
            this.eDirection = direction;
            if (eItemGo != null) {
                MoveItemForChangeDirection(direction);
                RpcChangeItemDirection(direction);
            }
        }

        [Client]
        void ChangeDirection(Direction direction) {
            eDirection = direction;
            // Don't call this if the player hasn't been initialized yet,
            // we'll get nulls trying to access the sprite flip objects.
            if (lInitialized) {
                OnChangeDirection();
            }
        }

        /// eDirection contains the new direction.
        protected virtual void OnChangeDirection() {}

        [ClientRpc]
        void RpcChangeItemDirection(Direction direction) {
            if (eItem != null) {
                eItem.ChangeDirection(direction);
            }
        }

        public virtual void SetColor(Color color) {}

        [Server]
        public void Knockback(Vector2 force) {
            RpcKnockback(force);
            if (!Modifier.Invincible.IsEnabled(eModifierState)) {
                sKnockbackLockoutEndTime = Time.time + ((float)eDamage).Scale(0f, 1000f, 0.1f, 1.2f);
                if (!Modifier.InKnockback.IsEnabled(eModifierState)) {
                    AddModifier(Modifier.InKnockback);
                    AddModifier(Modifier.CantMove);
                    AddModifier(Modifier.CantAttack);
                }
            }
        }

        [ClientRpc]
        public void RpcKnockback(Vector2 force) {
            if (!hasAuthority) {
                return;
            }
            Modifier.Invincible.TryAddKnockback(eModifierState, lRb, force);
        }

        public void Interact(IAttack attack) {
            if (sAttackHit != null) {
                sAttackHit(attack.Type, attack.Properties);
            }
        }

        public int RegisterNetworkAttack(NetworkAttack attack) {
            lNetworkAttacks.Add(attack);
            return lNetworkAttacks.Count - 1;
        }

        [Command]
        public void CmdNetworkAttackSync(NetworkAttackMessage message) {
            RpcNetworkAttackSync(message);
        }

        [ClientRpc]
        void RpcNetworkAttackSync(NetworkAttackMessage message) {
            if (message.messageId < 0 || message.messageId >= lNetworkAttacks.Count) {
                Debug.LogWarning("Network attack index out of range, ignoring (" + message.messageId + ")");
                return;
            }

            lNetworkAttacks[message.messageId].ReceiveMessage(message);
        }
    }
}