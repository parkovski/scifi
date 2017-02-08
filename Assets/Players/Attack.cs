using UnityEngine;
using System;

using SciFi.Players.Modifiers;

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

    public interface IAttack {
        AttackType Type { get; }
        AttackProperty Properties { get; }
    }

    public abstract class Attack {
        protected Player player;
        float cooldown;
        float lastFireTime;
        bool canCharge;
        bool isCharging;
        bool shouldCancel;

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
        }

        public Player Player { get { return player; } }
        public float Cooldown { get { return cooldown; } }
        public bool CanCharge { get { return canCharge; } }
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
            protected set { shouldCancel = value; }
        }

        /// This function is called on every frame for non-authoritative
        /// attacks, so that network attacks can call OnKeepCharging on every frame.
        public virtual void UpdateStateNonAuthoritative() {}

        public void UpdateState(InputManager inputManager, int control) {
            var direction = player.eDirection;
            bool cooldownOver = Time.time > lastFireTime + cooldown;
            if (canFireDown && inputManager.IsControlActive(Control.Down)) {
                direction = Direction.Down;
            }
            if (inputManager.IsControlActive(control)) {
                if (canCharge) {
                    if (isCharging) {
                        // Charging, continue.
                        if (shouldCancel) {
                            inputManager.InvalidateControl(control);
                            shouldCancel = false;
                            Cancel();
                            player.RemoveModifier(Modifier.CantAttack);
                            player.RemoveModifier(Modifier.CantMove);
                        } else {
                            OnKeepCharging(inputManager.GetControlHoldTime(control), direction);
                        }
                    } else {
                        // Not charging but button pressed, begin charging.
                        if (!player.IsModifierEnabled(Modifier.CantAttack) && cooldownOver) {
                            isCharging = true;
                            shouldCancel = false;
                            lastFireTime = Time.time;
                            player.AddModifier(Modifier.CantAttack);
                            player.AddModifier(Modifier.CantMove);
                            OnBeginCharging(direction);
                        }
                    }
                } else {
                    // Attack doesn't charge, fire immediately.
                    inputManager.InvalidateControl(control);
                    if (!player.IsModifierEnabled(Modifier.CantAttack) && cooldownOver) {
                        lastFireTime = Time.time;
                        OnEndCharging(0f, direction);
                    }
                }
            } else {
                if (isCharging) {
                    // Charging but button released, fire the attack.
                    isCharging = false;
                    OnEndCharging(inputManager.GetControlHoldTime(control), direction);
                    player.RemoveModifier(Modifier.CantAttack);
                    player.RemoveModifier(Modifier.CantMove);
                }
            }
        }

        public virtual void OnBeginCharging(Direction direction) {}
        public virtual void OnKeepCharging(float chargeTime, Direction direction) {}
        /// For non-charging attacks, this acts as the fire method,
        /// and the chargeTime parameter can be ignored.
        public abstract void OnEndCharging(float chargeTime, Direction direction);

        public virtual void OnCancel() {}

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
    }
}