using UnityEngine;
using System;
using System.Collections.Generic;

namespace SciFi {
    public class MultiStateChangeListener : IStateChangeListener {
        List<IStateChangeListener> listeners;

        public MultiStateChangeListener() {
            listeners = new List<IStateChangeListener>();
        }

        public MultiStateChangeListener(IEnumerable<IStateChangeListener> list)
            : this()
        {
            listeners.AddRange(list);
        }

        public void Add(IStateChangeListener listener) {
            listeners.Add(listener);
        }

        void Each(Action<IStateChangeListener> action) {
            for (var i = 0; i < listeners.Count; i++) {
                action(listeners[i]);
            }
        }

        public void GameStarted() {
            Each(l => l.GameStarted());
        }

        public void GameEnded() {
            Each(l => l.GameEnded());
        }

        public void ControlStateChanged(int control, bool active) {
            Each(l => l.ControlStateChanged(control, active));
        }

        public void ControlStateChanged(int control, float amount) {
            Each(l => l.ControlStateChanged(control, amount));
        }

        public void PlayerPositionChanged(int playerId, Vector2 position) {
            Each(l => l.PlayerPositionChanged(playerId, position));
        }

        public void DamageChanged(int playerId, int newDamage) {
            Each(l => l.DamageChanged(playerId, newDamage));
        }

        public void LifeChanged(int playerId, int newLives) {
            Each(l => l.LifeChanged(playerId, newLives));
        }

        public void ObjectCreated(GameObject obj) {
            Each(l => l.ObjectCreated(obj));
        }

        public void ObjectWillBeDestroyed(GameObject obj) {
            Each(l => l.ObjectWillBeDestroyed(obj));
        }
    }
}