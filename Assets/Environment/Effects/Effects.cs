using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Environment {
    public static class Effects {
        public static void Star(Vector3 position) {
            var star = Object.Instantiate(EffectsEditorParams.Instance.star, position, Quaternion.identity);
            NetworkServer.Spawn(star);
        }
    }
}