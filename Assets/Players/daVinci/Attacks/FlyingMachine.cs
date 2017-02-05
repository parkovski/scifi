using UnityEngine;
using UnityEngine.Networking;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class FlyingMachine : Projectile {
        public Sprite unmovingProp;
        public Sprite[] movingProps;
        public GameObject projectileItemContainerPrefab;

        // Flight path
        [HideInInspector]
        public float dx;
        [HideInInspector]
        public float y;
        /// 1-10
        [HideInInspector]
        public int power;

        // Trapped player
        float holdTime = 1f;
        float initialHoldStateTime;
        Vector3 heldPlayerOffset;
        Player heldPlayer;

        Rigidbody2D rb;

        // Propeller animation
        float initialTime;
        SpriteRenderer spriteRenderer;
        const float movingPropSpriteTime = .03f;
        float changePropSpriteTime;
        int propIndex = 0;

        enum State {
            /// The machine flies in a half-parabola shape
            /// and grabs a player if it collides with one.
            Flying,
            /// The machine passed by a player and is now
            /// moving toward the player's center.
            FindingTarget,
            /// The machine is carrying a player - the player's
            /// movement is disabled, and the machine is slowing down.
            CarryingPlayer,
            /// The machine is holding a player to allow da Vinci to
            /// attack them.
            HoldingPlayer,
            /// The machine dropped the player and will now fly away.
            Finished,
            /// Someone attacked the machine while it was in the flying
            /// state - now it just becomes a regular projectile that
            /// anyone can pick up and throw.
            Broken,
        }

        State state = State.Flying;

        void Start() {
            BaseStart();
            y = transform.position.y;
            initialTime = Time.time;
            holdTime = .5f + power * 1.5f / 10f;
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = transform.Find("FlyingMachine_Prop").GetComponent<SpriteRenderer>();
            changePropSpriteTime = Time.time + .3f;
            //Destroy(gameObject, 5f);
        }

        void FixedUpdate() {
            switch (state) {
            case State.Flying:
                UpdateFlying();
                break;
            case State.FindingTarget:
                UpdateFindingTarget();
                break;
            case State.CarryingPlayer:
                UpdateCarryingPlayer();
                break;
            case State.HoldingPlayer:
                UpdateHoldingPlayer();
                break;
            case State.Finished:
                UpdateFinished();
                break;
            case State.Broken:
                UpdateBroken();
                break;
            default:
                throw new System.ArgumentOutOfRangeException("state");
            }
        }

        void AnimateProp() {
            if (Time.time > changePropSpriteTime) {
                changePropSpriteTime = Time.time + movingPropSpriteTime;
                spriteRenderer.sprite = movingProps[propIndex];
                if (++propIndex >= movingProps.Length) {
                    propIndex = 0;
                }
            }
        }

        void UpdateFlying() {
            /// For x=t and y=(t-.5)^4, set velocity to their derivatives.
            float t = (Time.time - initialTime) * 4;
            rb.velocity = new Vector2(t*dx, Mathf.Pow((t - 1.25f) * .8f, 3) / 3f);

            AnimateProp();
        }

        void UpdateFindingTarget() {
            // We want to be reasonably close to the center of the player
            // on the x axis, and above the center in the y axis.
            var xOffset = transform.position.x - heldPlayer.transform.position.x;
            var yOffset = transform.position.y - heldPlayer.transform.position.y;
            var newXVelocity = 0f;
            var newYVelocity = 0f;
            bool xOk = false;
            bool yOk = false;
            if (yOffset < .5f) {
                if (rb.velocity.y < 2f) {
                    newYVelocity = rb.velocity.y + 1f;
                } else if (rb.velocity.y > 0f) {
                    newYVelocity = rb.velocity.y / 2f;
                } else {
                    newYVelocity = 1f;
                }
            } else if (yOffset > .9f) {
                if (rb.velocity.y > .1f) {
                    newYVelocity = rb.velocity.y / 1.5f;
                } else {
                    newYVelocity = rb.velocity.y - 0.5f;
                }
            } else {
                newYVelocity = Mathf.Max(0f, rb.velocity.y);
                yOk = true;
            }

            if (Mathf.Abs(xOffset) < 0.2f) {
                xOk = true;
                newXVelocity = rb.velocity.x / 1.2f;
            } else {
                newXVelocity = rb.velocity.x - xOffset / 1.5f;
            }

            if (xOk && yOk) {
                state = State.CarryingPlayer;
                heldPlayerOffset = transform.position - heldPlayer.transform.position;
            }
            rb.velocity = new Vector2(newXVelocity, newYVelocity);

            AnimateProp();
        }

        void UpdateCarryingPlayer() {
            var newXVelocity = rb.velocity.x / 1.5f;
            if (rb.velocity.y < 1f) {
                rb.velocity = new Vector2(newXVelocity, 1f);
                initialHoldStateTime = Time.time;
                state = State.HoldingPlayer;
                rb.isKinematic = true;
            } else if (rb.velocity.y > 2f) {
                rb.velocity = new Vector2(newXVelocity, rb.velocity.y / 2.5f);
            } else {
                rb.velocity = new Vector2(newXVelocity, rb.velocity.y);
                heldPlayer.transform.position = transform.position - heldPlayerOffset;
            }

            AnimateProp();
        }

        void UpdateHoldingPlayer() {
            heldPlayer.transform.position = transform.position - heldPlayerOffset;
            heldPlayer.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            rb.velocity = new Vector2(rb.velocity.x / 1.1f, 1f);
            if (Time.time > initialHoldStateTime + holdTime) {
                initialTime = Time.time;
                state = State.Finished;
                rb.isKinematic = false;
            }

            AnimateProp();
        }

        void UpdateFinished() {
            UpdateFlying();
        }

        void UpdateBroken() {

        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (state == State.Flying) {
                CollideFlying(collider);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (state != State.Broken) {
                return;
            }

            CollideBroken(collision);
        }

        void CollideFlying(Collider2D collider) {
            if (collider.gameObject.layer == Layers.players) {
                heldPlayer = collider.gameObject.GetComponent<Player>();
                state = State.FindingTarget;
            }
        }

        void CollideBroken(Collision2D collision) {
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                GameController.Instance.Hit(collision.gameObject, this, gameObject, 10, 6f);
                Effects.Star(transform.position);
                Destroy(transform.parent.gameObject);
            }
        }

        /// When the machine is hit while in the initial flying state,
        /// it becomes broken where it can be used as a throwable item.
        public override void Interact(IAttack attack) {
            if (state == State.Flying) {
                state = State.Broken;
                spriteRenderer.sprite = unmovingProp;
                GetComponent<Collider2D>().isTrigger = false;
                if (isServer) {
                    var container = Instantiate(projectileItemContainerPrefab, transform.position, Quaternion.identity);
                    container.GetComponent<ProjectileItemContainer>().projectile = gameObject;
                    NetworkServer.Spawn(container);
                }
            }
        }
    }
}