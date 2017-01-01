public abstract class Attack {
    protected Player player;
    float cooldown;
    bool canCharge;
    bool isCharging;

    public Attack(Player player, bool canCharge) {
        this.player = player;
        this.canCharge = canCharge;
    }

    public void UpdateState(InputManager inputManager, int control) {
        var direction = player.direction;
        if (inputManager.IsControlActive(Control.Down)) {
            direction = Direction.Down;
        }
        if (inputManager.IsControlActive(control)) {
            if (canCharge) {
                if (isCharging) {
                    KeepCharging(inputManager.GetControlHoldTime(control));
                } else {
                    isCharging = true;
                    BeginCharging();
                }
            } else {
                inputManager.InvalidateControl(control);
                EndCharging(0f, direction);
            }
        } else {
            if (isCharging) {
                isCharging = false;
                EndCharging(inputManager.GetControlHoldTime(control), direction);
            }
        }
    }

    public virtual void BeginCharging() {}
    public virtual void KeepCharging(float chargeTime) {}
    /// For non-charging attacks, this acts as the fire method,
    /// and the chargeTime parameter can be ignored.
    public abstract void EndCharging(float chargeTime, Direction direction);
}