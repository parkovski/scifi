using UnityEngine.Networking;

namespace SciFi.Network {
    [NetworkSettings(channel = 2)]
    public class SFNetworkTransform : NetworkTransform {
        public override int GetNetworkChannel() {
            return 2;
        }
    }
}