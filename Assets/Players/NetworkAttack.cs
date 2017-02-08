using System;

namespace SciFi.Players.Attacks {
    public class NetworkAttack : Attack {
        /// The underlying attack that calls are forwarded/synced to.
        Attack attack;
        /// The ID to pass to the player's callback that
        /// identifies this attack on that player.
        int messageId;
        /// A unique identifier for each copy, so that
        /// the originator doesn't run twice.
        byte[] guidAsBytes;
        /// How often to send KeepCharging messages, in seconds.
        float keepChargingSyncPeriod;
        /// The last time a KeepCharging message was sent.
        float lastKeepChargingSendTime;

        /// Create an attack wrapper that will sync to all players with the same syncId set.
        /// KeepCharging messages will be sent every keepChargingSyncPeriod seconds.
        public NetworkAttack(Attack attack, float keepChargingSyncPeriod)
            : base(attack.Player, attack.CanCharge)
        {
            this.attack = attack;
            this.canFireDown = attack.CanFireDown;
            this.guidAsBytes = Guid.NewGuid().ToByteArray();
            this.messageId = player.RegisterNetworkAttack(this);
            this.keepChargingSyncPeriod = keepChargingSyncPeriod;
        }

        public override void OnBeginCharging(Direction direction) {
            lastKeepChargingSendTime = 0f;
            attack.OnBeginCharging(direction);
            attack.IsCharging = true;
            player.CmdNetworkAttackSync(new NetworkAttackMessage {
                sender = this.guidAsBytes,
                messageId = this.messageId,
                function = NetworkAttackFunction.OnBeginCharging,
                direction = direction,
                chargeTime = 0f,
            });
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            attack.OnKeepCharging(chargeTime, direction);
            if (chargeTime > lastKeepChargingSendTime + keepChargingSyncPeriod) {
                lastKeepChargingSendTime = chargeTime;
                player.CmdNetworkAttackSync(new NetworkAttackMessage {
                    sender = this.guidAsBytes,
                    messageId = this.messageId,
                    function = NetworkAttackFunction.OnKeepCharging,
                    direction = direction,
                    chargeTime = chargeTime,
                });
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            attack.OnEndCharging(chargeTime, direction);
            attack.IsCharging = false;
            player.CmdNetworkAttackSync(new NetworkAttackMessage {
                sender = this.guidAsBytes,
                messageId = this.messageId,
                function = NetworkAttackFunction.OnEndCharging,
                direction = direction,
                chargeTime = chargeTime,
            });
        }

        public override void OnCancel() {
            attack.OnCancel();
            player.CmdNetworkAttackSync(new NetworkAttackMessage {
                sender = this.guidAsBytes,
                messageId = this.messageId,
                function = NetworkAttackFunction.OnCancel,
                direction = Direction.Invalid,
                chargeTime = 0f,
            });
        }

        /// This assumes these arrays are Guids, and their lengths are equal.
        bool GuidArraysEqual(byte[] guid1, byte[] guid2) {
            for (var i = 0; i < guid1.Length; i++) {
                if (guid1[i] != guid2[i]) {
                    return false;
                }
            }
            return true;
        }

        public void ReceiveMessage(NetworkAttackMessage message) {
            if (GuidArraysEqual(this.guidAsBytes, message.sender)) {
                return;
            }

            switch (message.function) {
            case NetworkAttackFunction.OnBeginCharging:
                attack.OnBeginCharging(message.direction);
                attack.IsCharging = true;
                break;
            case NetworkAttackFunction.OnKeepCharging:
                attack.OnKeepCharging(message.chargeTime, message.direction);
                break;
            case NetworkAttackFunction.OnEndCharging:
                attack.OnEndCharging(message.chargeTime, message.direction);
                attack.IsCharging = false;
                break;
            case NetworkAttackFunction.OnCancel:
                attack.RequestCancel();
                break;
            }
        }
    }

    public enum NetworkAttackFunction : byte {
        OnBeginCharging,
        OnKeepCharging,
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