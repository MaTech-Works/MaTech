// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    public class TouchAreaPolygon : TouchArea3DBase {
        public Vector2[] vertices = { Vector2.left, Vector2.up, Vector2.right, Vector2.down };
        
        protected override bool Contains(Vector2 localPosition) {
            if (vertices == null || vertices.Length < 2) return false;
            
            // 以localPosition为射线起点沿多边形扫一圈，角度和不为零即是在内部
            float angle = 0;
            var lastDist = vertices[vertices.Length - 1] - localPosition;
            foreach (var vert in vertices) {
                var dist = vert - localPosition;
                angle += Vector2.SignedAngle(lastDist, dist);
                lastDist = dist;
            }
            
            return Mathf.Abs(angle) > 180;
        }
        
        #if UNITY_EDITOR
        private Vector3[] vertices3D;
        protected override void DrawLocalGizmos() {
            if (vertices == null || vertices.Length < 2) return;
            if (vertices3D == null || vertices3D.Length != vertices.Length + 1) vertices3D = new Vector3[vertices.Length + 1];
            for (int i = 0, n = vertices.Length; i < n; ++i) vertices3D[i] = vertices[i];
            vertices3D[vertices.Length] = vertices[0];
            Handles.DrawAAPolyLine(vertices3D);
        }
        #endif
    }
}