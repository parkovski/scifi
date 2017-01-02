using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class Bow : Item {
    int arrows = 5;

    public GameObject normalArrow;
    public GameObject fireArrow;
    public GameObject rockArrow;
    public GameObject bombArrow;

    /// This array contains duplicates, because it is weighted
    /// to give more powerful arrows less often.
    GameObject[] arrowsArray;
    /// The type of arrow chosen for this bow
    GameObject arrowPrefab;
    /// The arrow shown with this bow. When the bow is spawned,
    /// an arrow is created. When it is picked up, the arrow
    /// disappears until the player starts to shoot.
    GameObject arrow;

    void Start() {
        BaseStart(aliveTime: 10f);

        if (arrowsArray == null) {
            arrowsArray = new [] {
                normalArrow, normalArrow, normalArrow, normalArrow,
                rockArrow, rockArrow,
                bombArrow,
                fireArrow,
            };
        }

        arrowPrefab = arrowsArray[Random.Range(0, arrowsArray.Length)];

        CreateArrow();
    }

    void CreateArrow() {
        arrow = Instantiate(arrowPrefab, gameObject.transform.position + new Vector3(.13f, 0f, 0f), Quaternion.identity);
        arrow.layer = Layers.items;
        arrow.transform.parent = gameObject.transform;
        //arrow.GetComponent<Rigidbody2D>().isKinematic = true;
        NetworkServer.Spawn(arrow);
    }

    void Update() {
        BaseUpdate();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }

    public override void OnPickup(Player player) {
        base.OnPickup(player);
        if (isServer) {
            Destroy(arrow);
        }
    }
    public override void OnDiscard(Player player) {
        base.OnDiscard(player);
        //CreateArrow();
    }

    public override bool ShouldThrow() {
        return arrows == 0;
    }

    public override bool ShouldCharge() {
        return arrows > 0;
    }

    public override void EndCharging(float chargeTime, Direction direction, NetworkInstanceId playerNetId) {
        --arrows;
        Vector2 force;
        if (direction == Direction.Left) {
            force = new Vector2(-20f, 10f);
        } else {
            force = new Vector2(20f, 10f);
        }
        GameController.Instance.CmdSpawnProjectile(
            arrowPrefab,
            playerNetId,
            netId,
            gameObject.transform.position,
            Quaternion.identity,
            force,
            0f
        );
    }
}