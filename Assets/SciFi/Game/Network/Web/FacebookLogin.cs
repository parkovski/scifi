#if ENABLE_FACEBOOK

using UnityEngine;
using System.Collections.Generic;

using Facebook.Unity;

namespace SciFi.Network.Web {
    public class FacebookLogin : CustomYieldInstruction {
        bool done = false;
        public override bool keepWaiting { get { return !done; } }
        public ILoginResult loginResult { get; private set; }
        public ulong fbid { get; private set; }

        public static FacebookLogin globalLogin { get; private set; }

        public FacebookLogin(IEnumerable<string> permissions) {
            FB.Init(() => {
                FB.LogInWithReadPermissions(permissions, result => {
                    if (result == null) {
                        done = true;
                        return;
                    }
                    loginResult = result;
                    if (result.AccessToken == null) {
                        done = true;
                        return;
                    }
                    globalLogin = this;
                    ulong _fbid;
                    ulong.TryParse(result.AccessToken.UserId, out _fbid);
                    fbid = _fbid;
                    done = true;
                });
            });
        }
    }
}

#endif