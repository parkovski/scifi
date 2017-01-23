using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

using SciFi.Items;
using SciFi.Players;

namespace SciFi.Environment {
    /// This script should be applied to an object with
    /// a regular collider and a larger trigger collider.
    /// It enables objects to pass through the collider from
    /// below, and for players, from above when holding the
    /// down button.
    public class OneWayPlatform : NetworkBehaviour {
        Collider2D lGroundCollider;
        // Since objects might have multiple colliders,
        // when an object enters the trigger, we increase
        // the collider count, and only stop ignoring
        // collisions when the count reaches 0.
        Dictionary<GameObject, int> lColliderCount;

        int GetColliderCount(GameObject obj) {
            int count = 0;
            lColliderCount.TryGetValue(obj, out count);
            return count;
        }

        void SetColliderCount(GameObject obj, int count) {
            lColliderCount[obj] = count;
        }

        void Start() {
            var colliders = GetComponents<Collider2D>();
            if (colliders.Length != 2 || colliders.Count(c => c.isTrigger) != 1) {
                throw new InvalidOperationException("OneWayPlatform is only valid " +
                    "on objects with one trigger Collider2D and one non-trigger Collider2D");
            }
            lGroundCollider = colliders.First(c => !c.isTrigger);

            lColliderCount = new Dictionary<GameObject, int>();
        }

        void IgnoreCollisions(GameObject obj) {
            var player = obj.GetComponent<Player>();
            var shouldFall = false;
            if (player != null) {
                shouldFall = player.eShouldFallThroughOneWayPlatform;
            }
            bool isLeftOfBox = obj.transform.position.x < lGroundCollider.bounds.center.x - lGroundCollider.bounds.extents.x;
            bool isRightOfBox = obj.transform.position.x > lGroundCollider.bounds.center.x + lGroundCollider.bounds.extents.x;
            bool isBelowBox = obj.transform.position.y < lGroundCollider.bounds.center.y;
            if (isLeftOfBox || isRightOfBox || isBelowBox || shouldFall) {
                Item.IgnoreCollisions(obj, lGroundCollider);
            }
        }

        void OnTriggerEnter2D(Collider2D otherCollider) {
            var go = otherCollider.gameObject;
            SetColliderCount(go, GetColliderCount(go) + 1);
            IgnoreCollisions(go);
        }

        void OnTriggerExit2D(Collider2D otherCollider) {
            var go = otherCollider.gameObject;
            var count = GetColliderCount(go);
            if (count <= 1) {
                SetColliderCount(go, 0);
                Item.IgnoreCollisions(go, lGroundCollider, false);
            } else {
                SetColliderCount(go, count - 1);
            }
        }

        /// This should be called when the player presses down
        /// when standing on the platform to force a fall through.
        /// If down is pressed when the player enters the trigger
        /// area, fall through will happen automatically without
        /// calling this.
        [Command]
        public void CmdFallThrough(GameObject go) {
            Item.IgnoreCollisions(go, lGroundCollider);
            RpcFallThrough(go);
        }

        [ClientRpc]
        void RpcFallThrough(GameObject go) {
            Item.IgnoreCollisions(go, lGroundCollider);
        }
    }
}