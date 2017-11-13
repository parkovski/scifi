using System;
using UnityEngine;

namespace SciFi.UI {
    public interface IRefreshComponent {
        void RefreshComponent(string action);
    }

    [Serializable]
    public struct RefreshButton {
        [SerializeField]
#pragma warning disable 414 // Unused variable
        string action;
#pragma warning restore 414

        public RefreshButton(string action) {
            this.action = action;
        }
    }
}