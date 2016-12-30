using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class Newton : Player {
    public GameObject apple;
    public GameObject calc1;
    public GameObject calc2;
    public GameObject calc3;
    private GameObject chargingCalcBook;
    private int calcBookPower = 0;

    private Animator animator;
    private bool walkAnimationPlaying;

    // Physics parameters
    const float appleHorizontalForce = 50f;
    const float appleVerticalForce = 20f;
    // Torque is random from (-appleTorqueRange, appleTorqueRange).
    const float appleTorqueRange = 5f;

    void Start() {
        BaseStart();

        attack2CanCharge = true;
        //animator = GetComponent<Animator>();
    }

    public override void OnStartLocalPlayer() {
        //GetComponent<SpriteRenderer>().color = new Color(.8f, .9f, 1f, .8f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision) {
        BaseCollisionExit2D(collision);
    }

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        BaseInput();
    }
/*
    void Update() {
        if (rb.velocity.x > .5f) {
            if (!walkAnimationPlaying) {
                walkAnimationPlaying = true;
                animator.SetTrigger("WalkRight");
            }
        } else if (rb.velocity.x < -.5f) {
            if (!walkAnimationPlaying) {
                walkAnimationPlaying = true;
                animator.SetTrigger("WalkLeft");
            }
        } else {
            if (walkAnimationPlaying) {
                walkAnimationPlaying = false;
                animator.SetTrigger("Stand");
            }
        }
    }
*/
    [ClientRpc]
    protected override void RpcChangeDirection(Direction direction) {
        foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
            sr.flipX = !sr.flipX;
        }
        for (var i = 0; i < gameObject.transform.childCount; i++) {
            var child = gameObject.transform.GetChild(i);
            child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
        }
    }

    [Command]
    void CmdSpawnApple(NetworkInstanceId netId, NetworkInstanceId itemNetId, bool down) {
        var force = transform.up * appleVerticalForce;
        if (down) {
            force = -transform.up * appleHorizontalForce;
        } else {
            if (data.direction == Direction.Left) {
                force += -transform.right * appleHorizontalForce;
            } else {
                force += transform.right * appleHorizontalForce;
            }
        }
        var torque = Random.Range(-appleTorqueRange, appleTorqueRange);
        SpawnProjectile(netId, itemNetId, apple, gameObject.transform.position, force, torque);
    }

    protected override void Attack1() {
        var itemNetId = item == null ? NetworkInstanceId.Invalid : item.GetComponent<ItemData>().item.netId;
        CmdSpawnApple(netId, itemNetId, inputManager.IsControlActive(Control.Down));
    }

    void SpawnChargingCalcBook(GameObject book) {
        var offset = data.direction == Direction.Left ? new Vector3(-1f, .5f) : new Vector3(1f, .5f);
        chargingCalcBook = Instantiate(book, gameObject.transform.position + offset, Quaternion.identity);
        chargingCalcBook.transform.parent = gameObject.transform;
        var behavior = chargingCalcBook.GetComponent<CalcBook>();
        behavior.spawnedBy = gameObject;
        behavior.finishAttack = () => Destroy(chargingCalcBook);
    }

    protected override void BeginChargingAttack2() {
        calcBookPower = 0;
        SpawnChargingCalcBook(calc1);
    }

    protected override void KeepChargingAttack2(float chargeTime) {
        if (chargeTime > .7f && calcBookPower == 0) {
            ++calcBookPower;
            Destroy(chargingCalcBook);
            SpawnChargingCalcBook(calc2);
        } else if (chargeTime > 1.4f && calcBookPower == 1) {
            ++calcBookPower;
            Destroy(chargingCalcBook);
            SpawnChargingCalcBook(calc3);
        }
    }

    protected override void EndChargingAttack2(float chargeTime) {
        Destroy(chargingCalcBook);
    }
}