using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Collections;

using SciFi.Players;
using SciFi.Players.Attacks;
using SciFi.Util.Extensions;

namespace SciFi.Items {
    public class Bow : Item {
        int cArrows = 5;
        bool eFlipArrow = false;
        int lPower = 1;

        public GameObject normalArrow;
        public GameObject fireArrow;
        public GameObject rockArrow;
        public GameObject bombArrow;

        /// This array contains duplicates, because it is weighted
        /// to give more powerful arrows less often.
        GameObject[] eArrowsArray;
        /// The type of arrow chosen for this bow
        [SyncVar]
        int eArrowPrefabIndex;
        /// The arrow shown with this bow. When the bow is spawned,
        /// an arrow is created. When it is picked up, the arrow
        /// disappears until the player starts to shoot.
        GameObject lDisplayArrow;
        readonly Vector3 arrowOffset = new Vector3(.13f, 0f);
        readonly Vector3 flippedArrowOffset = new Vector3(-.13f, 0f);

        AudioSource audioSource;

        void Start() {
            BaseStart();
            InitArrowsArray();
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnStartServer() {
            InitArrowsArray();
        }

        public override void OnStartClient() {
            CreateDisplayArrow();
        }

        void InitArrowsArray() {
            if (eArrowsArray == null) {
                eArrowsArray = new [] {
                    normalArrow, normalArrow, normalArrow, normalArrow,
                    rockArrow, rockArrow,
                    bombArrow,
                    fireArrow,
                };

                if (isServer) {
                    eArrowPrefabIndex = GameController.PrefabToIndex(eArrowsArray[Random.Range(0, eArrowsArray.Length)]);
                }
            }
        }

        [ClientRpc]
        void RpcCreateDisplayArrow() {
            CreateDisplayArrow();
        }

        void CreateDisplayArrow() {
            var prefab = GameController.IndexToPrefab(eArrowPrefabIndex);
            var offset = eFlipArrow ? flippedArrowOffset : arrowOffset;
            lPower = 0;
            var angle = GetArrowAngle(GetArrowForce());
            lDisplayArrow = Instantiate(prefab, gameObject.transform.position + offset, Quaternion.Euler(0f, 0f, angle));
            Destroy(lDisplayArrow.GetComponent<Arrow>());
            lDisplayArrow.GetComponent<SpriteRenderer>().flipX = eFlipArrow;
            lDisplayArrow.layer = Layers.displayOnly;
            lDisplayArrow.GetComponent<Rigidbody2D>().isKinematic = true;
            lDisplayArrow.transform.parent = gameObject.transform;
        }

        [ClientRpc]
        void RpcDestroyDisplayArrow() {
            Destroy(lDisplayArrow);
        }

        void Update() {
            BaseUpdate();
        }

        protected override void OnBlink(float alpha) {
            if (lDisplayArrow == null) {
                return;
            }
            var sr = lDisplayArrow.GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        protected override void OnPickup() {
            if (isServer) {
                //Destroy(lDisplayArrow);
                //RpcDestroyDisplayArrow();
            }
        }
        protected override void OnDiscard() {
            if (isServer) {
                //RpcCreateDisplayArrow();
            }
        }

        protected override void OnChangeDirection(Direction direction) {
            GetComponent<SpriteRenderer>().flipX = eFlipArrow = direction == Direction.Left;
            if (lDisplayArrow != null) {
                lDisplayArrow.GetComponent<SpriteRenderer>().flipX = eFlipArrow;
                lDisplayArrow.transform.localPosition = eFlipArrow ? flippedArrowOffset : arrowOffset;
                lDisplayArrow.transform.rotation = Quaternion.Euler(0f, 0f, GetArrowAngle(GetArrowForce()));
            }
        }

        public override bool ShouldThrow() {
            return cArrows == 0;
        }

        public override bool ShouldCharge() {
            return cArrows > 0 && lDisplayArrow != null;
        }

        protected override void OnKeepCharging(float chargeTime) {
            float xOffset = 0;
            chargeTime = Mathf.Clamp(chargeTime, 0f, 2f);
            if (eDirection == Direction.Left) {
                xOffset = chargeTime / 10 + flippedArrowOffset.x;
            } else {
                xOffset = -chargeTime / 10 + arrowOffset.x;
            }
            lPower = (int)(chargeTime * 5);
            lDisplayArrow.transform.localPosition = new Vector3(xOffset, 0, 0);
            lDisplayArrow.transform.rotation = Quaternion.Euler(0f, 0f, GetArrowAngle(GetArrowForce()));
        }

        protected override void OnEndCharging(float chargeTime) {
            lDisplayArrow.transform.localPosition = eDirection == Direction.Left ? flippedArrowOffset : arrowOffset;

            --cArrows;
            if (cArrows == 0) {
                Destroy(lDisplayArrow);
                lDisplayArrow = null;
            } else {
                StartCoroutine(TemporarilyDestroyDisplayArrow());
            }

            audioSource.Play();
            eOwner.CmdSpawnProjectileFlipped(
                eArrowPrefabIndex,
                gameObject.transform.position,
                Quaternion.identity,
                GetArrowForce(),
                0f,
                eFlipArrow
            );
        }

        Vector2 GetArrowForce() {
            return new Vector2(250f + lPower * 25, 50f + lPower * 5f).FlipDirection(eDirection);
        }

        float GetArrowAngle(Vector2 force) {
            if (force.x < 0) {
                return -Mathf.Atan2(force.y, -force.x) * Mathf.Rad2Deg;
            } else {
                return Mathf.Atan2(force.y, force.x) * Mathf.Rad2Deg;
            }
        }

        IEnumerator TemporarilyDestroyDisplayArrow() {
            Destroy(lDisplayArrow);
            lDisplayArrow = null;
            yield return new WaitForSeconds(.5f);
            CreateDisplayArrow();
        }

        public override AttackType Type { get { return AttackType.Projectile; } }
    }
}