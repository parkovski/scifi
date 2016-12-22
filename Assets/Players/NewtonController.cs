using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class NewtonController : Player {
    Rigidbody2D rb;
    bool canJump = false;
    bool canDoubleJump = false;
    int groundCollisions = 0;
    float cooldownOver = 0f;
    public GameObject apple;
    InputManager inputManager;
    DebugPrinter debug;
    int debugVelocityField;

    // Physics parameters
    const float maxSpeed = 6.5f;
    const float walkForce = 1500f;
    const float jumpForce = 600f;
    const float minDoubleJumpVelocity = 2f;
    const float appleHorizontalForce = 50f;
    const float appleVerticalForce = 20f;
    // Torque is random from (-appleTorqueRange, appleTorqueRange).
    const float appleTorqueRange = 5f;
    const float attackCooldown = .5f;

    void Start () {
        rb = GetComponent<Rigidbody2D>();
        var gameControllerGo = GameObject.Find("GameController");
        // TODO: Remove when objects are instantiated via GameController
        inputManager = gameControllerGo.GetComponent<InputManager>();
        debug = gameControllerGo.GetComponent<DebugPrinter>();
        debugVelocityField = debug.NewField();

        GameController.Instance.RegisterNewPlayer(gameObject);

        if (!Input.touchSupported) {
            Destroy(GameObject.Find("left-button"));
            Destroy(GameObject.Find("right-button"));
            Destroy(GameObject.Find("fire-button"));
        }
    }

    public override void OnStartLocalPlayer() {
        //GetComponent<SpriteRenderer>().color = new Color(.8f, .9f, 1f, .8f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            ++groundCollisions;
            canJump = true;
            canDoubleJump = false;
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            if (--groundCollisions == 0) {
                canJump = false;
                canDoubleJump = true;
            }
        }
    }

    void Update () {
    }

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        if (inputManager.IsControlActive(Control.Left)) {
            if (rb.velocity.x > -maxSpeed) {
                rb.AddForce(transform.right * -walkForce);
            }
        }
        if (inputManager.IsControlActive(Control.Right)) {
            if (rb.velocity.x < maxSpeed) {
                rb.AddForce(transform.right * walkForce);
            }
        }
        debug.SetField(debugVelocityField, string.Format("Vel: ({0}, {1})", rb.velocity.x, rb.velocity.y));
        if (inputManager.IsControlActive(Control.Up)) {
            inputManager.InvalidateControl(Control.Up);
            if (canJump) {
                canJump = false;
                canDoubleJump = true;
                rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            } else if (canDoubleJump) {
                canDoubleJump = false;
                if (rb.velocity.y < minDoubleJumpVelocity) {
                    rb.velocity = new Vector2(rb.velocity.x, minDoubleJumpVelocity);
                }
                rb.AddForce(transform.up * jumpForce / 2, ForceMode2D.Impulse);
            }
        }

        if (inputManager.IsControlActive(Control.Attack)) {
            inputManager.InvalidateControl(Control.Attack);
            if (Time.time > cooldownOver) {
                cooldownOver = Time.time + attackCooldown;
                CmdSpawnApple(netId);
            }
        }
    }

    [Command]
    void CmdSpawnApple(NetworkInstanceId netId) {
        var newApple = Instantiate(apple, gameObject.transform.position, Quaternion.identity);
        newApple.GetComponent<AppleBehavior>().spawnedBy = netId;
        var appleRb = newApple.GetComponent<Rigidbody2D>();
        appleRb.AddForce(transform.right * appleHorizontalForce);
        appleRb.AddForce(transform.up * appleVerticalForce);
        appleRb.AddTorque(Random.Range(-appleTorqueRange, appleTorqueRange));
        NetworkServer.Spawn(newApple);
    }
}