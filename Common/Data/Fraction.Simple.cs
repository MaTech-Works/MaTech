// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using MaTech.Common.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace MaTech.Common.Data {
    // todo: make all arithmetics checked
    [Serializable]
    public partial struct FractionImproper : IComparable<FractionImproper>, IEquatable<FractionImproper> {
        public static readonly FractionImproper invalid = default;
        public static readonly FractionImproper zero = new(0);
        public static readonly FractionImproper maxValue = new(int.MaxValue);
        public static readonly FractionImproper minValue = -maxValue; // avoid int.MinValue for negation overflow

        [SerializeField, FormerlySerializedAs("_num")] private int a;
        [SerializeField, FormerlySerializedAs("_den")] private int b;

        static FractionImproper() { Assert.IsFalse(default(FractionImproper).IsValid); }

        public FractionImproper(int value) { a = value; b = 1; }
        public FractionImproper(int numerator, int denominator) { a = numerator; b = denominator; }

        public int Numerator { readonly get => a; set => a = value; }
        public int Denominator { readonly get => b; set => b = value; }

        public readonly bool IsZero => a == 0;
        public readonly bool IsValid => b != 0 && a != int.MinValue;
        public readonly bool IsInvalid => b == 0 || a == int.MinValue;

        public readonly float Float => (float)a / b;
        public readonly double Double => (double)a / b;

        public readonly FractionImproper Decimal => ((FractionMixed)this).Decimal;
        public readonly float DecimalFloat => Decimal.Float;
        public readonly double DecimalDouble => Decimal.Double;
        
        public readonly FractionMixed Mixed => FractionMixed.Normal(0, a, b);
        
        public static bool operator true(FractionImproper fraction) => fraction.IsValid;
        public static bool operator false(FractionImproper fraction) => fraction.IsInvalid;

        public static explicit operator float(FractionImproper fraction) { return fraction.Float; }
        public static explicit operator double(FractionImproper fraction) { return fraction.Double; }
        
        public static implicit operator FractionImproper(int integer) => new(integer);
        public static implicit operator FractionImproper((int numerator, int denominator) t) => new(t.numerator, t.denominator);
        
        private void Reduce() {
            if (b == 0) return;
            int t = MathUtil.GCD(a, b);
            a /= t; b /= t;
        }

        public readonly FractionImproper Validated => IsValid ? this : invalid;
        public readonly FractionImproper Normalized => b >= 0 ? Validated : new(-a, -b);
        public readonly FractionImproper Reduced { get { var clone = this; clone.Reduce(); return clone; } }
        public readonly FractionImproper Simplified { get { var clone = Normalized; clone.Reduce(); return clone; } }

        public readonly FractionImproper Negated => IsValid ? new(-a, b) : invalid;
        public readonly FractionImproper Inversed => Valid(b, a);
        
        public readonly int Floored => b == 0 ? 0 : a / b;
        public readonly int Rounded => b == 0 ? 0 : (a + b / 2) / b;
        public readonly int Ceiling => b == 0 ? 0 : (a + b - 1) / b;
        
        public static FractionImproper operator-(FractionImproper x) => x.Negated;

        public static FractionImproper operator+(FractionImproper x, FractionImproper y) => checked(Simple(x.a * y.b + y.a * x.b, x.b * y.b));
        public static FractionImproper operator-(FractionImproper x, FractionImproper y) => checked(Simple(x.a * y.b - y.a * x.b, x.b * y.b));
        public static FractionImproper operator*(FractionImproper x, FractionImproper y) => checked(Simple(x.a * y.a, x.b * y.b));
        public static FractionImproper operator/(FractionImproper x, FractionImproper y) => checked(Simple(x.a * y.b, x.b * y.a));

        public static FractionImproper operator+(FractionImproper x, int value) => checked(Valid(x.a + value * x.b, x.b));
        public static FractionImproper operator-(FractionImproper x, int value) => checked(Valid(x.a - value * x.b, x.b));
        public static FractionImproper operator*(FractionImproper x, int value) => checked(Reduce(x.a * value, x.b));
        public static FractionImproper operator/(FractionImproper x, int value) => checked(Reduce(x.a, x.b * value));
        
        public static FractionImproper operator+(int value, FractionImproper x) => checked(Valid(x.a + value * x.b, x.b));
        public static FractionImproper operator-(int value, FractionImproper x) => checked(Valid(x.a - value * x.b, x.b));
        public static FractionImproper operator*(int value, FractionImproper x) => checked(Reduce(x.a * value, x.b));
        public static FractionImproper operator/(int value, FractionImproper x) => checked(Reduce(x.b * value, x.a));
        
        public static FractionImproper Max(FractionImproper x, FractionImproper y) => x > y ? x : y;
        public static FractionImproper Min(FractionImproper x, FractionImproper y) => x < y ? x : y;

        public readonly bool Equals(FractionImproper x) => b != 0 && x.b != 0 && CompareTo(x) == 0;
        public readonly int CompareTo(FractionImproper other) {
            long x = b == 0 ? long.MaxValue : a * other.b;
            long y = other.b == 0 ? long.MaxValue : other.a * b;
            return x.CompareTo(y);
        }

        public static bool operator<(FractionImproper x, FractionImproper y) => x.b != 0 && y.b != 0 && x.CompareTo(y) < 0;
        public static bool operator>(FractionImproper x, FractionImproper y) => x.b != 0 && y.b != 0 && x.CompareTo(y) > 0;
        public static bool operator<=(FractionImproper x, FractionImproper y) => x.b != 0 && y.b != 0 && x.CompareTo(y) <= 0;
        public static bool operator>=(FractionImproper x, FractionImproper y) => x.b != 0 && y.b != 0 && x.CompareTo(y) >= 0;
        public static bool operator==(FractionImproper x, FractionImproper y) => x.b != 0 && y.b != 0 && x.CompareTo(y) == 0;
        public static bool operator!=(FractionImproper x, FractionImproper y) => x.b != 0 && y.b != 0 && x.CompareTo(y) != 0;
        
        public static FractionImproper Valid(int numerator, int denominator) => new FractionImproper(numerator, denominator).Validated;
        public static FractionImproper Reduce(int numerator, int denominator) => new FractionImproper(numerator, denominator).Reduced;
        public static FractionImproper Normal(int numerator, int denominator) => new FractionImproper(numerator, denominator).Normalized;
        public static FractionImproper Simple(int numerator, int denominator) => new FractionImproper(numerator, denominator).Simplified;

        public static FractionImproper Division(int division) => new(1, division);

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator.
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="denominator"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode for <see cref="MathUtil.RoundToInt(double, MathUtil.RoundingMode)"/> </param>
        public static FractionImproper FromFloatRounded(double value, int denominator, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            int numerator = MathUtil.RoundToInt(value * denominator, mode);
            return new FractionImproper(numerator, denominator).Normalized;
        }
        
        /// <summary>
        /// 用连分数法寻找分母在 maxDenominator 以内离 value 最近的分数
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="maxDenominator"> Maximum denominator that can be produced </param>
        public static FractionImproper FromFloat(double value, int maxDenominator = 1000) {
            int a0 = (int)Math.Round(value);
            FractionImproper result = new FractionImproper(a0);

            // 连分数 a0+1/(a1+1/(a2+1/(...(an+1/x)...))) 中的剩余展开值 x，其后迭代运算直至分母超过限制达到最优解
            double remain = value - a0;
            
            double invMaxDenominator = 1.0 / maxDenominator;
            if (Math.Abs(remain) < invMaxDenominator) { // 做一个浮点数比较，如果 remain * 2 小于 1/maxDenominator 了，那分母势必大于 maxDenominator，而且有可能爆int
                if (Math.Abs(remain) * 2 < invMaxDenominator) return result; // 分母太小，不忍直视
                return new FractionImproper(Math.Sign(remain), maxDenominator); // 0 与 1/maxDenominator 比起来，1/maxDenominator 更优的情况
            }

            List<int> arr = threadLocalCachedList.Value;
            arr.Clear();
            arr.Add(a0);

            while (Math.Abs(remain) * 2 >= invMaxDenominator) { // 先为 remain 判断，避免爆int
                double invRemain = 1 / remain;
                int a = (int)Math.Round(invRemain);
                remain = invRemain - a;
                arr.Add(a);

                FractionImproper next = FromContinuousFraction(arr); // 由于连分数为反序计算，所以无法重用前一次迭代的计算结果
                if (Math.Abs(next.b) > maxDenominator) break;
                result = next;
            }

            return result.Normalized;
            
            FractionImproper FromContinuousFraction(List<int> values) { // 反序取倒数相加
                var result = zero;
                for (int i = values.Count - 1; i >= 0; --i) {
                    result += values[i];
                    if (i != 0) result = result.Inversed;
                }
                return result;
            }
        }
        private static readonly ThreadLocal<List<int>> threadLocalCachedList = new(() => new List<int>());
        
        public static FractionImproper SafeAdd(in FractionImproper x, in FractionImproper y) {
            if (x.b == 0 || y.b == 0) return invalid;
            try {
                if (x.b == y.b) {
                    return Normal(x.a + y.a, x.b);
                }
                int d = MathUtil.GCD(x.b, y.b);
                int xd = x.b / d;
                int yd = y.b / d;
                return Normal(x.a * yd + y.a * xd, x.b * yd);
            } catch (OverflowException) {
                return invalid;
            }
        }
        public static FractionImproper SafeSubtract(in FractionImproper x, in FractionImproper y) {
            if (x.b == 0 || y.b == 0) return invalid;
            try {
                if (x.b == y.b) {
                    return Normal(x.a - y.a, x.b);
                }
                int d = MathUtil.GCD(x.b, y.b);
                int xd = x.b / d;
                int yd = y.b / d;
                return Normal(x.a * yd - y.a * xd, x.b * yd);
            } catch (OverflowException) {
                return invalid;
            }
        }
        public static FractionImproper SafeMultiply(in FractionImproper x, in FractionImproper y) {
            if (x.b == 0 || y.b == 0) return invalid;
            try {
                var s = x.Reduced;
                var t = y.Reduced;
                s = Reduce(s.a, t.b);
                t = Reduce(t.a, s.b);
                return Normal(s.a * t.a, s.b * t.b);
            } catch (OverflowException) {
                return invalid;
            }
        }
        public static FractionImproper SafeDivide(in FractionImproper x, in FractionImproper y) {
            if (x.b == 0 || y.b == 0) return invalid;
            try {
                var s = x.Reduced;
                var t = y.Reduced; // notice that t is inversed on the next step
                s = Reduce(s.a, t.a);
                t = Reduce(t.b, s.b);
                return Normal(x.a * y.b, x.b * y.a);
            } catch (OverflowException) {
                return invalid;
            }
        }

        public override readonly string ToString() { return b == 0 ? "Invalid" : $"{a}/{b}"; }
        public readonly int[] ToArray() => new[] { a, b };

        public override readonly bool Equals(object obj) => obj is FractionImproper other && Equals(other);
        public override readonly int GetHashCode() { var x = Simplified; return HashCode.Combine(x.a, x.b); }
    }
}