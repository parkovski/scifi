using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class NewtonController : NetworkBehaviour, IPlayer {
    Rigidbody2D rb;
    Collider2D[] colliders;
    bool canJump = false;
    int groundCollisions = 0;
    float cooldownOver = 0f;
    public GameObject apple;
    InputManager inputManager;

    static int playerNumber = 0;
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        // TODO: Remove when objects are instantiated via GameController
        this.GameController = GameObject.Find("GameController").GetComponent<GameController>();
        this.Id = ++playerNumber;
        inputManager = GameObject.Find("GameController").GetComponent<InputManager>();

        if (!Input.touchSupported) {
            Destroy(GameObject.Find("left-button"));
            Destroy(GameObject.Find("right-button"));
            Destroy(GameObject.Find("fire-button"));
        }

        GetComponent<PlayerProxy>().PlayerDelegate = this;
    }

    public override void OnStartLocalPlayer() {
        //GetComponent<SpriteRenderer>().color = new Color(.8f, .9f, 1f, .8f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            ++groundCollisions;
            canJump = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            if (--groundCollisions == 0) {
                canJump = false;
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
            rb.AddForce(transform.right * -10f);
        }
        if (inputManager.IsControlActive(Control.Right)) {
            rb.AddForce(transform.right * 10f);
        }
        if (canJump && inputManager.IsControlActive(Control.Up)) {
            inputManager.InvalidateControl(Control.Up);
            canJump = false;
            rb.AddForce(transform.up * 6f, ForceMode2D.Impulse);
        }

        if (inputManager.IsControlActive(Control.Attack)) {
            inputManager.InvalidateControl(Control.Attack);
            if (Time.time > cooldownOver) {
                cooldownOver = Time.time + 0.5f;

                var newApple = Instantiate(apple, gameObject.transform.position, Quaternion.identity);
                // Don't let the apple damage its creator
                var appleColliders = newApple.GetComponents<Collider2D>();
                foreach (var coll in colliders) {
                    foreach (var appleColl in appleColliders) {
                        Physics2D.IgnoreCollision(coll, appleColl);
                    }
                }
                var appleRb = newApple.GetComponent<Rigidbody2D>();
                appleRb.AddForce(transform.right * 5f);
                appleRb.AddForce(transform.up * 2f);
                appleRb.AddTorque(Random.Range(-.2f, .2f));
            }
        }
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int Lives { get; set; }
    public int Damage { get; set; }
    public GameController GameController { get; set; }
    public void TakeDamage(int amount) {
        this.Damage += amount;
        this.GameController.TakeDamage(this, amount);
    }
}
