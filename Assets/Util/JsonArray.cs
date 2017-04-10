using UnityEngine;
using System;

namespace SciFi.Util {
    [Serializable]
    struct JsonArrayWrapper<T> {
        public T[] array;
    }

    /// Unity's JsonUtility doesn't support top-level arrays.
    /// There's a forum post from 2011 about this, so I doubt they'll
    /// ever fix it.
    public class JsonArray {
        public static T[] From<T>(string json) {
            var wrappedJson = "{\"array\":" + json + "}";
            var wrapper = JsonUtility.FromJson<JsonArrayWrapper<T>>(wrappedJson);
            return wrapper.array;
        }
    }
}