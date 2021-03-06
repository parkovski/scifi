using System;

using SciFi.Players.Hooks;
using SciFi.Util;

namespace SciFi.Players.Modifiers {
    public enum ModId {
        OnFire        = 0,
        CantMove      = 1,
        CantJump      = 2,
        CantAttack    = 3,
        Invincible    = 4,
        Slow          = 5,
        Fast          = 6,
        UsingShield   = 7,
        Frozen        = 8,
        InGravityWell = 9,
        CanSmash      = 10,
        InKnockback   = 11,
    }

    public class ModifierCollection {
        public Modifier OnFire => modifiers[(int)ModId.OnFire];
        public Modifier CantMove => modifiers[(int)ModId.CantMove];
        public Modifier CantJump => modifiers[(int)ModId.CantJump];
        public Modifier CantAttack => modifiers[(int)ModId.CantAttack];
        public Modifier Invincible => modifiers[(int)ModId.Invincible];
        public Modifier Slow => modifiers[(int)ModId.Slow];
        public Modifier Fast => modifiers[(int)ModId.Fast];
        public Modifier UsingShield => modifiers[(int)ModId.UsingShield];
        public Modifier Frozen => modifiers[(int)ModId.Frozen];
        public Modifier InGravityWell => modifiers[(int)ModId.InGravityWell];
        public Modifier CanSmash => modifiers[(int)ModId.CanSmash];
        public Modifier InKnockback => modifiers[(int)ModId.InKnockback];

        Modifier[] modifiers;
        HookCollection hooks;

        public ModifierCollection(HookCollection hooks) {
            this.hooks = hooks;
            var count = Enum.GetNames(typeof(ModId)).Length;
            modifiers = new Modifier[count];
            Func<ModId, bool, bool> callback = ModifierWillChangeState;
            for (var i = 0; i < count; i++) {
                modifiers[i] = new Modifier((ModId)i, callback);
            }
        }

        bool ModifierWillChangeState(ModId id, bool newState) {
            return hooks.CallModifierStateChangedHooks(this, id, newState);
        }

        public string GetDebugString() {
            return string.Format(
                "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
                OnFire.IsEnabled()         ? "F" : "",
                CantMove.IsEnabled()       ? "M" : "",
                CantJump.IsEnabled()       ? "J" : "",
                CantAttack.IsEnabled()     ? "A" : "",
                Invincible.IsEnabled()     ? "I" : "",
                Slow.IsEnabled()           ? "-" : "",
                Fast.IsEnabled()           ? "+" : "",
                UsingShield.IsEnabled()    ? "U" : "",
                Frozen.IsEnabled()         ? "Z" : "",
                InGravityWell.IsEnabled()  ? "G" : "",
                CanSmash.IsEnabled()       ? "S" : "",
                InKnockback.IsEnabled()    ? "K" : ""
            );
        }

        public uint ToBitfield() {
            uint bits = 0u;
            for (var i = 0; i < modifiers.Length; i++) {
                if (modifiers[i].IsEnabled()) {
                    bits |= 1u << i;
                }
            }
            return bits;
        }

        public Modifier FromId(ModId id) {
            int index = (int)id;
            if (index < 0 || index > modifiers.Length) {
                throw new ArgumentOutOfRangeException("id", "Not a valid modifier ID");
            }
            return modifiers[index];
        }
    }

    public class ModifiersDebugField {
        ModifierCollection mods;
        string idStr;
        uint oldState;
        DebugField field;

        public ModifiersDebugField(ModifierCollection mods, string idStr) {
            this.mods = mods;
            this.idStr = idStr;
            this.oldState = mods.ToBitfield();
            this.field = new DebugField();
        }

        public void Update() {
            var state = mods.ToBitfield();
            if (state == oldState) { return; }
            oldState = state;
            field.Set(idStr + mods.GetDebugString());
        }
    }
}