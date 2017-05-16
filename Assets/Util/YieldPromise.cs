using UnityEngine;

namespace SciFi.Util {
    public class YieldPromise<T, E> : CustomYieldInstruction {
        bool done = false;
        public T data { get; private set; }
        public bool isError { get; private set; }
        public E error { get; private set; }
        public override bool keepWaiting { get { return !done; } }

        public YieldPromise() {
        }

        public void Resolve(T data) {
            done = true;
            this.data = data;
        }

        public void Reject(E error) {
            done = true;
            isError = true;
            this.error = error;
        }
    }
}