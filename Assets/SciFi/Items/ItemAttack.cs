using UnityEngine;
using System;

using SciFi.Players;
using SciFi.Players.Attacks;
using SciFi.Util.Extensions;

namespace SciFi.Items {
    /// Attack wrapper for items, handles networking.
    public class ItemAttack : Attack {
        Item item;
        IInputManager inputManager;
        float directionThrowTimeout;
        /// The network attack that wraps this attack.
        byte[] guidAsBytes;
        float beginChargeTime;

        public ItemAttack(Player player, IInputManager inputManager, float directionThrowTimeout)
            : base(player, false)
        {
            this.inputManager = inputManager;
            this.directionThrowTimeout = directionThrowTimeout;
            this.guidAsBytes = Guid.NewGuid().ToByteArray();
        }

        public void SetItem(Item item) {
            this.item = item;
            if (item == null) {
                CanCharge = false;
            } else {
                CanCharge = item.ShouldCharge();
            }
            player.UpdateItemControlGraphic();
        }

        public Direction GetThrowDirection() {
            if (inputManager.IsControlActive(Control.Left) && inputManager.GetControlHoldTime(Control.Left) < directionThrowTimeout) {
                return Direction.Left;
            } else if (inputManager.IsControlActive(Control.Right) && inputManager.GetControlHoldTime(Control.Right) < directionThrowTimeout) {
                return Direction.Right;
            } else if (inputManager.IsControlActive(Control.Down) && inputManager.GetControlHoldTime(Control.Down) < directionThrowTimeout) {
                return Direction.Down;
            } else if (inputManager.IsControlActive(Control.Up) && inputManager.GetControlHoldTime(Control.Up) < directionThrowTimeout) {
                return Direction.Up;
            }
            return Direction.Invalid;
        }

        public override void OnBeginCharging(Direction direction) {
            var throwDirection = GetThrowDirection();
            if (throwDirection != Direction.Invalid) {
                player.CmdDiscardItem(throwDirection);
                RequestCancel();
                return;
            }

            item.BeginCharging();
            if (item.ShouldCancel()) {
                RequestCancel();
            }
            player.ItemAttackSync(new ItemAttackMessage {
                sender = this.guidAsBytes,
                function = NetworkAttackFunction.OnBeginCharging,
                chargeTime = 0f,
            });
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            item.KeepCharging(chargeTime);
            if (item.ShouldCancel()) {
                RequestCancel();
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            if (item == null) {
                player.PickUpItem();
                return;
            }

            if (chargeTime > 0f) {
                item.EndCharging(chargeTime);
                CanCharge = item.ShouldCharge();
                player.UpdateItemControlGraphic();
                player.ItemAttackSync(new ItemAttackMessage {
                    sender = this.guidAsBytes,
                    function = NetworkAttackFunction.OnEndCharging,
                    chargeTime = chargeTime,
                });
            } else {
                var throwDirection = GetThrowDirection();
                if (throwDirection == Direction.Invalid && item.ShouldThrow()) {
                    throwDirection = direction;
                }
                if (throwDirection != Direction.Invalid) {
                    player.CmdDiscardItem(direction);
                    player.UpdateItemControlGraphic();
                } else {
                    item.EndCharging(chargeTime);
                    CanCharge = item.ShouldCharge();
                    player.UpdateItemControlGraphic();
                }
            }
        }

        public override void OnCancel() {
            if (item.IsCharging()) {
                player.ItemAttackSync(new ItemAttackMessage {
                    sender = this.guidAsBytes,
                    function = NetworkAttackFunction.OnCancel,
                    chargeTime = 0f,
                });
            }
            item.Cancel();
            CanCharge = item.ShouldCharge();
            player.UpdateItemControlGraphic();
        }

        public override void UpdateStateNonAuthoritative() {
            if (IsCharging) {
                item.KeepCharging(Time.time - beginChargeTime);
            }
        }

        public void ReceiveMessage(ItemAttackMessage message) {
            if (guidAsBytes.EqualsArray(message.sender)) {
                return;
            }

            switch (message.function) {
            case NetworkAttackFunction.OnBeginCharging:
                beginChargeTime = Time.time;
                this.IsCharging = true;
                item.BeginCharging();
                break;
            case NetworkAttackFunction.OnEndCharging:
                this.IsCharging = false;
                item.EndCharging(message.chargeTime);
                break;
            case NetworkAttackFunction.OnCancel:
                item.Cancel();
                this.IsCharging = false;
                break;
            }
        }
    }

    public struct ItemAttackMessage {
        public byte[] sender;
        public NetworkAttackFunction function;
        public float chargeTime;
    }
}