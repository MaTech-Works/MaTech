// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    public class TouchSurfaceRadial : TouchSurfaceFlatBase {
        public float angleFromUpBegin = -90;
        public float angleFromUpEnd = 90;
        public float distanceBegin = 0;
        public float distanceEnd = 100;

        public Rect coordRange = new Rect(0, 0, 4096, 4096);
        public MathUtil.RoundingMode roundingMode = MathUtil.RoundingMode.Round;

        private Rect RadialRange => new Rect(angleFromUpBegin, distanceBegin, angleFromUpEnd - angleFromUpBegin, distanceEnd - distanceBegin);

        protected override Vector2Int GetInputCoordFromCastLocalPosition(Vector2 localPosition) {
            float angle = MathUtil.LowerBoundAngle(Vector2.SignedAngle(localSpaceUp, localPosition), angleFromUpBegin);
            float distance = localPosition.magnitude;

            var radialRange = RadialRange;
            var radialPosition = new Vector2(angle, distance);
            if (!radialRange.Contains(radialPosition) && !allowOutside) return invalidCoord;

            var raw = coordRange.NormalizedToPointUnclamped(radialRange.PointToNormalizedUnclamped(radialPosition));
            return raw.RoundToInt(roundingMode);
        }

        #if UNITY_EDITOR
        protected override void DrawLocalGizmos(bool selected) {
            base.DrawLocalGizmos(selected);

            float dk = selected ? 0.1f : 0.25f;

            Vector3 arcBegin = Quaternion.AngleAxis(angleFromUpBegin, localSpaceNormal) * localSpaceUp;
            for (float k = 0; k - 1 < 1e-3; k += dk) {
                Handles.DrawWireArc(Vector3.zero, localSpaceNormal, arcBegin, angleFromUpEnd - angleFromUpBegin, Mathf.Lerp(distanceBegin, distanceEnd, k));
            }

            for (float k = 0; k - 1 < 1e-3; k += dk) {
                Vector3 lineDirection = Quaternion.AngleAxis(Mathf.Lerp(angleFromUpBegin, angleFromUpEnd, k), localSpaceNormal) * localSpaceUp;
                Handles.DrawAAPolyLine(lineDirection * distanceBegin, lineDirection * distanceEnd);
            }
        }
        #endif
    }
}