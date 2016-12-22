using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class OneWayPlatform : MonoBehaviour {
    EdgeCollider2D edgeCollider;
    // Since objects might have multiple colliders,
    // when an object enters the trigger, we increase
    // the collider count, and only stop ignoring
    // collisions when the count reaches 0.
    Dictionary<GameObject, int> colliderCount;

    void Start() {
        var colliders = GetComponents<EdgeCollider2D>();
        if (colliders.Length != 2 || colliders.Count(c => c.isTrigger) != 1) {
            throw new InvalidOperationException("OneWayPlatform is only valid " +
                "on objects with one trigger EdgeCollider2D and one non-trigger EdgeCollider2D");
        }
        edgeCollider = colliders.First(c => !c.isTrigger);

        colliderCount = new Dictionary<GameObject, int>();
    }

    void OnTriggerEnter2D(Collider2D otherCollider) {
        var go = otherCollider.gameObject;
        int count;
        if (colliderCount.TryGetValue(go, out count)) {
            colliderCount[go] = count + 1;
            if (count > 0) {
                return;
            }
        } else {
            colliderCount[go] = 1;
        }
        foreach (var coll in go.GetComponents<Collider2D>()) {
            Physics2D.IgnoreCollision(edgeCollider, coll, true);
        }
    }

    void OnTriggerExit2D(Collider2D otherCollider) {
        var go = otherCollider.gameObject;
        int count;
        if (!colliderCount.TryGetValue(go, out count)) {
            return;
        }
        if (count <= 1) {
            colliderCount[go] = 0;
            foreach (var coll in go.GetComponents<Collider2D>()) {
                Physics2D.IgnoreCollision(edgeCollider, coll, false);
            }
        } else {
            colliderCount[go] = count - 1;
        }
    }
}