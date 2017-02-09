using UnityEngine.Networking;

namespace SciFi.Network {
    public class SFNetworkTransform : NetworkTransform {
        public override int GetNetworkChannel() {
            return 2;
        }
    }
}