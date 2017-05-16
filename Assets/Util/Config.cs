using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SciFi.Util {
    public static class Config {
        private static Dictionary<string, string> dictionary;

        public static void Initialize() {
            Initialize(Application.streamingAssetsPath + "/config.txt");
        }

        public static void Initialize(string path) {
            using (var reader = new StreamReader(path)) {
                Initialize(reader);
            }
        }

        public static void Initialize(StreamReader reader) {
            dictionary = new Dictionary<string, string>();
            var kvPairs = reader
                .ReadToEnd()
                .Split('\n')
                .Where(line => !line.TrimStart().StartsWith("#"))
                .Select(line => {
                    var firstEq = line.IndexOf('=');
                    string left, right;
                    if (firstEq == -1) {
                        left = line.Trim();
                        right = "";
                    } else {
                        left = line.Substring(0, firstEq).Trim();
                        right = line.Substring(firstEq + 1).Trim();
                    }
                    return new {
                        left = left,
                        right = right,
                    };
                });
            foreach (var pair in kvPairs) {
                dictionary[pair.left] = pair.right;
            }
        }

        public static bool HasKey(string key) {
            if (dictionary == null) {
                Initialize();
            }
            return dictionary.ContainsKey(key);
        }

        public static string GetKey(string key) {
            if (dictionary == null) {
                Initialize();
            }
            string value;
            if (dictionary.TryGetValue(key, out value)) {
                return value;
            }
            return "";
        }
    }
}