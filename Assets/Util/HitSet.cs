using UnityEngine;
using System.Collections.Generic;

namespace SciFi.Util {
    public sealed class HitSet {
        HashSet<GameObject> hitObjects;

        public HitSet() {
            hitObjects = new HashSet<GameObject>();
        }

        /// Returns true if this object was already marked as hit.
        /// After this call, the object will be marked.
        public bool CheckOrFlag(GameObject gameObject) {
            bool contains = hitObjects.Contains(gameObject);
            if (!contains) {
                hitObjects.Add(gameObject);
            }
            return contains;
        }

        public void Clear() {
            hitObjects.Clear();
        }
    }
}