using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Network;
using SciFi.Util;

namespace SciFi {
    /// Keeps track of objects to avoid running the GC.
    /// To use this, implement IPoolNotificationHandler
    /// but still handle initialization in Start - only
    /// subsequent uses will call Acquire. The object
    /// must have a PooledObject or NetworkPooledObject
    /// component, and you should assign its notification
    /// handler in the editor.
    public class GameObjectPool {
        /// Prefab index -> pool
        Dictionary<int, List<NetworkPooledObject>> netPools;
        /// Prefab -> pool
        Dictionary<GameObject, List<PooledObject>> localPools;

#if DEBUG_POOLS
        int dbgNetPool = -1;
        int dbgLocPool = -1;
#endif

        public GameObjectPool() {
            netPools = new Dictionary<int, List<NetworkPooledObject>>();
            localPools = new Dictionary<GameObject, List<PooledObject>>();
        }

        /// Get the first available object from the pool,
        /// or add one if none are free. This calls Acquire.
        /// Call Release to release the object to the pool.
        /// If the object has a NetworkPooledObject behaviour,
        /// it will be spawned.
        /// If this is called on a non-server copy for a networked
        /// object, null will be returned.
        public GameObject GetNet(int prefabIndex, Vector3 position, Quaternion rotation) {
            List<NetworkPooledObject> list;
            if (!netPools.TryGetValue(prefabIndex, out list)) {
                list = new List<NetworkPooledObject>();
                netPools.Add(prefabIndex, list);
            }

            foreach (var obj in list) {
                if (obj.IsFree()) {
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.Acquire();
                    // Networked objects not on the server do nothing
                    // for Acquire - don't return them.
                    if (!obj.IsFree()) {
                        return obj.gameObject;
                    } else {
                        return null;
                    }
                }
            }

            // A new object is considered acquired already.
            // This is to avoid duplicate initialization of generic
            // components like Projectile that need to run some
            // initialization logic in Start for non-pooled objects,
            // and would run that code twice if we called Acquire here.
            var newObj = Object.Instantiate(GameController.IndexToPrefab(prefabIndex), position, rotation);
            NetworkServer.Spawn(newObj);
            list.Add(newObj.GetComponent<NetworkPooledObject>());
#if DEBUG_POOLS
            UpdateDebug();
#endif
            return newObj;
        }

        /// Gets a non-networked (local) pooled object instance,
        /// or creates one if there is none. Acquire has already been called,
        /// except when first created, where it is not expected. Call Release
        /// to give it back to the pool.
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation) {
            List<PooledObject> list;
            if (!localPools.TryGetValue(prefab, out list)) {
                list = new List<PooledObject>();
                localPools.Add(prefab, list);
            }

            foreach (var obj in list) {
                if (obj.IsFree()) {
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.Acquire();
                    return obj.gameObject;
                }
            }

            var newObj = Object.Instantiate(prefab, position, rotation);
            list.Add(newObj.GetComponent<PooledObject>());
#if DEBUG_POOLS
            UpdateDebug();
#endif
            return newObj;
        }

#if DEBUG_POOLS
        void UpdateDebug() {
            var printer = DebugPrinter.Instance;
            if (printer == null) {
                return;
            }

            if (dbgNetPool == -1) {
                dbgNetPool = printer.NewField();
            }
            if (dbgLocPool == -1) {
                dbgLocPool = printer.NewField();
            }

            int netMax = 0, netAvg = 0;
            foreach (var pair in netPools) {
                var count = pair.Value.Count;
                if (count > netMax) {
                    netMax = count;
                }
                netAvg += count;
            }
            if (netPools.Count != 0) {
                netAvg /= netPools.Count;
            }

            int locMax = 0, locAvg = 0;
            foreach (var pair in localPools) {
                var count = pair.Value.Count;
                if (count > locMax) {
                    locMax = count;
                }
                locAvg += count;
            }
            if (localPools.Count != 0) {
                locAvg /= localPools.Count;
            }

            printer.SetField(dbgNetPool, "NetPool A:" + netAvg + " M:" + netMax);
            printer.SetField(dbgLocPool, "LocPool A:" + locAvg + " M:" + locMax);
        }
#endif
    }
}