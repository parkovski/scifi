using UnityEngine.Networking;
using System;

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

        public void Add(SyncListUInt modifiers) {
            if (modifiers == null) {
                return;
            }

            ++modifiers[(int)Id];
        }

        public void Remove(SyncListUInt modifiers) {
            if (modifiers == null) {
                return;
            }

            if (modifiers[(int)Id] == 0) {
                return;
            }
            
            --modifiers[(int)Id];
        }

        public bool IsEnabled(SyncListUInt modifiers) {
            if (modifiers == null) {
                return false;
            }

            return modifiers[(int)Id] > 0;
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

        public static void Initialize(SyncListUInt modifiers) {
            int count = Count;
            for (var i = 0; i < count; i++) {
                modifiers.Add(0);
            }
        }
    }
}