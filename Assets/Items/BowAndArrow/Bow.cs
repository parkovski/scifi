using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Collections;

using SciFi.Players;

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

        void Start() {
            BaseStart(true, aliveTime: 10f);
            InitArrowsArray();
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
            var angle = GetArrowAngle(GetArrowForce(eFlipArrow ? Direction.Left : Direction.Right));
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

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        public override void OnPickup() {
            if (isServer) {
                //Destroy(lDisplayArrow);
                //RpcDestroyDisplayArrow();
            }
        }
        public override void OnDiscard() {
            if (isServer) {
                //RpcCreateDisplayArrow();
            }
        }

        public override void ChangeDirection(Direction direction) {
            GetComponent<SpriteRenderer>().flipX = eFlipArrow = direction == Direction.Left;
            if (lDisplayArrow != null) {
                lDisplayArrow.GetComponent<SpriteRenderer>().flipX = eFlipArrow;
                lDisplayArrow.transform.localPosition = eFlipArrow ? flippedArrowOffset : arrowOffset;
                lDisplayArrow.transform.rotation = Quaternion.Euler(0f, 0f, GetArrowAngle(GetArrowForce(direction)));
            }
        }

        public override bool ShouldThrow() {
            return cArrows == 0;
        }

        public override bool ShouldCharge() {
            return cArrows > 0 && lDisplayArrow != null;
        }

        public override void BeginCharging(Direction direction) {
            base.BeginCharging(direction);
        }

        public override void KeepCharging(float chargeTime, Direction direction) {
            float xOffset = 0;
            chargeTime = Mathf.Clamp(chargeTime, 0f, 2f);
            if (direction == Direction.Left) {
                xOffset = chargeTime / 10 + flippedArrowOffset.x;
            } else {
                xOffset = -chargeTime / 10 + arrowOffset.x;
            }
            lPower = (int)(chargeTime * 5);
            lDisplayArrow.transform.localPosition = new Vector3(xOffset, 0, 0);
            lDisplayArrow.transform.rotation = Quaternion.Euler(0f, 0f, GetArrowAngle(GetArrowForce(direction)));
        }

        public override void EndCharging(float chargeTime, Direction direction) {
            base.EndCharging(chargeTime, direction);

            lDisplayArrow.transform.localPosition = direction == Direction.Left ? flippedArrowOffset : arrowOffset;

            --cArrows;
            if (cArrows == 0) {
                Destroy(lDisplayArrow);
            } else {
                StartCoroutine(TemporarilyDestroyDisplayArrow());
            }

            var force = GetArrowForce(direction);

            var arrow = Instantiate(GameController.IndexToPrefab(eArrowPrefabIndex), gameObject.transform.position, Quaternion.identity);
            if (eFlipArrow) {
                arrow.GetComponent<SpriteRenderer>().flipX = true;
            }
            eOwner.CmdSpawnCustomProjectile(arrow, force, 0f);
        }

        Vector2 GetArrowForce(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector2(-250f - lPower * 25, 50f + lPower * 5f);
            } else {
                return new Vector2(250f + lPower * 25, 50f + lPower * 5f);
            }
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
    }
}