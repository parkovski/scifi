using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using SciFi.Network.Web;

namespace SciFi.Scenes {
    public class Connect : MonoBehaviour {
        public Text facebookLoginText;
        public Text otherLoginText;
        public Text title;

        void Start() {
            otherLoginText.text =
#if UNITY_ANDROID
                "Google Play Games";
#elif UNITY_IOS
                "GameCenter"
#else
                "Um..."
#endif
            ;
        }

        public void FacebookLoginClicked() {
            StartCoroutine(DoFacebookLogin());
        }

        IEnumerator DoFacebookLogin() {
#if ENABLE_FACEBOOK
            yield return new FacebookLogin(new [] { "public_profile" });
            if (FacebookLogin.globalLogin == null || FacebookLogin.globalLogin.loginResult == null) {
                print("No response from Facebook");
                facebookLoginText.text = "Error";
            } else if (!string.IsNullOrEmpty(FacebookLogin.globalLogin.loginResult.Error)) {
                print(FacebookLogin.globalLogin.loginResult.Error);
                facebookLoginText.text = "Error";
            } else {
                facebookLoginText.text = string.Format("Log out", FacebookLogin.globalLogin.fbid);
            }
#else
            print("Facebook is not enabled");
            yield break;
#endif
        }

        public void OtherLoginClicked() {
        }
    }
}