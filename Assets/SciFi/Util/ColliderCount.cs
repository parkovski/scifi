using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SciFi.Util {
    public class ColliderCount {
        Dictionary<GameObject, uint> counts;

        public ColliderCount() {
            counts = new Dictionary<GameObject, uint>();
        }

        public void Increase(Collider2D collider) {
            uint count;
            if (counts.TryGetValue(collider.gameObject, out count)) {
                counts[collider.gameObject] = count + 1;
            } else {
                counts[collider.gameObject] = 1;
            }
        }

        public void Decrease(Collider2D collider) {
            uint count;
            if (counts.TryGetValue(collider.gameObject, out count)) {
                if (count > 0) {
                    counts[collider.gameObject] = count - 1;
                }
            }
        }

        public void Clear() {
            counts.Clear();
        }

        public uint GetCount(Collider2D collider) {
            uint count;
            if (counts.TryGetValue(collider.gameObject, out count)) {
                return count;
            }
            return 0;
        }

        /// Filters out objects with a zero count and null (destroyed) objects.
        public IEnumerable<GameObject> ObjectsWithPositiveCount {
            get {
                return counts.Where(pair => pair.Value > 0 && pair.Key != null).Select(pair => pair.Key);
            }
        }
    }
}