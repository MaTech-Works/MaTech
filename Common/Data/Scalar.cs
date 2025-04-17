// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Common.Data {
    [Serializable]
    public partial struct Scalar {
        [SerializeField] private FractionMixed fraction;
        [SerializeField] private float tails;
        
        public readonly float Float => Clamp(fraction.Float + tails);
        public readonly double Double => Clamp(fraction.Double + tails);
        public readonly FractionMixed Fraction => Clamp(fraction);

        public readonly bool IsMax => fraction >= maxFraction;
        public readonly bool IsMin => fraction <= minFraction;

        public static implicit operator float(in Scalar obj) => obj.Float;
        public static implicit operator double(in Scalar obj) => obj.Double;
        public static implicit operator FractionMixed(in Scalar obj) => obj.fraction;
        public static implicit operator FractionImproper(in Scalar obj) => obj.fraction.Improper;
        public static implicit operator Scalar(int value) => new(value);
        public static implicit operator Scalar(float value) => Approximate(value);
        public static implicit operator Scalar(double value) => Approximate(value);
        public static implicit operator Scalar(FractionMixed value) => new(value, tails: null);
        public static implicit operator Scalar(FractionImproper value) => new(value, tails: null);
        public static implicit operator Scalar((int a, int b) t) => Improper(t.a, t.b);
        public static implicit operator Scalar((int n, int a, int b) t) => Mixed(t.n, t.a, t.b);
        
        public static Scalar Zero => new(0);
        public static Scalar MaxValue => maxFraction;
        public static Scalar MinValue => minFraction;
        
        public static Scalar Integer(int count) => new(count);
        public static Scalar Mixed(int n, int a, int b) => new(FractionMixed.Simple(n, a, b), tails: null);
        public static Scalar Improper(int a, int b) => new(FractionImproper.Simple(a, b), tails: null);
        public static Scalar Rounded(double value, int denominator) => new(FractionMixed.FromFloatRounded(value, denominator), value);
        public static Scalar Approximate(double value, int maxDenominator = 1000) => new(FractionMixed.FromFloat(value, maxDenominator), value);

        private Scalar(in Scalar value, in Scalar offset) {
            try {
                fraction = value.fraction + offset.fraction;
                tails = value.tails + offset.tails;
            } catch (OverflowException) {
                this = Approximate(value.Double + offset.Double);
            }
        }
        
        public static bool operator==(in Scalar a, in Scalar b) => a.fraction == b.fraction;
        public static bool operator!=(in Scalar a, in Scalar b) => a.fraction != b.fraction;

        public static Scalar operator+(in Scalar x, in Scalar y) => new(x, y);
        
        public static Scalar operator-(in Scalar x) => new(x.fraction.Negated, -x.tails);
        public static Scalar operator-(in Scalar x, in Scalar y) => new(x, -y);

        public static Scalar operator*(in Scalar x, in Scalar y) => new(x.fraction * y.fraction, total: x.Double * y.Double);
        public static Scalar operator*(in Scalar x, double y) => Approximate(x.fraction.Double * y);
        public static Scalar operator*(double x, in Scalar y) => Approximate(x * y.fraction.Double);
        
        public static Scalar operator/(in Scalar x, in Scalar y) => new(x.fraction / y.fraction, total: x.Double / y.Double);

        public readonly int CompareTo(Scalar other) => CompareTo(other, false);
        public readonly int CompareTo(in Scalar other, bool fractionOnly) {
            if (CompareUtil.TryCompareTo(fraction, other.fraction, out var result)) return result;
            return fractionOnly ? 0 : tails.CompareTo(other.tails);
        }
        
        public bool Equals(Scalar other) => CompareTo(other, true) == 0;
        public override bool Equals(object? obj) => obj is Scalar other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(fraction, tails);

        public override readonly string ToString() {
            if (IsMax) return "Max"; if (IsMin) return "Min";
            return tails.Near(0) ? $"{fraction}" : $"{fraction} ({(tails > 0 ? "+" : "-")}{tails:G3})";
        }
        
        private const int MaxInteger = 1000000000;
        private static readonly FractionMixed maxFraction = new(MaxInteger);
        private static readonly FractionMixed minFraction = new(-MaxInteger);
    }
}