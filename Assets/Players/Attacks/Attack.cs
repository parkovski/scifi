using UnityEngine;
using System;

using SciFi.Players.Modifiers;
using SciFi.Environment.State;

namespace SciFi.Players.Attacks {
    public enum AttackHit {
        None,
        HitOnly,
        HitAndDamage,
    }

    public enum AttackType {
        /// Does no damage
        Inert,
        /// Close-range attacks
        Melee,
        /// Thrown, launched, etc.
        Projectile,
    }

    [Flags]
    public enum AttackProperty {
        None           = 0x0,
        OnFire         = 0x1,
        Explosive      = 0x2,
        Frozen         = 0x4,
        AffectsGravity = 0x8,
        LightBeam      = 0x10,
        Electric       = 0x20,
    }

    public interface IAttackSource {
        AttackType Type { get; }
        AttackProperty Properties { get; }
        /// Can be null if no player owns this attack.
        Player Owner { get; }
    }

    public abstract class Attack : IStateSnapshotProvider<AttackState> {
        protected Player player;
        float cooldown;
        float lastFireTime;
        bool canCharge;
        bool isCharging;
        bool shouldCancel;
        ModifierMultiStateChange modifierStateChange;

        private static readonly ModId[] chargingModifiers = {
            ModId.CantMove,
            ModId.CantAttack,
        };

        // Extra parameters for child classes
        protected bool canFireDown = false;

        public Attack(Player player, bool canCharge)
            : this(player, 0.5f, canCharge)
        {
        }

        public Attack(Player player, float cooldown, bool canCharge) {
            this.player = player;
            this.cooldown = cooldown;
            this.canCharge = canCharge;
            this.modifierStateChange = new ModifierMultiStateChange(player, chargingModifiers, ShouldEndLockout);
        }

        public Player Player { get { return player; } }
        public float Cooldown { get { return cooldown; } }
        public bool CanCharge {
            get {
                return canCharge;
            }
            protected set {
                canCharge = value;
            }
        }
        public bool IsCharging {
            get {
                return isCharging;
            }
            set {
                isCharging = value;
            }
        }
        public bool CanFireDown { get { return canFireDown; } }
        public bool ShouldCancel {
            get { return shouldCancel; }
            set { shouldCancel = value; }
        }

        bool ShouldEndLockout() {
            return !isCharging;
        }

        void EndLockout() {
            modifierStateChange.End();
        }

        /// This function is called on every frame for non-authoritative
        /// attacks, so that network attacks can call OnKeepCharging on every frame.
        public virtual void UpdateStateNonAuthoritative() {}

        public void UpdateState(IInputManager inputManager, int control) {
            var direction = player.eDirection;
            bool cooldownOver = Time.time > lastFireTime + cooldown;
            if (!cooldownOver && !isCharging) {
                inputManager.InvalidateControl(control);
                return;
            }
            if (canFireDown && inputManager.IsControlActive(Control.Down)) {
                direction = Direction.Down;
            }
            Func<bool> checkCancel = () => {
                if (shouldCancel) {
                    inputManager.InvalidateControl(control);
                    Cancel();
                    EndLockout();
                    return true;
                } else {
                    return false;
                }
            };
            if (inputManager.IsControlActive(control)) {
                if (canCharge) {
                    if (isCharging) {
                        // Charging, continue.
                        if (!checkCancel()) {
                            OnKeepCharging(inputManager.GetControlHoldTime(control), direction);
                        }
                    } else {
                        // Not charging but button pressed, begin charging.
                        if (!player.IsModifierEnabled(ModId.CantAttack)) {
                            isCharging = true;
                            shouldCancel = false;
                            lastFireTime = Time.time;
                            modifierStateChange.Start();
                            OnBeginCharging(direction);
                            checkCancel();
                        }
                    }
                } else {
                    // Attack doesn't charge, fire immediately.
                    inputManager.InvalidateControl(control);
                    if (!player.IsModifierEnabled(ModId.CantAttack)) {
                        lastFireTime = Time.time;
                        OnEndCharging(0f, direction);
                    }
                }
            } else {
                if (isCharging) {
                    // Charging but button released, fire the attack.
                    isCharging = false;
                    if (!checkCancel()) {
                        OnEndCharging(inputManager.GetControlHoldTime(control), direction);
                        EndLockout();
                    }
                }
            }
        }

        public virtual void OnBeginCharging(Direction direction) {}
        public virtual void OnKeepCharging(float chargeTime, Direction direction) {}
        /// For non-charging attacks, this acts as the fire method,
        /// and the chargeTime parameter can be ignored.
        public abstract void OnEndCharging(float chargeTime, Direction direction);

        public abstract void OnCancel();

        private void Cancel() {
            shouldCancel = false;
            OnCancel();
            isCharging = false;
        }

        public void RequestCancel() {
            shouldCancel = true;
        }

        public static int LayerMask {
            get {
                return 1 << Layers.players | 1 << Layers.items;
            }
        }

        public static AttackHit GetAttackHit(int layer) {
            if (layer == Layers.players || layer == Layers.items || layer == Layers.shield) {
                return AttackHit.HitAndDamage;
            }
            if (layer == Layers.projectiles) {
                return AttackHit.HitOnly;
            }
            return AttackHit.None;
        }

        public void GetStateSnapshot(ref AttackState snapshot) {
            snapshot.canCharge = this.canCharge;
            // TODO: Fill in the rest.
        }
    }
}