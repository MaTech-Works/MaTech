// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace MaTech.Common.Data {
    // todo: make all arithmetics checked
    [Serializable]
    public partial struct FractionMixed : IComparable<FractionMixed>, IEquatable<FractionMixed> {
        public static readonly FractionMixed invalid = default;
        public static readonly FractionMixed zero = new(0);
        public static readonly FractionMixed maxValue = new(int.MaxValue);
        public static readonly FractionMixed minValue = -maxValue; // avoid int.MinValue for negation overflow

        [SerializeField, FormerlySerializedAs("_int")] private int n;
        [SerializeField, FormerlySerializedAs("_num")] private int a;
        [SerializeField, FormerlySerializedAs("_den")] private int b;

        static FractionMixed() { Assert.IsFalse(default(FractionMixed).IsValid); }

        public FractionMixed(int value) { n = value; a = 0; b = 1; }
        public FractionMixed(int integer, int numerator, int denominator) { n = integer; a = numerator; b = denominator; }
        
        public int Integer { readonly get => n; set => n = value; }
        public int Numerator { readonly get => a; set => a = value; }
        public int Denominator { readonly get => b; set => b = value; }

        public readonly bool IsZero => n == 0 && a == 0;
        public readonly bool IsValid => b != 0 && n != int.MaxValue;
        public readonly bool IsInvalid => b == 0 || n == int.MaxValue;

        public readonly float Float => n + (float)a / b;
        public readonly double Double => n + (double)a / b;

        public readonly FractionSimple Decimal => new(a, b);
        public readonly float DecimalFloat => (float)a / b;
        public readonly double DecimalDouble => (double)a / b;
        
        public readonly FractionSimple Improper => new(a + n * b, b);
        
        public static bool operator true(FractionMixed fraction) => fraction.IsValid;
        public static bool operator false(FractionMixed fraction) => fraction.IsInvalid;

        public static explicit operator float(FractionMixed fraction) => fraction.Float;
        public static explicit operator double(FractionMixed fraction) => fraction.Double;

        public static implicit operator FractionSimple(FractionMixed fraction) => fraction.Improper;
        public static implicit operator FractionMixed(FractionSimple fraction) => fraction.Mixed;

        public static implicit operator FractionMixed(int integer) => new(integer);
        public static implicit operator FractionMixed((int integer, int numerator, int denominator) t) => new(t.integer, t.numerator, t.denominator);

        private void Normalize() {
            if (b == 0) return;
            n += a / b;
            a %= b;
            if (b < 0) {
                a = -a;
                b = -b;
            }
            if (a < 0) {
                n--;
                a += b;
            }
        }
        private void Reduce() {
            if (b == 0) return;
            int t = MathUtil.GCD(a, b);
            a /= t; b /= t;
        }

        public readonly FractionMixed Validated => IsValid ? this : invalid;
        public readonly FractionMixed Normalized { get { var clone = this; clone.Normalize(); return clone; } }
        public readonly FractionMixed Reduced { get { var clone = this; clone.Reduce(); return clone; } }
        public readonly FractionMixed Simplified { get { var clone = this; clone.Normalize(); clone.Reduce(); return clone; } }
        
        public readonly FractionMixed Negated => Improper.Negated;
        public readonly FractionMixed Inversed => Improper.Inversed;
        
        public readonly int Floored => Improper.Floored;
        public readonly int Rounded => Improper.Rounded;
        public readonly int Ceiling => Improper.Ceiling;

        public static FractionMixed operator-(FractionMixed x) => x.Negated;
        
        public static FractionMixed operator+(FractionMixed x, FractionMixed y) => x.Improper + y.Improper;
        public static FractionMixed operator-(FractionMixed x, FractionMixed y) => x.Improper - y.Improper;
        public static FractionMixed operator*(FractionMixed x, FractionMixed y) => x.Improper * y.Improper;
        public static FractionMixed operator/(FractionMixed x, FractionMixed y) => x.Improper / y.Improper;
        
        public static FractionMixed operator+(FractionMixed x, int value) => Valid(x.n + value, x.a, x.b);
        public static FractionMixed operator-(FractionMixed x, int value) => Valid(x.n - value, x.a, x.b);
        public static FractionMixed operator*(FractionMixed x, int value) => Normal(x.n * value, x.a * value, x.b);
        public static FractionMixed operator/(FractionMixed x, int value) => Normal(0, x.n * x.b + x.a, x.b * value);
        
        public static FractionMixed operator+(int value, FractionMixed x) => Valid(x.n + value, x.a, x.b);
        public static FractionMixed operator-(int value, FractionMixed x) => Valid(x.n - value, x.a, x.b);
        public static FractionMixed operator*(int value, FractionMixed x) => Normal(x.n * value, x.a * value, x.b);
        public static FractionMixed operator/(int value, FractionMixed x) => Normal(0, x.b * value, x.a + x.n * x.b); // == Normalize(1 / x.Simple);

        public static FractionMixed Max(FractionMixed x, FractionMixed y) => x > y ? x : y;
        public static FractionMixed Min(FractionMixed x, FractionMixed y) => x < y ? x : y;

        public readonly bool Equals(FractionMixed x) => CompareTo(x) == 0;
        public readonly int CompareTo(FractionMixed other) {
            long x = b == 0 ? long.MaxValue : ((long)n * b + a) * other.b;
            long y = other.b == 0 ? long.MaxValue : ((long)other.n * other.b + other.a) * b;
            return x.CompareTo(y);
        }

        public static bool operator<(FractionMixed x, FractionMixed y) => x.b != 0 && y.b != 0 && x.CompareTo(y) < 0;
        public static bool operator>(FractionMixed x, FractionMixed y) => x.b != 0 && y.b != 0 && x.CompareTo(y) > 0;
        public static bool operator<=(FractionMixed x, FractionMixed y) => x.b != 0 && y.b != 0 && x.CompareTo(y) <= 0;
        public static bool operator>=(FractionMixed x, FractionMixed y) => x.b != 0 && y.b != 0 && x.CompareTo(y) >= 0;
        public static bool operator==(FractionMixed x, FractionMixed y) => x.b != 0 && y.b != 0 && x.CompareTo(y) == 0;
        public static bool operator!=(FractionMixed x, FractionMixed y) => x.b != 0 && y.b != 0 && x.CompareTo(y) != 0;
        
        public static FractionMixed Valid(int integer, int numerator, int denominator) => denominator == 0 ? invalid : new(integer, numerator, denominator);
        public static FractionMixed Normal(int integer, int numerator, int denominator) => new FractionMixed(integer, numerator, denominator).Normalized;
        public static FractionMixed Reduce(int integer, int numerator, int denominator) => new FractionMixed(integer, numerator, denominator).Reduced;
        public static FractionMixed Simple(int integer, int numerator, int denominator) => new FractionMixed(integer, numerator, denominator).Simplified;
        
        public static FractionMixed Division(int division) => new(0, 1, division);
        
        /// <summary>
        /// 用连分数法寻找分母在含 maxDenominator 以内离 value 最近的分数
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="maxDenominator"> Maximum denominator that can be produced </param>
        public static FractionMixed FromFloat(double value, int maxDenominator = 1000) => FractionSimple.FromFloat(value, maxDenominator);

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator.
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="denominator"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode as used in MathUtil.RoundToInt </param>
        public static FractionMixed FromFloatRounded(double value, int denominator, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            return FractionSimple.FromFloatRounded(value, denominator, mode);
        }

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator, with integer and decimal parts separated.
        /// </summary>
        /// <param name="integerPart"> The integer part of the sum </param>
        /// <param name="decimalPart"> The decimal part of the sum </param>
        /// <param name="denominator"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode as used in MathUtil.RoundToInt </param>
        public static FractionMixed FromFloatRounded(int integerPart, float decimalPart, int denominator, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            return FromFloatRounded(decimalPart, denominator, mode) + new FractionMixed(integerPart);
        }

        public static FractionMixed FromIntArray(int[] arr) {
            return new FractionMixed(arr[0], arr[1], arr[2]);
        }

        public override readonly string ToString() => $"{n} {a}/{b}";
        public readonly int[] ToArray() => new[] { n, a, b };
        
        public override readonly bool Equals(object obj) => obj is FractionMixed other && Equals(other);
        public override readonly int GetHashCode() => Improper.GetHashCode();
    }
}