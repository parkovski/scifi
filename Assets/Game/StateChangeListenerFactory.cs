using System.Collections.Generic;

namespace SciFi {
    public class StateChangeListenerFactory {
        List<IStateChangeListener> listeners;

        public StateChangeListenerFactory() {
            listeners = new List<IStateChangeListener>();
        }

        public void Add(IStateChangeListener listener) {
            listeners.Add(listener);
        }

        public IStateChangeListener Get() {
            if (listeners.Count == 0) {
                return new EmptyStateChangeListener();
            }
            else if (listeners.Count == 1) {
                return listeners[0];
            } else {
                return new MultiStateChangeListener(listeners);
            }
        }
    }
}