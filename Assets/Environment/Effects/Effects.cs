using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

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

        public static void Smoke(Vector3 position) {
            var smoke = Object.Instantiate(EffectsEditorParams.Instance.smoke, position, Quaternion.identity);
            NetworkServer.Spawn(smoke);
        }

        public static void FadeIn() {
            var fade = Object.Instantiate(EffectsEditorParams.Instance.fadeOverlay, Vector3.zero, Quaternion.identity);
            fade.GetComponent<Animator>().SetTrigger("FadeIn");
        }

        public static void FadeOut() {
            var fade = Object.Instantiate(EffectsEditorParams.Instance.fadeOverlay, Vector3.zero, Quaternion.identity);
            fade.GetComponent<Animator>().SetTrigger("FadeOut");
        }

        private static IEnumerator FadeAudioCoroutine(
            AudioSource audioSource,
            float time,
            float from,
            float to,
            int steps,
            float waitAmount
        ) {
            float deltaVolume = (to - from) / steps;
            for (int i = 0; i < steps; i++) {
                audioSource.volume += deltaVolume;
                yield return new WaitForSeconds(waitAmount);
            }
        }

        public static void FadeOutAudio(
            AudioSource audioSource,
            float time,
            int steps
        ) {
            EffectsEditorParams.RunCoroutine(FadeAudioCoroutine(audioSource, time, 1f, 0f, steps, time / steps));
        }

        public static void FadeInAudio(
            AudioSource audioSource,
            float time,
            int steps
        ) {
            EffectsEditorParams.RunCoroutine(FadeAudioCoroutine(audioSource, time, 0f, 1f, steps, time / steps));
        }
    }
}