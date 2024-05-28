// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
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