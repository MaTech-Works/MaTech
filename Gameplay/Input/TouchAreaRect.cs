// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
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