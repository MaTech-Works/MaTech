// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Algorithm {
    public static partial class Interpolators {
        private const double PI_OVER_TWO = Math.PI / 2;
        private const double TWO_OVER_PI = 2 / Math.PI;
        private const double INVERSE_TWO_PI = 1 / (Math.PI * 2);

        private static class Presets {
            public class Linear : IInterpolator {
                public double Map(double k) => k;
                public double Derivative(double k) => 1;
                public double Integral(double k) => k * k / 2;
            }

            public class HoldAndChange : IInterpolator {
                public double Map(double k) => k >= 1 ? 1 : 0;
                public double Derivative(double k) => 0; // TODO: 思考一下是否要针对1处的跳变进行额外处理
                public double Integral(double k) => Math.Max(k, 1);
            }

            public class ChangeAndHold : IInterpolator {
                public double Map(double k) => k > 0 ? 1 : 0;
                public double Derivative(double k) => 0; // TODO: 思考一下是否要针对0处的跳变进行额外处理
                public double Integral(double k) => Math.Max(k, 0);
            }

            public class EaseInQuad : IInterpolator {
                public double Map(double k) => k * k;
                public double Derivative(double k) => 2 * k;
                public double Integral(double k) => k * k * k / 3;
            }

            public class EaseOutQuad : IInterpolator {
                public double Map(double k) => (2 - k) * k; // 1-(1-k)^2
                public double Derivative(double k) => 2 - 2 * k;
                public double Integral(double k) => k * k * (1 - k / 3); // k^2-k^3/3
            }

            public class EaseInOutQuad : IInterpolator {
                public double Map(double k) {
                    double t = k * 2 - 1;
                    return ((2 - Math.Abs(t)) * t + 1) / 2;
                }
                public double Derivative(double k) {
                    double t = k * 2 - 1;
                    return 2 - 2 * Math.Abs(t);
                }
                public double Integral(double k) {
                    double t = k * 2 - 1;
                    return (t * t * (1 - Math.Abs(t) / 3) + t) / 2;
                }
            }

            public class EaseInSine : IInterpolator {
                public double Map(double k) => 1 - Math.Cos(k * PI_OVER_TWO);
                public double Derivative(double k) => (PI_OVER_TWO * Math.Sin(k * PI_OVER_TWO));
                public double Integral(double k) => k - (TWO_OVER_PI * Math.Sin(k * PI_OVER_TWO));
            }

            public class EaseOutSine : IInterpolator {
                public double Map(double k) => Math.Sin(k * PI_OVER_TWO);
                public double Derivative(double k) => (PI_OVER_TWO * Math.Cos(k * PI_OVER_TWO));
                public double Integral(double k) => (-TWO_OVER_PI * Math.Sin(k * PI_OVER_TWO));
            }

            public class EaseInOutSine : IInterpolator {
                public double Map(double k) => (1 - Math.Cos(k * Math.PI)) / 2;
                public double Derivative(double k) => (PI_OVER_TWO * Math.Sin(k * Math.PI));
                public double Integral(double k) => k / 2 - (Math.Sin(k * Math.PI) * INVERSE_TWO_PI);
            }
        }
    }
}