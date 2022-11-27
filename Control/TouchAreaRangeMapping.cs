// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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