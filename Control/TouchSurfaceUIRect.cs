using MaTech.Common.Unity;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Control {
    public class TouchSurfaceUIRect : TouchSurfaceUIBase {
        public Rect coordRange = new Rect(0, 0, 4096, 4096);
        public MathUtil.RoundingMode roundingMode = MathUtil.RoundingMode.Round;

        protected override Vector2Int GetInputCoordFromRectNormalizedPosition(Vector2 normalizedPosition) {
            Vector2 raw = coordRange.NormalizedToPointUnclamped(normalizedPosition);
            return raw.RoundToInt(roundingMode);
        }
    }
}