using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace SciFi.Debug {
    public class DebugPrinter : MonoBehaviour {
        List<string> fields;
        public Text text;
        public bool showDebugInfo;

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

        public int NewField() {
            var index = fields.Count;
            fields.Add(null);
            RefreshText();
            return index;
        }

        public void ClearField(int index) {
            fields[index] = null;
            RefreshText();
        }

        public void SetField(int index, string text) {
            fields[index] = text;
            RefreshText();
        }

        public string GetField(int index) {
            return fields[index];
        }
    }
}