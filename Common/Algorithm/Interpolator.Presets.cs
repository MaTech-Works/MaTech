// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Utils;

namespace MaTech.Common.Algorithm {
    public static partial class Interpolators {
        private const double PiOverTwo = Math.PI / 2;
        private const double TwoOverPi = 2 / Math.PI;
        private const double InverseTwoPi = 1 / (Math.PI * 2);

        private static class Presets {
            public class Linear : IInterpolator {
                public double Map(double t) => t;
                public double Derivative(double t) => 1;
                public double Integral(double t) => t * t / 2;
            }

            public class HoldStart : IInterpolator {
                public double Map(double t) => 0;
                public double Derivative(double t) => 0;
                public double Integral(double t) => 0;
            }

            public class HoldEnd : IInterpolator {
                public double Map(double t) => 1;
                public double Derivative(double t) => 0;
                public double Integral(double t) => t;
            }

            public class ChangeAtStart : IInterpolator {
                public double Map(double t) => t > 0 ? 1 : 0;
                public double Derivative(double t) => t.Exactly(0) ? double.PositiveInfinity : 0;
                public double Integral(double t) => Math.Max(t, 0);
            }

            public class ChangeAtEnd : IInterpolator {
                public double Map(double t) => t >= 1 ? 1 : 0;
                public double Derivative(double t) => t.Exactly(1) ? double.PositiveInfinity : 0;
                public double Integral(double t) => Math.Max(t - 1, 1);
            }

            public class EaseInQuad : IInterpolator {
                public double Map(double t) => t * t;
                public double Derivative(double t) => 2 * t;
                public double Integral(double t) => t * t * t / 3;
            }

            public class EaseOutQuad : IInterpolator {
                public double Map(double t) => (2 - t) * t; // 1-(1-k)^2
                public double Derivative(double t) => 2 - 2 * t;
                public double Integral(double t) => t * t * (1 - t / 3); // k^2-k^3/3
            }

            public class EaseInOutQuad : IInterpolator {
                public double Map(double t) {
                    double x = t * 2 - 1;
                    return ((2 - Math.Abs(x)) * x + 1) / 2;
                }
                public double Derivative(double t) {
                    double x = t * 2 - 1;
                    return 2 - 2 * Math.Abs(x);
                }
                public double Integral(double t) {
                    double x = t * 2 - 1;
                    return (x * x * (1 - Math.Abs(x) / 3) + x) / 2;
                }
            }

            public class EaseInSine : IInterpolator {
                public double Map(double t) => 1 - Math.Cos(t * PiOverTwo);
                public double Derivative(double t) => (PiOverTwo * Math.Sin(t * PiOverTwo));
                public double Integral(double t) => t - (TwoOverPi * Math.Sin(t * PiOverTwo));
            }

            public class EaseOutSine : IInterpolator {
                public double Map(double t) => Math.Sin(t * PiOverTwo);
                public double Derivative(double t) => (PiOverTwo * Math.Cos(t * PiOverTwo));
                public double Integral(double t) => (-TwoOverPi * Math.Sin(t * PiOverTwo));
            }

            public class EaseInOutSine : IInterpolator {
                public double Map(double t) => (1 - Math.Cos(t * Math.PI)) / 2;
                public double Derivative(double t) => (PiOverTwo * Math.Sin(t * Math.PI));
                public double Integral(double t) => t / 2 - (Math.Sin(t * Math.PI) * InverseTwoPi);
            }
        }
    }
}