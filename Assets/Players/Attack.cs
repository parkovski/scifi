namespace SciFi.Players.Attacks {
    public abstract class Attack {
        protected Player player;
        float cooldown;
        bool canCharge;
        bool isCharging;

        // Extra parameters for child classes
        protected bool canFireDown = false;

        public Attack(Player player, bool canCharge) {
            this.player = player;
            this.canCharge = canCharge;
        }

        public void UpdateState(InputManager inputManager, int control) {
            var direction = player.eDirection;
            if (canFireDown && inputManager.IsControlActive(Control.Down)) {
                direction = Direction.Down;
            }
            if (inputManager.IsControlActive(control)) {
                if (canCharge) {
                    if (isCharging) {
                        // Charging, continue.
                        OnKeepCharging(inputManager.GetControlHoldTime(control), direction);
                    } else {
                        // Not charging but button pressed, begin charging.
                        if (player.FeatureEnabled(PlayerFeature.Attack)) {
                            isCharging = true;
                            player.SuspendFeature(PlayerFeature.Attack);
                            player.SuspendFeature(PlayerFeature.Movement);
                            OnBeginCharging(direction);
                        }
                    }
                } else {
                    // Attack doesn't charge, fire immediately.
                    inputManager.InvalidateControl(control);
                    if (player.FeatureEnabled(PlayerFeature.Attack)) {
                        OnEndCharging(0f, direction);
                    }
                }
            } else {
                if (isCharging) {
                    // Charging but button released, fire the attack.
                    isCharging = false;
                    OnEndCharging(inputManager.GetControlHoldTime(control), direction);
                    player.ResumeFeature(PlayerFeature.Attack);
                    player.ResumeFeature(PlayerFeature.Movement);
                }
            }
        }

        public virtual void OnBeginCharging(Direction direction) {}
        public virtual void OnKeepCharging(float chargeTime, Direction direction) {}
        /// For non-charging attacks, this acts as the fire method,
        /// and the chargeTime parameter can be ignored.
        public abstract void OnEndCharging(float chargeTime, Direction direction);
        public void CancelCharging() {
            isCharging = false;
            OnCancelCharging();
        }
        public virtual void OnCancelCharging() {}
}
}