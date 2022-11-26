using UnityEditor;
using UnityEngine;

namespace MaTech.Control {
    public class TouchAreaRangeMapping : TouchAreaUIBase {
        [SerializeField]
        private int range;
        protected override bool Contains(Vector2 localPosition) {
            var delta = (localPosition.x - CachedLocalRect.x) / CachedLocalRect.width;
            keyIndex = Mathf.RoundToInt(delta * range);
            return CachedLocalRect.Contains(localPosition);
        }
        
        #if UNITY_EDITOR
        protected override void DrawLocalGizmos() {
            Rect rect = RectTransform.rect;
            Handles.DrawAAPolyLine(rect.min, new Vector2(rect.xMin, rect.yMax), rect.max, new Vector2(rect.xMax, rect.yMin), rect.min);
        }
        #endif
    }
}