using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Environment.Effects {
    /// Visual effects that are self-managing.
    public static class Effects {
        public static void Star(Vector3 position) {
            var star = Object.Instantiate(EffectsEditorParams.Instance.star, position, Quaternion.identity);
            NetworkServer.Spawn(star);
        }

        public static void Explosion(Vector3 position) {
            var explosion = Object.Instantiate(EffectsEditorParams.Instance.explosion, position, Quaternion.identity);
            NetworkServer.Spawn(explosion);
        }
    }
}