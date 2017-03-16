using UnityEngine;
using System;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    /// This is a wrapper class that will mirror the enclosed attack's
    /// events across the network.
    public class NetworkAttack : Attack {
        /// The underlying attack that calls are forwarded/synced to.
        Attack attack;
        /// The ID to pass to the player's callback that
        /// identifies this attack on that player.
        int messageId;
        /// A unique identifier for each copy, so that
        /// the originator doesn't run twice.
        byte[] guidAsBytes;
        float beginChargeTime;
        Direction chargeDirection;

        /// Create an attack wrapper that will sync to all players with the same syncId set.
        /// KeepCharging messages will be sent every keepChargingSyncPeriod seconds.
        public NetworkAttack(Attack attack)
            : base(attack.Player, attack.Cooldown, attack.CanCharge)
        {
            this.attack = attack;
            this.canFireDown = attack.CanFireDown;
            this.guidAsBytes = Guid.NewGuid().ToByteArray();
            this.messageId = player.RegisterNetworkAttack(this);
        }

        public override void UpdateStateNonAuthoritative() {
            if (IsCharging) {
                OnKeepCharging(Time.time - beginChargeTime, chargeDirection);
            }
        }

        public override void OnBeginCharging(Direction direction) {
            attack.IsCharging = true;
            attack.OnBeginCharging(direction);
            this.ShouldCancel = attack.ShouldCancel;
            player.NetworkAttackSync(new NetworkAttackMessage {
                sender = this.guidAsBytes,
                messageId = this.messageId,
                function = NetworkAttackFunction.OnBeginCharging,
                direction = direction,
                chargeTime = 0f,
            });
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            attack.OnKeepCharging(chargeTime, direction);
            this.ShouldCancel = attack.ShouldCancel;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            attack.OnEndCharging(chargeTime, direction);
            attack.IsCharging = false;
            player.NetworkAttackSync(new NetworkAttackMessage {
                sender = this.guidAsBytes,
                messageId = this.messageId,
                function = NetworkAttackFunction.OnEndCharging,
                direction = direction,
                chargeTime = chargeTime,
            });
        }

        public override void OnCancel() {
            attack.OnCancel();
            attack.IsCharging = false;
            attack.ShouldCancel = false;
            player.NetworkAttackSync(new NetworkAttackMessage {
                sender = this.guidAsBytes,
                messageId = this.messageId,
                function = NetworkAttackFunction.OnCancel,
                direction = Direction.Invalid,
                chargeTime = 0f,
            });
        }

        public void ReceiveMessage(NetworkAttackMessage message) {
            if (this.guidAsBytes.EqualsArray(message.sender)) {
                return;
            }

            switch (message.function) {
            case NetworkAttackFunction.OnBeginCharging:
                beginChargeTime = Time.time;
                chargeDirection = message.direction;
                this.IsCharging = true;
                attack.IsCharging = true;
                attack.OnBeginCharging(message.direction);
                break;
            case NetworkAttackFunction.OnEndCharging:
                attack.OnEndCharging(message.chargeTime, message.direction);
                this.IsCharging = false;
                attack.IsCharging = false;
                break;
            case NetworkAttackFunction.OnCancel:
                attack.OnCancel();
                this.IsCharging = false;
                attack.IsCharging = false;
                break;
            }
        }
    }

    public enum NetworkAttackFunction : byte {
        OnBeginCharging,
        OnEndCharging,
        OnCancel,
    }

    public struct NetworkAttackMessage {
        /// The sender's GUID converted to a byte array.
        /// Dunno why Unity doesn't support sending Guids.
        public byte[] sender;
        public int messageId;
        public NetworkAttackFunction function;
        public Direction direction;
        public float chargeTime;
    }
}