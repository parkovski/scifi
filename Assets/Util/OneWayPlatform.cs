using UnityEngine;
using System;
using System.Linq;

public class OneWayPlatform : MonoBehaviour {
    EdgeCollider2D edgeCollider;
    EdgeCollider2D trigger;

    void Start() {
        var colliders = GetComponents<EdgeCollider2D>();
        if (colliders.Length != 2 || colliders.Count(c => c.isTrigger) != 1) {
            throw new InvalidOperationException("OneWayPlatform is only valid " +
                "on objects with one trigger EdgeCollider2D and one non-trigger EdgeCollider2D");
        }
        edgeCollider = colliders.First(c => !c.isTrigger);
        trigger = colliders.First(c => c.isTrigger);
    }

    void OnTriggerEnter2D(Collider2D otherCollider) {
        Physics2D.IgnoreCollision(edgeCollider, otherCollider, true);
    }

    void OnTriggerExit2D(Collider2D otherCollider) {
        Physics2D.IgnoreCollision(edgeCollider, otherCollider, false);
    }
}