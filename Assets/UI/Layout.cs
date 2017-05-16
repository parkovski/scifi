using UnityEngine;

namespace SciFi.UI {
    [ExecuteInEditMode]
    public class Layout : MonoBehaviour {
        public enum Anchor {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, Center, MiddleRight,
            BottomLeft, BottomCenter, BottomRight,
        }

        public enum MarginMode {
            ScreenPercent,
            ThisControlPercent,
            UnityUnits,
        }

        /// By providing 
        public Layout reference;
        public Anchor anchor = Anchor.Center;
        public bool autoWidth = true;
        public float percentWidth;
        public bool autoHeight = false;
        public float percentHeight;
        public MarginMode marginMode;
        public Vector2 margin;

        void Start() {
            CalculateLayoutRecursive();
        }

#if UNITY_EDITOR
        /*void Update() {
            CalculateLayout();
        }*/
#endif

        void CalculateLayoutRecursive() {
            if (reference != null) {
                reference.CalculateLayoutRecursive();
            }
            CalculateLayout();
        }

        void CalculateLayout() {
            Vector3 position;
            SetScale();
            if (reference == null) {
                position = GetRootAnchor();
            } else {
                position = GetReferenceAnchor();
            }
            transform.position = position;
        }

        void SetScale() {
            if (autoWidth && autoHeight) {
                return;
            }
            transform.localScale = new Vector3(1, 1, transform.localScale.z);
            var screenHeight = Camera.main.orthographicSize * 2;
            var screenWidth = screenHeight * Screen.width / Screen.height;
            var mySize = GetComponent<SpriteRenderer>().bounds.size;
            float scale;
            if (autoHeight) {
                scale = screenWidth * percentWidth / mySize.x;
            } else {
                scale = screenHeight * percentHeight / mySize.y;
            }
            if (scale <= 0) {
                return;
            }
            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
        }

        Vector3 GetRootAnchor() {
            float screenHeightExtent = Camera.main.orthographicSize;
            float screenWidthExtent = screenHeightExtent * Screen.width / Screen.height;
            Vector2 myExtents = GetComponent<SpriteRenderer>().bounds.extents;
            var z = transform.position.z;
            switch (anchor) {
            case Anchor.TopLeft:
                return new Vector3(-screenWidthExtent + myExtents.x, screenHeightExtent - myExtents.y, z);
            case Anchor.TopCenter:
                return new Vector3(0, screenHeightExtent - myExtents.y, z);
            case Anchor.TopRight:
                return new Vector3(screenWidthExtent - myExtents.x, screenHeightExtent - myExtents.y, z);
            case Anchor.MiddleLeft:
                return new Vector3(-screenWidthExtent + myExtents.x, 0, z);
            case Anchor.Center:
                return new Vector3(0, 0, z);
            case Anchor.MiddleRight:
                return new Vector3(screenWidthExtent - myExtents.x, 0, z);
            case Anchor.BottomLeft:
                return new Vector3(-screenWidthExtent + myExtents.x, -screenHeightExtent + myExtents.y, z);
            case Anchor.BottomCenter:
                return new Vector3(0, -screenHeightExtent + myExtents.y, z);
            case Anchor.BottomRight:
                return new Vector3(screenWidthExtent - myExtents.x, -screenHeightExtent + myExtents.y, z);
            default:
                throw new System.ArgumentOutOfRangeException("anchor");
            }
        }

        Vector3 GetReferenceAnchor() {
            Vector2 myExtents = GetComponent<SpriteRenderer>().bounds.extents;
            Vector2 referenceExtents = reference.GetComponent<SpriteRenderer>().bounds.extents;
            var z = transform.position.z;
            switch (anchor) {
            case Anchor.TopLeft:
                return new Vector3(reference.transform.position.x - referenceExtents.x - myExtents.x, reference.transform.position.y + referenceExtents.y + myExtents.y, z);
            case Anchor.TopCenter:
                return new Vector3(reference.transform.position.x, reference.transform.position.y + referenceExtents.y + myExtents.y, z);
            case Anchor.TopRight:
                return new Vector3(reference.transform.position.x + referenceExtents.x + myExtents.x, reference.transform.position.y + referenceExtents.y + myExtents.y, z);
            case Anchor.MiddleLeft:
                return new Vector3(reference.transform.position.x - referenceExtents.x - myExtents.x, reference.transform.position.y, z);
            case Anchor.Center:
                return new Vector3(reference.transform.position.x, reference.transform.position.y, z);
            case Anchor.MiddleRight:
                return new Vector3(reference.transform.position.x + referenceExtents.x + myExtents.x, reference.transform.position.y, z);
            case Anchor.BottomLeft:
                return new Vector3(reference.transform.position.x - referenceExtents.x - myExtents.x, reference.transform.position.y - referenceExtents.y - myExtents.y, z);
            case Anchor.BottomCenter:
                return new Vector3(reference.transform.position.x, reference.transform.position.y - referenceExtents.y - myExtents.y, z);
            case Anchor.BottomRight:
                return new Vector3(reference.transform.position.x + referenceExtents.x + myExtents.x, reference.transform.position.y - referenceExtents.y - myExtents.y, z);
            default:
                throw new System.ArgumentOutOfRangeException("anchor");
            }
        }
    }
}