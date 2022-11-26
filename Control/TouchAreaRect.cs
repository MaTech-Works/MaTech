using UnityEditor;
using UnityEngine;

namespace MaTech.Control {
    public class TouchAreaRect : TouchArea3DBase {
        public Rect rect = new Rect(0, 0, 1, 1);
        protected override bool Contains(Vector2 localPosition) {
            return rect.Contains(localPosition);
        }
        
        #if UNITY_EDITOR
        protected override void DrawLocalGizmos() {
            Handles.DrawAAPolyLine(rect.min, new Vector2(rect.xMin, rect.yMax), rect.max, new Vector2(rect.xMax, rect.yMin), rect.min);
        }
        #endif
    }
}