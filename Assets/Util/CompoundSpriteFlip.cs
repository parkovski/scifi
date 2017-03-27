using UnityEngine;

using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.Util {
    public class CompoundSpriteFlip {
        Direction currentDirection;
        Transform rootTransform;

        public CompoundSpriteFlip(GameObject go, Direction direction) {
            currentDirection = direction;
            rootTransform = go.transform;
        }

        public void Flip(Direction direction) {
            if (direction == currentDirection) {
                return;
            }
            currentDirection = direction;
            int index = 0;
            FlipTransform(rootTransform, ref index);
        }

        void FlipTransform(Transform transform, ref int index) {
            var sr = transform.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.flipX = !sr.flipX;
                if (sr.sprite.pivot.x != 0 || sr.sprite.pivot.y != 0) {
                    var colliders = transform.GetComponents<Collider2D>();
                    foreach (var collider in colliders) {
                        collider.offset = collider.offset.FlipX();
                    }
                }
            }
            // Don't change the root transform's position.
            if (index != 0) {
                transform.localPosition = transform.localPosition.FlipX();
            }
            ++index;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++) {
                FlipTransform(transform.GetChild(i), ref index);
            }
        }
    }
}