using UnityEngine;

namespace SciFi.UI {
    [ExecuteInEditMode]
    public class ItemPicker : MonoBehaviour, IRefreshComponent {
        public Color seg1Color;
        public Color seg2Color;
        public Color seg3Color;
        public RefreshButton refresh = new RefreshButton("Refresh colors");

        private Material material;
        private float size;
        private Vector2 cameraExtents;
        private int idMode;
        private int idRadius;

        private const int MODE_SMALL = 0;
        private const int MODE_EXPAND = 1;
        private const int MODE_SEG1_SELECTED = 2;
        private const int MODE_SEG2_SELECTED = 3;
        private const int MODE_SEG3_SELECTED = 4;

        void Start() {
            idMode = Shader.PropertyToID("_Mode");
            idRadius = Shader.PropertyToID("_Radius");
            Reset();
        }

        void Reset() {
            material = GetComponent<SpriteRenderer>().sharedMaterial;
            material.SetColorArray("_SegColors", new [] { seg1Color, seg2Color, seg3Color });
            size = GetComponent<SpriteRenderer>().bounds.size.x;
            cameraExtents.y = Camera.main.orthographicSize;
            cameraExtents.x = cameraExtents.y * Camera.main.aspect;
        }

        void IRefreshComponent.RefreshComponent(string action) {
            Reset();
        }

        public void InputPositionChanged(Vector2 position) {
            // Change from default center -> top left coordinates to top right -> bottom left.
            position = cameraExtents - (Vector2)Camera.main.ScreenToWorldPoint(position);
            var radius = Mathf.Clamp(position.magnitude / size + .15f, .3f, 1f);
            if (radius > .6f) {
                float angle = Mathf.PI * .5f;
                if (position.x > .0001f) {
                    angle = Mathf.Atan2(position.y, position.x);
                }
                if (angle < Mathf.PI * .166667f) {
                    material.SetInt(idMode, MODE_SEG1_SELECTED);
                } else if (angle < Mathf.PI * .333333f) {
                    material.SetInt(idMode, MODE_SEG2_SELECTED);
                } else {
                    material.SetInt(idMode, MODE_SEG3_SELECTED);
                }
            } else {
                material.SetInt(idMode, MODE_EXPAND);
            }
            material.SetFloat(idRadius, radius);
        }

        public void InputEnded() {
            material.SetInt(idMode, MODE_SMALL);
            material.SetFloat(idRadius, .3f);
        }
    }
}