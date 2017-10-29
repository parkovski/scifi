using System.Collections;
using System.Collections.Generic;

namespace SciFi.Util {
    /// A list where the item is added if you call IndexOf.
    /// Used to fake a spawn prefab list in single player mode.
    public class JitList<T> : IList<T> {
        List<T> backingList;

        public JitList() {
            backingList = new List<T>();
        }

        public bool Contains(T item) {
            return backingList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            backingList.CopyTo(array, arrayIndex);
        }

        public void Clear() {
            backingList.Clear();
        }

        public int IndexOf(T item) {
            int backingListIndex = backingList.IndexOf(item);
            if (backingListIndex != -1) {
                return backingListIndex;
            }

            backingList.Add(item);
            return backingList.Count - 1;
        }

        public void Insert(int index, T item) {
            backingList.Insert(index, item);
        }

        public void RemoveAt(int index) {
            backingList.RemoveAt(index);
        }

        public T this[int index] {
            get {
                return backingList[index];
            }
            set {
                backingList[index] = value;
            }
        }

        public void Add(T item) {
            backingList.Add(item);
        }

        public bool Remove(T item) {
            return backingList.Remove(item);
        }

        public int Count {
            get {
                return backingList.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return backingList.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return backingList.GetEnumerator();
        }
    }
}