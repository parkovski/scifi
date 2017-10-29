using System;

namespace SciFi.Util {
    public class Lazy<T> {
        T obj;
        bool isLoaded;
        Func<T> get;

        public Lazy(Func<T> getter) {
            get = getter;
        }

        public T Value {
            get {
                if (!isLoaded) {
                    obj = get();
                    isLoaded = true;
                }
                return obj;
            }
        }
    }

    public static class Lazy {
        public static Lazy<T> From<T>(Func<T> getter) {
            return new Lazy<T>(getter);
        }
    }
}