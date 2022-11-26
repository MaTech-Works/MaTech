using UnityEditor;
using UnityEngine;

namespace MaTech.Control {
    public class TouchAreaUIRect : TouchAreaUIBase {
        protected override bool Contains(Vector2 localPosition) {
            return CachedLocalRect.Contains(localPosition);
        }
        
        #if UNITY_EDITOR
        protected override void DrawLocalGizmos() {
            Rect rect = RectTransform.rect;
            Vector2 min = rect.min;
            Vector2 max = rect.max;
            Handles.DrawAAPolyLine(min, new Vector2(min.x, max.y), max, new Vector2(max.x, min.y), min);
        }
        #endif
    }
}