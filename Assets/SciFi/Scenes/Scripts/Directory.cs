using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    public class Directory : MonoBehaviour {
        public void Back() {
            SceneManager.LoadScene("TitleScreen");
        }

        public void LevelEditor() {
            SceneManager.LoadScene("LevelEditor");
        }

        public void About() {
            //SceneManager.LoadScene("About");
        }
    }
}