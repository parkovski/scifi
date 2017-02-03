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
        HasGelignite  = 7,
        UsingShield   = 8,
        Frozen        = 9,
        InGravityWell = 10,
        CanSmash      = 11,
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

        public static OnFire OnFire { get; private set; }
        public static CantMove CantMove { get; private set; }
        public static CantJump CantJump { get; private set; }
        public static CantAttack CantAttack { get; private set; }
        public static Invincible Invincible { get; private set; }
        public static Slow Slow { get; private set; }
        public static Fast Fast { get; private set; }
        public static HasGelignite HasGelignite { get; private set; }
        public static UsingShield UsingShield { get; private set; }
        public static Frozen Frozen { get; private set; }
        public static InGravityWell InGravityWell { get; private set; }
        public static CanSmash CanSmash { get; private set; }

        static Modifier() {
            OnFire = new OnFire();
            CantMove = new CantMove();
            CantJump = new CantJump();
            CantAttack = new CantAttack();
            Invincible = new Invincible();
            Slow = new Slow();
            Fast = new Fast();
            HasGelignite = new HasGelignite();
            UsingShield = new UsingShield();
            Frozen = new Frozen();
            InGravityWell = new InGravityWell();
            CanSmash = new CanSmash();
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
            case ModId.HasGelignite:
                return HasGelignite;
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
            for (var i = 0; i < Count; i++) {
                modifiers.Add(0);
            }
        }
    }
}