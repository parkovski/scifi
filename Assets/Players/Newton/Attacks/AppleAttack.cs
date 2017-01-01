using UnityEngine;

public class AppleAttack : Attack {
    const float verticalForce = 50f;
    const float horizontalForce = 20f;
    const float torqueRange = 5f;
    GameObject apple;

    public AppleAttack(Player player, GameObject apple)
        : base(player, false)
    {
        this.apple = apple;
    }

    public override void EndCharging(float chargeTime, Direction direction) {
        var force = new Vector2(0f, verticalForce);
        if (direction == Direction.Down) {
            force = new Vector2(0f, -horizontalForce);
        } else if (direction == Direction.Left) {
            force += new Vector2(-horizontalForce, 0f);
        } else {
            force += new Vector2(horizontalForce, 0f);
        }

        var torque = Random.Range(-torqueRange, torqueRange);
        GameController.Instance.CmdSpawnProjectile(
            apple,
            player.netId,
            player.GetItemNetId(),
            player.gameObject.transform.position,
            Quaternion.identity,
            force,
            torque
        );
    }
}