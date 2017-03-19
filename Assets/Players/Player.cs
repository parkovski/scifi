using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Network;
using SciFi.Environment;
using SciFi.Players.Attacks;
using SciFi.Players.Modifiers;
using SciFi.Players.Hooks;
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
        public static readonly Color blueTeamColor = new Color(0.5f, 0.5f, 1f, 1f);
        public static readonly Color blueTeamColorDark = new Color(0f, 0f, .6f, 1f);
        public static readonly Color redTeamColor = new Color(1f, .4f, .4f, 1f);
        public static readonly Color redTeamColorDark = new Color(.6f, 0f, 0f, 1f);
        public static readonly Color greenTeamColor = new Color(.1f, .6f, .1f, 1f);
        public static readonly Color greenTeamColorDark = new Color(0, .4f, 0f, 1f);
        public static readonly Color yellowTeamColor = new Color(1f, 1f, .4f, 1f);
        public static readonly Color yellowTeamColorDark = new Color(0.6f, 0.6f, 0f, 1f);

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
        public int eTeam = -1;
        [SyncVar, HideInInspector]
        public bool eShouldFallThroughOneWayPlatform;

        private bool lInitialized = false;
        protected Rigidbody2D lRb;
        protected IInputManager pInputManager;
        private int pGroundCollisions;
        private int pNumJumps;
        private bool pIsTouchingGround;
        private bool pExtraGravityFlag;
        protected GameObject eItemGo;
        protected Item eItem;
        private OneWayPlatform sCurrentOneWayPlatform;
        private HookCollection lHooks;
        private JumpForceHook lJumpForceHook;
        protected ModifierCollection eModifiers;
        private int pModifiersDebugField;
        private uint pOldModifierState;
        private List<NetworkAttack> lNetworkAttacks;
        private float sKnockbackLockoutEndTime = float.PositiveInfinity;
        private Vector2 sKnockbackForce;

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
        protected Attack eAttack3;
        private ItemAttack eItemAttack;
        //protected Attack eSpecialAttack;
        protected Shield eShield;

        public delegate void AttackHitHandler(AttackType type, AttackProperty properties);
        /// Emitted on the server when an attack hits.
        public event AttackHitHandler sAttackHit;

        void Start() {
            lRb = GetComponent<Rigidbody2D>();
            eDirection = Direction.Right;

            var shieldObj = Instantiate(shieldPrefab, transform.position + new Vector3(.6f, .1f), Quaternion.identity, transform);
            eShield = shieldObj.GetComponent<Shield>();

            lNetworkAttacks = new List<NetworkAttack>();

            lHooks = new HookCollection();
            eModifiers = new ModifierCollection(lHooks);
            // GameController will remove these when the game starts.
            eModifiers.CantAttack.Add();
            eModifiers.CantMove.Add();
            StandardHooks.Install(lHooks, eModifiers);
            lJumpForceHook = new StandardJumpForce();
            lJumpForceHook.Install(lHooks);
            pModifiersDebugField = DebugPrinter.Instance.NewField();

            if (pInputManager != null) {
                Initialize();
                lInitialized = true;
            }
        }

        public static Color TeamToColor(int team, bool dark = false) {
            switch (team) {
            case 0:
                return dark ? Player.blueTeamColorDark : Player.blueTeamColor;
            case 1:
                return dark ? Player.redTeamColorDark : Player.redTeamColor;
            case 2:
                return dark ? Player.greenTeamColorDark : Player.greenTeamColor;
            case 3:
                return dark ? Player.yellowTeamColorDark : Player.yellowTeamColor;
            default:
                return Color.clear;
            }
        }

        private void Initialize() {
            eItemAttack = new ItemAttack(this, pInputManager, throwItemHoldTime);
            OnInitialize();
        }

        protected abstract void OnInitialize();

        public bool IsInitialized() {
            return lInitialized;
        }

        public void GameControllerReady(GameController gameController, IInputManager inputManager) {
            // This is called twice on a host - once when the client game
            // starts and once when the server game starts.
            if (pInputManager != null) {
                return;
            }

            if (eTeam != -1) {
                GetComponent<SpriteOverlay>().SetColor(TeamToColor(eTeam));
            }

            pInputManager = inputManager;
            pLeftControl = new MultiPressControl(pInputManager, Control.Left, .4f);
            pRightControl = new MultiPressControl(pInputManager, Control.Right, .4f);

            if (isLocalPlayer) {
                var leftButton = GameObject.Find("LeftButton");
                if (leftButton != null) {
                    pTouchButtons = leftButton.GetComponent<TouchButtons>();
                }
            }

            if (lRb != null) {
                Initialize();
                lInitialized = true;
            }
        }

        protected void BaseCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Ground") {
                ++pGroundCollisions;
                pNumJumps = 0;
                pIsTouchingGround = true;

                var oneWay = collision.gameObject.GetComponent<OneWayPlatform>();
                if (oneWay != null) {
                    sCurrentOneWayPlatform = oneWay;
                }
            }
        }

        protected void BaseCollisionExit2D(Collision2D collision) {
            if (collision.gameObject.tag == "Ground") {
                if (--pGroundCollisions == 0) {
                    pIsTouchingGround = false;
                }

                var oneWay = collision.gameObject.GetComponent<OneWayPlatform>();
                if (oneWay != null) {
                    sCurrentOneWayPlatform = null;
                }
            }
        }

        /// The server will set the real state of these, but clients
        /// may speculatively add/remove them when appropriate.
        public void AddModifier(ModId id) {
            var mod = eModifiers.FromId(id);
            mod.Add();
            if (isServer) {
                RpcSetModifier(id, mod.Count);
            }
        }

        public void RemoveModifier(ModId id) {
            var mod = eModifiers.FromId(id);
            mod.Remove();
            if (isServer) {
                RpcSetModifier(id, mod.Count);
            }
        }

        public bool IsModifierEnabled(ModId id) {
            return eModifiers.FromId(id).IsEnabled();
        }

        [ClientRpc]
        void RpcSetModifier(ModId id, uint count) {
            eModifiers.FromId(id).Count = count;
        }

        [Server]
        void SetModifiersForMessage(NetworkAttackFunction function) {
            switch (function) {
            case NetworkAttackFunction.OnBeginCharging:
                eModifiers.CantMove.Add();
                eModifiers.CantAttack.Add();
                break;
            case NetworkAttackFunction.OnEndCharging:
            case NetworkAttackFunction.OnCancel:
                eModifiers.CantMove.Remove();
                eModifiers.CantAttack.Remove();
                break;
            }
        }

        public void SetJumpBehaviour(JumpForceHook hook) {
            lJumpForceHook.Remove(lHooks);
            hook.Install(lHooks);
            lJumpForceHook = hook;
        }

        void HandleLeftRightInput(MultiPressControl control, Direction direction) {
            if (control.IsActive()) {
                float axisAmount;
                var presses = control.GetPresses();
                if (presses == 0) {
                    axisAmount = 0;
                } else if (presses == 1) {
                    axisAmount = 0.5f;
                } else {
                    axisAmount = 1f;
                }
                var localMaxSpeed = lHooks.CallMaxSpeedHooks(axisAmount, maxSpeed);
                bool canSpeedUp;
                if (Mathf.Approximately(localMaxSpeed, 0f)) {
                    canSpeedUp = false;
                } else if (direction == Direction.Left) {
                    canSpeedUp = lRb.velocity.x > -localMaxSpeed;
                } else {
                    canSpeedUp = lRb.velocity.x < localMaxSpeed;
                }
                if (canSpeedUp) {
                    lRb.AddForce(new Vector2(lHooks.CallWalkForceHooks(direction, axisAmount, walkForce), 0f));
                }

                // Without the cached parameter, this will get triggered
                // multiple times until the direction has had a chance to sync.
                if (this.eDirection != direction && !eModifiers.CantMove.IsEnabled()) {
                    this.eDirection = direction;
                    CmdChangeDirection(direction);
                }
            }
        }

        void AddDampingForce() {
            if (lRb.velocity.x < .25f && lRb.velocity.x > -.25f) {
                //lRb.velocity = new Vector2(0f, lRb.velocity.y);
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

        /// Make the players fall faster than they would
        /// with just regular gravity.
        void AddExtraGravity() {
            if (lRb.velocity.y < 0f && !pIsTouchingGround) {
                var force = Mathf.Clamp(lRb.velocity.y, -5f, 0f).Scale(0f, -5f, -1000f, 0f);
                lRb.AddForce(new Vector3(0f, force, 0f));
            }
        }

        protected void BaseInput() {
            if (pInputManager == null) {
                return;
            }
            if (eLives <= 0) {
                return;
            }
            if (!hasAuthority) {
                eAttack1.UpdateStateNonAuthoritative();
                eAttack2.UpdateStateNonAuthoritative();
                eAttack3.UpdateStateNonAuthoritative();
                eItemAttack.UpdateStateNonAuthoritative();
                return;
            }

            pLeftControl.Update();
            pRightControl.Update();

            HandleLeftRightInput(pLeftControl, Direction.Left);
            HandleLeftRightInput(pRightControl, Direction.Right);
            if (!pLeftControl.IsActive()
                && !pRightControl.IsActive()
                && !IsModifierEnabled(ModId.InKnockback)
                && pIsTouchingGround
            ) {
                AddDampingForce();
            }
            AddExtraGravity();

            if (pInputManager.IsControlActive(Control.Up) && !eModifiers.CantMove.IsEnabled() && !eModifiers.CantJump.IsEnabled()) {
                pInputManager.InvalidateControl(Control.Up);
                var jf = lHooks.CallJumpForceHooks(pIsTouchingGround, pNumJumps++, jumpForce);
                if (!Mathf.Approximately(jf, 0f)) {
                    if (pNumJumps > 0 && lRb.velocity.y < minDoubleJumpVelocity) {
                        lRb.velocity = new Vector2(lRb.velocity.x, minDoubleJumpVelocity);
                    }
                    lRb.AddForce(new Vector2(0f, jf), ForceMode2D.Impulse);
                }
            }

            if (pInputManager.IsControlActive(Control.Down) && !eModifiers.CantMove.IsEnabled()) {
                if (!eShouldFallThroughOneWayPlatform) {
                    eShouldFallThroughOneWayPlatform = true;
                    CmdFallThroughOneWayPlatform();
                }
                if (!eShield.IsActive()) {
                    eShield.Activate();
                }
            } else {
                if (eShouldFallThroughOneWayPlatform) {
                    eShouldFallThroughOneWayPlatform = false;
                    CmdStopFallingThroughOneWayPlatform();
                }
                if (eShield.IsActive()) {
                    eShield.Deactivate();
                }
            }

            eAttack1.UpdateState(pInputManager, Control.Attack1);
            eAttack2.UpdateState(pInputManager, Control.Attack2);
            eAttack3.UpdateState(pInputManager, Control.Attack3);
            eItemAttack.UpdateState(pInputManager, Control.Item);

#if UNITY_EDITOR
            var modifierState = eModifiers.ToBitfield();
            if (modifierState != pOldModifierState) {
                pOldModifierState = modifierState;
                DebugPrinter.Instance.SetField(pModifiersDebugField, "P" + (eId+1) + ": " + eModifiers.GetDebugString());
            }
#endif
        }

        [Command]
        void CmdFallThroughOneWayPlatform() {
            eShouldFallThroughOneWayPlatform = true;
            if (sCurrentOneWayPlatform != null) {
                sCurrentOneWayPlatform.FallThrough(gameObject);
            }
        }

        [Command]
        void CmdStopFallingThroughOneWayPlatform() {
            eShouldFallThroughOneWayPlatform = false;
        }

        protected void Update() {
            if (!isServer) {
                return;
            }

            if (eModifiers.InKnockback.IsEnabled() && Time.time > sKnockbackLockoutEndTime) {
                eModifiers.InKnockback.Remove();
                eModifiers.CantAttack.Remove();
                eModifiers.CantMove.Remove();
                sKnockbackLockoutEndTime = float.PositiveInfinity;
            }
        }

        [Command]
        public void CmdSpawnProjectileFlipped(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 velocity,
            float angularVelocity,
            bool flipX
        ) {
            SpawnProjectile(
                GameController.IndexToPrefab(prefabIndex),
                position,
                rotation,
                velocity,
                angularVelocity,
                flipX
            );
        }

        [Server]
        void SpawnProjectile(
            GameObject prefab,
            Vector2 position,
            Quaternion rotation,
            Vector2 velocity,
            float angularVelocity,
            bool flipX
        ) {
            var obj = Instantiate(prefab, position, rotation);
            var projectile = obj.GetComponent<Projectile>();
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null) {
                projectile.SetInitialVelocity(velocity);
                rb.velocity = velocity;
                rb.angularVelocity = angularVelocity;
            }
            NetworkServer.Spawn(obj);
            projectile.Enable(netId, GetItemNetId(), flipX);
            var sync = obj.GetComponent<InitialStateSync>();
            if (sync != null) {
                sync.Resync();
            }
        }

        [Command]
        public void CmdSpawnPooledProjectile(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 velocity,
            float angularVelocity
        ) {
            SpawnPooledProjectile(
                prefabIndex,
                position,
                rotation,
                velocity,
                angularVelocity,
                false
            );
        }

        [Command]
        public void CmdSpawnPooledProjectileFlipped(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 velocity,
            float angularVelocity,
            bool flipX
        ) {
            SpawnPooledProjectile(
                prefabIndex,
                position,
                rotation,
                velocity,
                angularVelocity,
                flipX
            );
        }

        [Server]
        public GameObject SpawnPooledProjectile(
            int prefabIndex,
            Vector2 position,
            Quaternion rotation,
            Vector2 velocity,
            float angularVelocity,
            bool flipX
        ) {
            return SpawnPooledProjectileScaled(
                prefabIndex,
                position,
                null,
                rotation,
                velocity,
                angularVelocity,
                flipX
            );
        }

        [Server]
        public GameObject SpawnPooledProjectileScaled(
            int prefabIndex,
            Vector2 position,
            Vector3? scale,
            Quaternion rotation,
            Vector2 velocity,
            float angularVelocity,
            bool flipX
        ) {
            var obj = GameController.Instance.GetFromNetPool(prefabIndex, position, rotation);
            if (scale != null) {
                obj.transform.localScale = scale.Value;
            }
            var projectile = obj.GetComponent<Projectile>();
            projectile.Enable(netId, GetItemNetId(), flipX);
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null) {
                projectile.SetInitialVelocity(velocity);
                rb.velocity = velocity;
                rb.angularVelocity = angularVelocity;
            }
            var sync = obj.GetComponent<InitialStateSync>();
            if (sync != null) {
                sync.Resync();
            }
            return obj;
        }

        public NetworkInstanceId GetItemNetId() {
            return eItem == null ? NetworkInstanceId.Invalid : eItem.netId;
        }

        public void PickUpItem() {
            var item = CircleCastForItem();
            if (item != null) {
                if (item.GetComponent<Item>() == null) {
                    item = item.transform.parent.gameObject;
                    if (item.GetComponent<Item>() == null) {
                        return;
                    }
                }
                CmdTakeOwnershipOfItem(item);
            }
        }

        [ClientRpc]
        void RpcUpdateItemControlGraphic() {
            UpdateItemControlGraphic();
        }

        public void UpdateItemControlGraphic() {
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
            eItemAttack.SetItem(item);
            RpcSetItem(itemGo);
            RpcUpdateItemControlGraphic();

            item.ChangeDirection(eDirection);
            var networkTransform = itemGo.GetComponent<SFNetworkTransform>();
            if (networkTransform != null) {
                networkTransform.enabled = false;
            }
        }

        [Command]
        public void CmdLoseOwnershipOfItem(Direction direction) {
            var itemGo = this.eItemGo;
            var item = this.eItem;
            this.eItemGo = null;
            this.eItem = null;
            eItemAttack.SetItem(null);
            RpcSetItem(null);

            item.SetOwner(null);
            item.Throw(direction);

            RpcUpdateItemControlGraphic();

            var networkTransform = itemGo.GetComponent<SFNetworkTransform>();
            if (networkTransform != null) {
                networkTransform.enabled = true;
            }
        }

        [ClientRpc]
        void RpcSetItem(GameObject itemGo) {
            this.eItemGo = itemGo;
            if (itemGo == null) {
                this.eItem = null;
                eItemAttack.SetItem(null);
            } else {
                this.eItem = itemGo.GetComponent<Item>();
                eItemAttack.SetItem(eItem);
            }
        }

        [Server]
        void MoveItemForChangeDirection(Direction direction) {
            eItem.ChangeDirection(direction);
        }

        /// Returns the first item hit by the circle cast.
        /// For some items this may return a child GameObject.
        /// The caller is expected to check this.
        GameObject CircleCastForItem() {
            var hits = Physics2D.CircleCastAll(
                gameObject.transform.position,
                1f,
                Vector2.zero,
                Mathf.Infinity,
                1 << Layers.items | 1 << Layers.noncollidingItems);
            if (hits.Length == 0) {
                return null;
            }

            return hits[0].collider.gameObject;
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
            GetComponent<SFNetworkTransform>().SnapTo(position);
            lRb.velocity = new Vector2(0f, 0f);
        }

        [ClientRpc]
        public void RpcYouDeadFool() {
            lRb.isKinematic = true;
            lRb.gravityScale = 0;
            lRb.velocity = new Vector2(0, 0);
            if (hasAuthority) {
                GetComponent<SFNetworkTransform>().SnapTo(new Vector2(-1000, -1000));
            }
        }

        [Command]
        void CmdChangeDirection(Direction direction) {
            this.eDirection = direction;
            if (eItemGo != null) {
                MoveItemForChangeDirection(direction);
                RpcChangeItemDirection(direction);
            }
        }

        /// Called by a SyncVar hook.
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

        [Server]
        public void Hit(int damage) {
        }

        [Server]
        public void Knockback(Vector2 force, bool resetVelocity) {
            if (eModifiers.Invincible.IsEnabled()) {
                return;
            }
            sKnockbackLockoutEndTime = Time.time + ((float)eDamage).Scale(0f, 1000f, 0.15f, 1.35f);
            if (!eModifiers.InKnockback.IsEnabled()) {
                eModifiers.InKnockback.Add();
                eModifiers.CantMove.Add();
                eModifiers.CantAttack.Add();
                RpcKnockback(force, resetVelocity);
            } else {
                // If we get hit with another attack during the lockout period,
                // we should just add a force in the same direction, regardless
                // of where the attack came from.
                if ((sKnockbackForce.x < 0) != (force.x < 0)) {
                    force.x = -force.x;
                }
                if ((sKnockbackForce.y < 0) != (force.y < 0)) {
                    force.y = -force.y;
                }
                RpcKnockback(force, false);
            }
            sKnockbackForce = force;
        }

        [ClientRpc]
        void RpcKnockback(Vector2 force, bool resetVelocity) {
            if (!hasAuthority) {
                return;
            }
            if (eModifiers.Invincible.IsEnabled()) {
                return;
            }
            eAttack1.RequestCancel();
            eAttack2.RequestCancel();
            eAttack3.RequestCancel();
            eItemAttack.RequestCancel();
            if (resetVelocity) {
                lRb.velocity = Vector2.zero;
            }
            lRb.AddForce(force);
        }

        public void Interact(IAttackSource attack) {
            if (sAttackHit != null) {
                sAttackHit(attack.Type, attack.Properties);
            }
        }

        public int RegisterNetworkAttack(NetworkAttack attack) {
            lNetworkAttacks.Add(attack);
            return lNetworkAttacks.Count - 1;
        }

        public void NetworkAttackSync(NetworkAttackMessage message) {
            if (isServer) {
                RpcNetworkAttackSync(message);
                SetModifiersForMessage(message.function);
            } else if (isClient && hasAuthority) {
                CmdNetworkAttackSync(message);
            }
        }

        [Command]
        void CmdNetworkAttackSync(NetworkAttackMessage message) {
            RpcNetworkAttackSync(message);
            SetModifiersForMessage(message.function);
            lNetworkAttacks[message.messageId].ReceiveMessage(message);
        }

        [ClientRpc]
        void RpcNetworkAttackSync(NetworkAttackMessage message) {
            if (isServer) {
                return;
            }
            if (message.messageId < 0 || message.messageId >= lNetworkAttacks.Count) {
                Debug.LogWarning("Network attack index out of range, ignoring (" + message.messageId + ")");
                return;
            }

            lNetworkAttacks[message.messageId].ReceiveMessage(message);
        }

        public void ItemAttackSync(ItemAttackMessage message) {
            if (isServer) {
                RpcItemAttackSync(message);
                SetModifiersForMessage(message.function);
            } else if (isClient && hasAuthority) {
                CmdItemAttackSync(message);
            }
        }

        [Command]
        void CmdItemAttackSync(ItemAttackMessage message) {
            RpcItemAttackSync(message);
            SetModifiersForMessage(message.function);
            eItemAttack.ReceiveMessage(message);
        }

        [ClientRpc]
        void RpcItemAttackSync(ItemAttackMessage message) {
            if (isServer) {
                return;
            }
            eItemAttack.ReceiveMessage(message);
        }
    }
}