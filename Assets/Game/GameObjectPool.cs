using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Network;

namespace SciFi {
    /// Keeps track of objects to avoid running the GC.
    /// To use this, implement IPoolNotificationHandler
    /// but still handle initialization in Start - only
    /// subsequent uses will call Acquire. The object
    /// must have a PooledObject or NetworkPooledObject
    /// component, and you should assign its notification
    /// handler in the editor.
    public class GameObjectPool {
        Dictionary<int, List<IPooledObject>> pools;

        public GameObjectPool() {
            pools = new Dictionary<int, List<IPooledObject>>();
        }

        /// Get the first available object from the pool,
        /// or add one if none are free. This calls Acquire.
        /// Call Release to release the object to the pool.
        /// If the object has a NetworkPooledObject behaviour,
        /// it will be spawned.
        /// If this is called on a non-server copy for a networked
        /// object, null will be returned.
        public GameObject Get(int prefabIndex, Vector3 position, Quaternion rotation) {
            List<IPooledObject> list;
            if (!pools.TryGetValue(prefabIndex, out list)) {
                list = new List<IPooledObject>();
                pools.Add(prefabIndex, list);
            }

            foreach (var obj in list) {
                if (obj.IsFree()) {
                    var go = obj.GameObject;
                    go.transform.position = position;
                    go.transform.rotation = rotation;
                    obj.Acquire();
                    // Networked objects not on the server do nothing
                    // for Acquire - don't return them.
                    if (!obj.IsFree()) {
                        return obj.GameObject;
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
            IPooledObject pooledObj = newObj.GetComponent<NetworkPooledObject>();
            if (pooledObj != null) {
                NetworkServer.Spawn(newObj);
            } else {
                pooledObj = newObj.GetComponent<PooledObject>();
            }
            list.Add(pooledObj);
            return newObj;
        }
    }
}