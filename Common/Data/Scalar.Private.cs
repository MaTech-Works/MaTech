// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;

namespace MaTech.Common.Data {
    public partial struct Scalar {
        private readonly T Clamp<T>(in T result) where T : unmanaged => IsMax ? ClampRange<T>.max : IsMin ? ClampRange<T>.min : result;
        private static class ClampRange<T> where T : unmanaged {
            public static T min, max;
            static ClampRange() {
                void Assign<TValue>(in TValue minValue, in TValue maxValue) {
                    min = BoxlessConvert.To<T>.FromIdentity(minValue);
                    max = BoxlessConvert.To<T>.FromIdentity(maxValue);
                }
                if (typeof(T) == typeof(Scalar)) Assign(MinValue, MaxValue);
                if (typeof(T) == typeof(float)) Assign(float.NegativeInfinity, float.PositiveInfinity);
                if (typeof(T) == typeof(double)) Assign(double.NegativeInfinity, double.PositiveInfinity);
                if (typeof(T) == typeof(FractionMixed)) Assign(FractionMixed.maxValue, FractionMixed.minValue);
                if (typeof(T) == typeof(FractionImproper)) Assign(FractionImproper.maxValue, FractionImproper.minValue);
            }
        }

        private Scalar(int integer) {
            fraction = MathUtil.Clamp(integer, -MaxInteger, MaxInteger);
            tails = 0;
        }
        private Scalar(in FractionMixed fraction, float? tails = null) {
            this.fraction = MathUtil.Clamp(fraction, minFraction, maxFraction);
            this.tails = tails ?? 0.0f;
        }
        private Scalar(in FractionMixed fraction, double? total = null) {
            this.fraction = MathUtil.Clamp(fraction, minFraction, maxFraction);
            tails = (float?)(total - this.fraction.Double) ?? 0.0f;
        }
        private Scalar(double value, int maxDenominator, bool rounded) {
            value = MathUtil.Clamp(value, -MaxInteger, MaxInteger);
            fraction = rounded ? FractionMixed.FromFloatRounded(value, maxDenominator) : FractionMixed.FromFloat(value, maxDenominator);
            tails = (float)(value - fraction.Double);
        }
    }
}