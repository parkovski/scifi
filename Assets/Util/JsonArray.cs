using UnityEngine;
using System;

namespace SciFi.Util {
    /// Unity's JsonUtility doesn't support top-level arrays.
    /// There's a forum post from 2011 about this, so I doubt they'll
    /// ever fix it.
    public class JsonArray {
        [Serializable]
        struct JsonArrayWrapper<T> {
            public T[] array;
        }

        public static T[] FromJson<T>(string json) {
            var wrappedJson = "{\"array\":" + json + "}";
            var wrapper = JsonUtility.FromJson<JsonArrayWrapper<T>>(wrappedJson);
            return wrapper.array;
        }

        public static string ToJson<T>(T[] array) {
            var wrapper = new JsonArrayWrapper<T> {
                array = array,
            };
            var json = JsonUtility.ToJson(wrapper);
            // Strip off the {"array":} stuff.
            return json.Substring(9, json.Length - 9 - 1);
        }
    }
}