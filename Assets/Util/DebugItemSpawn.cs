using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

using SciFi.Network;

namespace SciFi.Util {
    public class DebugItemSpawn : MonoBehaviour {
        public Dropdown dropdown;
        public Button spawnButton;

        void Start() {
            dropdown.AddOptions(
                NetworkController.singleton.spawnPrefabs
                    .Where(p => p.layer != Layers.players)
                    .Select(p => p.name)
                    .ToList()
            );
            spawnButton.onClick.AddListener(() => {
                SpawnItem(dropdown.options[dropdown.value].text);
            });
        }

        void SpawnItem(string name) {
            var prefab = NetworkController.singleton.spawnPrefabs.Single(p => p.name == name);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(obj);
        }
    }
}