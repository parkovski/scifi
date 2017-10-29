using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SciFi.Util {
    /// A crude debug field on the screen. This is useful for showing values
    /// that update often, since you can request a field and update it,
    /// instead of spewing messages to the console.
    public class DebugPrinter : MonoBehaviour {
        /// Individually controllable lines in the debug window.
        List<string> fields;
        public Text text;
        /// Editor param to show/hide the debug window.
        /// If false, the window will be destroyed on load,
        /// and won't be brought back up again.
        public bool showDebugInfo;

        /// There can only be one DebugPrinter active per game -
        /// use this to access it.
        public static DebugPrinter Instance { get; private set; }

        public DebugPrinter() {
            Instance = this;

            fields = new List<string>();
            fields.Add("Debug:");
        }

        void Start() {
            if (!showDebugInfo) {
                Destroy(text);
                text = null;
            }
        }

        /// Redraw the debug window from the list of fields.
        void RefreshText() {
            if (text == null) {
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var f in fields) {
                if (f == null) {
                    continue;
                }
                sb.AppendLine(f);
            }

            text.text = sb.ToString();
        }

        /// Request a new line in the debug window.
        public int NewField() {
            var index = fields.Count;
            fields.Add(null);
            return index;
        }

        /// Clear a line in the debug window.
        /// A clear line will not be printed (there won't be a space between lines).
        public void ClearField(int index) {
            fields[index] = null;
            RefreshText();
        }

        /// Set a line in the debug window. The <c>index</c> must
        /// have been returned by <see cref="NewField" />.
        public void SetField(int index, string text) {
            fields[index] = text;
            RefreshText();
        }
    }

    public class DebugField<T> {
        int fieldId;
        Func<T, string> getText;

        public DebugField(Func<T, string> getText) {
            fieldId = DebugPrinter.Instance?.NewField() ?? -1;
            this.getText = getText;
        }

        private int GetField() {
            if (fieldId == -1) {
                var dp = DebugPrinter.Instance;
                if (dp == null) { return -1; }
                fieldId = dp.NewField();
            }
            return fieldId;
        }

        public void Clear() {
            DebugPrinter.Instance?.ClearField(GetField());
        }

        public void Update(T item) {
            DebugPrinter.Instance?.SetField(GetField(), getText(item));
        }
    }

    public class DebugField {
        DebugField<object> field;
        string text;

        public DebugField(Func<string> getText) {
            field = new DebugField<object>(unused => getText());
        }

        // TODO: Remove this. Passing lambdas makes it so this can exist
        // with no overhead when not in debug mode.
        // (Also TODO: make methods partial/conditional.)
        public DebugField() {
            field = new DebugField<object>(unused => this.text);
        }

        public void Clear() {
            field.Clear();
        }

        public void Update() {
            field.Update(null);
        }

        public void Set(string text) {
            this.text = text;
            Update();
        }
    }
}