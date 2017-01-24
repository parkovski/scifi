using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Linq;

using SciFi.Network;

namespace SciFi.Util {
    public class DebugItemSpawn : MonoBehaviour {
        public Dropdown dropdown;
        public Button spawnButton;

        void Start() {
#if UNITY_EDITOR
            try {
                dropdown.AddOptions(
                    NetworkController.singleton.spawnPrefabs
                        .Where(p => p.layer != Layers.players && p.layer != Layers.displayOnly)
                        .Select(p => p.name)
                        .ToList()
                );
            } catch (NullReferenceException) {
                // For some mysterious reason, NetworkController.singleton.spawnPrefabs
                // throws a NRE but still works. It works in other places without
                // the NRE, and still works here (seriously can't figure that out)
                // so just ignore it to silence the error message.
            }
            spawnButton.onClick.AddListener(() => {
                SpawnItem(dropdown.options[dropdown.value].text);
            });
#else
            Destroy(dropdown);
            Destroy(spawnButton);
#endif
        }

        void SpawnItem(string name) {
            var prefab = NetworkController.singleton.spawnPrefabs.Single(p => p.name == name);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(obj);
        }
    }
}