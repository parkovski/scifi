using UnityEngine;
using System;

using SciFi.Players;
using SciFi.Players.Modifiers;
using SciFi.Items;

namespace SciFi.Environment.State {
    /// Stats about this game for the player's profile.
    /// Reported to the server at the end of the game.
    public struct PlayerGameStats {
        public ulong profileId;
        public short kills;
        public short deaths;
        public int damageDealt;
    }

    /// Things I want to accomplish with snapshots:
    /// - AI decision making
    /// - Game recording / replays
    /// - Logging / visualization
    /// - Testing / validation
    /// - Lag compensation
    public struct PlayerSnapshot {
        public Team team;

        public short lives;
        public short damage;
        public sbyte magic;
        public Vector2 position;
        public Vector2 velocity;
        public AttackState[] attacks;
    }
}