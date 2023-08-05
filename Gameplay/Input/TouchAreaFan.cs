// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    public class TouchAreaFan : TouchAreaCircle {
        public float angleBegin = 0;
        public float angleEnd = 180;
        
        protected override bool Contains(Vector2 localPosition) {
            if (!CheckInRadius(localPosition)) return false;
            float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
            return Mathf.Repeat(angle - angleBegin, 360) < (angleEnd - angleBegin);
        }
        
        #if UNITY_EDITOR
        protected override void DrawLocalGizmos() {
            if (angleBegin > angleEnd) return;
            Vector3 arcBegin = Quaternion.AngleAxis(angleBegin, Vector3.forward) * Vector3.right * radius;
            Vector3 arcEnd = Quaternion.AngleAxis(angleEnd, Vector3.forward) * Vector3.right * radius;
            if (radius >= 0) {
                Handles.DrawAAPolyLine(arcBegin, Vector3.zero, arcEnd);
            } else {
                Handles.DrawAAPolyLine(arcBegin, arcBegin * 1000);
                Handles.DrawAAPolyLine(arcEnd, arcEnd * 1000);
            }
            Handles.DrawWireArc(Vector3.zero, Vector3.forward, arcBegin, angleEnd - angleBegin, radius);
        }
        #endif
    }
}