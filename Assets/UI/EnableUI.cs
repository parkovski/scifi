using UnityEngine;
using System.Collections.Generic;

namespace SciFi.UI {
    public interface IEnablableUIComponent {
        void Enable();
    }

    public class EnableUI : MonoBehaviour {
        List<IEnablableUIComponent> components = new List<IEnablableUIComponent>();
        bool initialized = false;

        public void Register(IEnablableUIComponent component) {
            if (initialized) {
                component.Enable();
            }
            components.Add(component);
        }

        public void Enable() {
            initialized = true;
            foreach (var c in components) {
                ((MonoBehaviour)c).enabled = true;
                c.Enable();
            }
        }
    }
}