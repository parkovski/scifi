using System;
using System.Collections.Generic;

namespace SciFi.Players.Modifiers {
    public enum ModId : uint {
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
    }

    public abstract class Modifier {
        public abstract ModId Id { get; }

        public void Add(IList<uint> modifiers) {
            if (modifiers == null) {
                return;
            }

            ++modifiers[(int)Id];
        }

        public void Remove(IList<uint> modifiers) {
            if (modifiers == null) {
                return;
            }

            if (modifiers[(int)Id] == 0) {
                return;
            }
            
            --modifiers[(int)Id];
        }

        public bool IsEnabled(IList<uint> modifiers) {
            if (modifiers == null) {
                return false;
            }

            return modifiers[(int)Id] > 0;
        }

        public static uint GetState(IList<uint> modifiers) {
            if (modifiers == null) {
                return 0;
            }

            return
                (OnFire.IsEnabled(modifiers) ? 1u : 0u)          << 0
                | (CantMove.IsEnabled(modifiers) ? 1u : 0u)      << 1
                | (CantJump.IsEnabled(modifiers) ? 1u : 0u)      << 2
                | (CantAttack.IsEnabled(modifiers) ? 1u : 0u)    << 3
                | (Invincible.IsEnabled(modifiers) ? 1u : 0u)    << 4
                | (Slow.IsEnabled(modifiers) ? 1u : 0u)          << 5
                | (Fast.IsEnabled(modifiers) ? 1u : 0u)          << 6
                | (UsingShield.IsEnabled(modifiers) ? 1u : 0u)   << 7
                | (Frozen.IsEnabled(modifiers) ? 1u : 0u)        << 8
                | (InGravityWell.IsEnabled(modifiers) ? 1u : 0u) << 9
                | (CanSmash.IsEnabled(modifiers) ? 1u : 0u)      << 10;
        }

        public static Marker OnFire { get; private set; }
        public static CantMove CantMove { get; private set; }
        public static Marker CantJump { get; private set; }
        public static Marker CantAttack { get; private set; }
        public static Invincible Invincible { get; private set; }
        public static SpeedModifier Slow { get; private set; }
        public static SpeedModifier Fast { get; private set; }
        public static Marker UsingShield { get; private set; }
        public static Marker Frozen { get; private set; }
        public static Marker InGravityWell { get; private set; }
        public static Marker CanSmash { get; private set; }

        static Modifier() {
            OnFire = new Marker(ModId.OnFire);
            CantMove = new CantMove();
            CantJump = new Marker(ModId.CantJump);
            CantAttack = new Marker(ModId.CantAttack);
            Invincible = new Invincible();
            Slow = new Slow();
            Fast = new Fast();
            UsingShield = new Marker(ModId.UsingShield);
            Frozen = new Marker(ModId.Frozen);
            InGravityWell = new Marker(ModId.InGravityWell);
            CanSmash = new Marker(ModId.CanSmash);
        }

        public static string GetDebugString(IList<uint> modifiers) {
            return string.Format(
                "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}",
                OnFire.IsEnabled(modifiers)         ? "F" : "",
                CantMove.IsEnabled(modifiers)       ? "M" : "",
                CantJump.IsEnabled(modifiers)       ? "J" : "",
                CantAttack.IsEnabled(modifiers)     ? "A" : "",
                Invincible.IsEnabled(modifiers)     ? "I" : "",
                Slow.IsEnabled(modifiers)           ? "-" : "",
                Fast.IsEnabled(modifiers)           ? "+" : "",
                UsingShield.IsEnabled(modifiers)    ? "U" : "",
                Frozen.IsEnabled(modifiers)         ? "Z" : "",
                InGravityWell.IsEnabled(modifiers)  ? "G" : "",
                CanSmash.IsEnabled(modifiers)       ? "S" : ""
            );
        }

        public static Modifier FromId(ModId id) {
            switch (id) {
            case ModId.OnFire:
                return OnFire;
            case ModId.CantMove:
                return CantMove;
            case ModId.CantJump:
                return CantJump;
            case ModId.CantAttack:
                return CantAttack;
            case ModId.Invincible:
                return Invincible;
            case ModId.Slow:
                return Slow;
            case ModId.Fast:
                return Fast;
            case ModId.UsingShield:
                return UsingShield;
            case ModId.Frozen:
                return Frozen;
            case ModId.InGravityWell:
                return InGravityWell;
            case ModId.CanSmash:
                return CanSmash;
            default:
                throw new ArgumentOutOfRangeException("id", "Not a valid modifier ID");
            }
        }

        public static int Count {
            get {
                return Enum.GetNames(typeof(ModId)).Length;
            }
        }

        public static void Initialize(IList<uint> modifiers) {
            int count = Count;
            for (var i = 0; i < count; i++) {
                modifiers.Add(0);
            }
        }
    }
}