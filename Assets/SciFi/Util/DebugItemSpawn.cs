using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System;
using System.Linq;

using SciFi.Network;

namespace SciFi.Util {
    /// When running in the editor, you can select from a list of
    /// items to spawn for testing.
    public class DebugItemSpawn : MonoBehaviour {
        /// The list of items, generated in <see cref="Start()" />.
        public Dropdown dropdown;
        /// Spawn the currently selected item.
        public Button spawnButton;
        public bool showItemSpawn;

        void Start() {
#if UNITY_EDITOR
            if (!showItemSpawn) {
                Destroy(dropdown.gameObject);
                Destroy(spawnButton.gameObject);
                return;
            }

            // The dropdown is unusable without this,
            // but it puts the UI behind attacks and players.
            GetComponent<Canvas>().sortingLayerID = SortingLayer.NameToID("Default");
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
                EventSystem.current.SetSelectedGameObject(null);
            });
#else
            Destroy(dropdown.gameObject);
            Destroy(spawnButton.gameObject);
#endif
        }

        void SpawnItem(string name) {
            var prefab = NetworkController.singleton.spawnPrefabs.Single(p => p.name == name);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(obj);
        }
    }
}