using UnityEngine;
using UnityEngine.Networking;

public class ParkerMove : NetworkBehaviour, IPlayer {
    Rigidbody2D rb;
    Collider2D[] colliders;
    bool movingLeft = false;
    bool movingRight = false;
    bool shouldJump = false;
    bool canJump = false;
    int groundCollisions = 0;
    int touchControlLayer;
    int? leftBtnFingerId = null;
    int? rightBtnFingerId = null;
    float cooldownOver = 0f;
    bool shouldShoot = false;
    public GameObject apple;

    static int playerNumber = 0;
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        touchControlLayer = LayerMask.NameToLayer("Touch Controls");
        // TODO: Remove when objects are instantiated via GameController
        this.GameController = GameObject.Find("GameController").GetComponent<GameController>();
        this.Id = ++playerNumber;

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

        HandleInput();

        if (movingLeft || (leftBtnFingerId != null)) {
            rb.AddForce(transform.right * -10f);
        }
        if (movingRight || (rightBtnFingerId != null)) {
            rb.AddForce(transform.right * 10f);
        }
        if (canJump && shouldJump) {
            shouldJump = false;
            canJump = false;
            rb.AddForce(transform.up * 6f, ForceMode2D.Impulse);
        }

        if (shouldShoot) {
            shouldShoot = false;
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
                appleRb.AddTorque(-.1f);
            }
        }
    }

    void HandleInput() {
        var horizontalAxis = Input.GetAxis("Horizontal");
        var verticalAxis = Input.GetAxis("Vertical");
        if (horizontalAxis < 0) {
            movingLeft = true;
            movingRight = false;
        } else if (horizontalAxis > 0) {
            movingLeft = false;
            movingRight = true;
        } else {
            movingLeft = movingRight = false;
        }

        if (verticalAxis > 0) {
            shouldJump = true;
        } else {
            shouldJump = false;
        }

        if (Input.GetButton("Fire1")) {
            shouldShoot = true;
        }

        if (Input.touchCount > 0) {
            foreach (var touch in Input.touches) {
                if (touch.phase == TouchPhase.Began) {
                    var ray = Camera.main.ScreenToWorldPoint(touch.position);
                    var hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity, 1 << touchControlLayer);
                    if (hit) {
                        var name = hit.rigidbody.gameObject.name;
                        if (name == "left-button") {
                            leftBtnFingerId = touch.fingerId;
                        } else if (name == "right-button") {
                            rightBtnFingerId = touch.fingerId;
                        } else if (name == "fire-button") {
                            shouldShoot = true;
                        }
                    }
                } else if (touch.phase == TouchPhase.Ended) {
                    if (touch.fingerId == leftBtnFingerId) {
                        leftBtnFingerId = null;
                    } else if (touch.fingerId == rightBtnFingerId) {
                        rightBtnFingerId = null;
                    }
                }
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
