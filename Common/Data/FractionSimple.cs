// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using MaTech.Common.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace MaTech.Common.Data {
    [Serializable]
    public partial struct FractionSimple : IComparable<FractionSimple>, IEquatable<FractionSimple> {
        // todo: make immutable? readonly struct cannot be serializable
        // todo: normalize on construct by default

        public static readonly FractionSimple invalid = new FractionSimple();
        public static readonly FractionSimple zero = new FractionSimple(0);
        public static readonly FractionSimple maxValue = new FractionSimple(int.MaxValue);
        public static readonly FractionSimple minValue = new FractionSimple(int.MinValue);

        // ReSharper disable InconsistentNaming
        [SerializeField]
        private int _num, _den;
        // ReSharper restore InconsistentNaming

        #if UNITY_EDITOR
        static FractionSimple() {
            Assert.IsFalse(default(FractionSimple).IsValid);
        }
        #endif

        public FractionSimple(int value) {
            _num = value;
            _den = 1;
        }

        public FractionSimple(int numerator, int denominator) {
            _num = numerator;
            _den = denominator;
        }

        public int Numerator { get => _num; set => _num = value; }
        public int Denominator { get => _den; set => _den = value; }

        public bool IsZero => _num == 0;
        public bool IsValid => _den != 0;
        public bool IsInvalid => _den == 0;

        public float Float => (float)_num / _den;
        public double Double => (double)_num / _den;

        public FractionSimple Decimal => ((Fraction)this).Decimal;
        public float DecimalFloat => Decimal.Float;
        public double DecimalDouble => Decimal.Double;
        
        public Fraction Mixed => Fraction.Normal(0, _num, _den);
        
        public static bool operator true(FractionSimple fraction) => fraction.IsValid;
        public static bool operator false(FractionSimple fraction) => fraction.IsInvalid;

        public static explicit operator float(FractionSimple fraction) { return fraction.Float; }
        public static explicit operator double(FractionSimple fraction) { return fraction.Double; }
        
        public static implicit operator FractionSimple(int integer) => new FractionSimple(integer);
        public static implicit operator FractionSimple((int numerator, int denominator) t) => new(t.numerator, t.denominator);
        
        private void Reduce() {
            if (_den == 0) return;
            int t = MathUtil.GCD(_num, _den);
            _num /= t; _den /= t;
        }

        public FractionSimple Validated => IsValid ? this : invalid;
        public FractionSimple Normalized => _den >= 0 ? Validated : new(-_num, -_den);
        public FractionSimple Reduced { get { var clone = this; clone.Reduce(); return clone; } }
        public FractionSimple Simplified { get { var clone = Normalized; clone.Reduce(); return clone; } }

        public FractionSimple Negated => Valid(-_num, _den);
        public FractionSimple Inversed => Valid(_den, _num);
        
        public int Floored => _den == 0 ? 0 : _num / _den;
        public int Rounded => _den == 0 ? 0 : (_num + _den / 2) / _den;
        public int Ceiling => _den == 0 ? 0 : (_num + _den - 1) / _den;
        
        public static FractionSimple operator-(FractionSimple x) => x.Negated;

        public static FractionSimple operator+(FractionSimple x, FractionSimple y) => Simple(x._num * y._den + y._num * x._den, x._den * y._den);
        public static FractionSimple operator-(FractionSimple x, FractionSimple y) => Simple(x._num * y._den - y._num * x._den, x._den * y._den);
        public static FractionSimple operator*(FractionSimple x, FractionSimple y) => Simple(x._num * y._num, x._den * y._den);
        public static FractionSimple operator/(FractionSimple x, FractionSimple y) => Simple(x._num * y._den, x._den * y._num);

        public static FractionSimple operator+(FractionSimple x, int value) => Valid(x._num + value * x._den, x._den);
        public static FractionSimple operator-(FractionSimple x, int value) => Valid(x._num - value * x._den, x._den);
        public static FractionSimple operator*(FractionSimple x, int value) => Reduce(x._num * value, x._den);
        public static FractionSimple operator/(FractionSimple x, int value) => Reduce(x._num, x._den * value);
        
        public static FractionSimple operator+(int value, FractionSimple x) => Valid(x._num + value * x._den, x._den);
        public static FractionSimple operator-(int value, FractionSimple x) => Valid(x._num - value * x._den, x._den);
        public static FractionSimple operator*(int value, FractionSimple x) => Reduce(x._num * value, x._den);
        public static FractionSimple operator/(int value, FractionSimple x) => Reduce(x._den * value, x._num);
        
        public static FractionSimple Max(FractionSimple x, FractionSimple y) => x > y ? x : y;
        public static FractionSimple Min(FractionSimple x, FractionSimple y) => x < y ? x : y;

        public int CompareTo(FractionSimple other) {
            long x = _den == 0 ? long.MaxValue : _num * other._den;
            long y = other._den == 0 ? long.MaxValue : other._num * _den;
            return x.CompareTo(y);
        }

        public bool Equals(FractionSimple x) => _den != 0 && x._den != 0 && CompareTo(x) == 0;
        public static bool operator<(FractionSimple x, FractionSimple y) => x._den != 0 && y._den != 0 && x.CompareTo(y) < 0;
        public static bool operator>(FractionSimple x, FractionSimple y) => x._den != 0 && y._den != 0 && x.CompareTo(y) > 0;
        public static bool operator<=(FractionSimple x, FractionSimple y) => x._den != 0 && y._den != 0 && x.CompareTo(y) <= 0;
        public static bool operator>=(FractionSimple x, FractionSimple y) => x._den != 0 && y._den != 0 && x.CompareTo(y) >= 0;
        public static bool operator==(FractionSimple x, FractionSimple y) => x._den != 0 && y._den != 0 && x.CompareTo(y) == 0;
        public static bool operator!=(FractionSimple x, FractionSimple y) => x._den != 0 && y._den != 0 && x.CompareTo(y) != 0;

        public override bool Equals(object obj) => obj is FractionSimple other && Equals(other);

        public override int GetHashCode() {
            var x = Simplified;
            return HashCode.Combine(x._num, x._den);
        }
        
        public static FractionSimple New(int numerator, int denominator) => new(numerator, denominator);
        public static FractionSimple Valid(int numerator, int denominator) => denominator == 0 ? invalid : new(numerator, denominator);
        public static FractionSimple Reduce(int numerator, int denominator) => new FractionSimple(numerator, denominator).Reduced;
        public static FractionSimple Normal(int numerator, int denominator) => new FractionSimple(numerator, denominator).Normalized;
        public static FractionSimple Simple(int numerator, int denominator) => new FractionSimple(numerator, denominator).Simplified;

        public static FractionSimple Division(int division) => new(1, division);
        
        /// <summary>
        /// 用连分数法寻找分母在 maxDenominator 以内离 value 最近的分数
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="maxDenominator"> Maximum denominator that can be produced </param>
        public static FractionSimple FromFloat(double value, int maxDenominator = 1000) {
            try {
                int a0 = (int)Math.Round(value);
                FractionSimple result = new FractionSimple(a0);

                // 连分数 a[0]+1/(a[1]+1/(a[2]+1/(...+1/(a[n] + 1/r)))) 中的剩余展开值 r
                double remain = value - a0;
                double invMaxDenominator = 1.0 / maxDenominator;
                if (Math.Abs(remain) < invMaxDenominator) { // 做一个浮点数比较，如果 remain * 2 小于 1/maxDenominator 了，那分母势必大于 maxDenominator，而且有可能爆int
                    if (Math.Abs(remain) * 2 < invMaxDenominator) return result; // 分母太小，不忍直视
                    return new FractionSimple(Math.Sign(remain), maxDenominator); // 0 与 1/maxDenominator 比起来，1/maxDenominator 更优的情况
                }

                List<int> arr = threadLocalCachedList.Value;
                arr.Clear();
                arr.Add(a0);

                while (Math.Abs(remain) * 2 >= invMaxDenominator) { // 先为 remain 判断，避免爆int
                    double invRemain = 1 / remain;
                    int a = (int)Math.Round(invRemain);
                    remain = invRemain - a;
                    arr.Add(a);

                    FractionSimple next = FromContinuousFraction(arr);
                    if (Math.Abs(next._den) > maxDenominator) break;
                    result = next;
                }

                return result.Normalized;
            } catch (OverflowException) {
                return new FractionSimple();
            }
        }

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator.
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="denominator"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode for <see cref="MathUtil.RoundToInt(double, MathUtil.RoundingMode)"/> </param>
        public static FractionSimple FromFloatRounded(double value, int denominator, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            int numerator = MathUtil.RoundToInt(value * denominator, mode);
            return new FractionSimple(numerator, denominator).Normalized;
        }

        /// <summary>
        /// 计算连分数合并后的分数
        /// </summary>
        public static FractionSimple FromContinuousFraction(List<int> values) {
            var result = zero;
            for (int i = values.Count - 1; i >= 0; --i) {
                result += values[i];
                if (i != 0) result = result.Inversed;
            }
            return result;
        }
        
        public static FractionSimple SafeAdd(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            if (x._den == y._den) {
                return Normal(x._num + y._num, x._den);
            }
            int d = MathUtil.GCD(x._den, y._den);
            int xd = x._den / d;
            int yd = y._den / d;
            return Normal(x._num * yd + y._num * xd, x._den * yd);
        }
        public static FractionSimple SafeSubtract(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            if (x._den == y._den) {
                return Normal(x._num - y._num, x._den);
            }
            int d = MathUtil.GCD(x._den, y._den);
            int xd = x._den / d;
            int yd = y._den / d;
            return Normal(x._num * yd - y._num * xd, x._den * yd);
        }
        public static FractionSimple SafeMultiply(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            var s = x.Reduced;
            var t = y.Reduced;
            s = Reduce(s._num, t._den);
            t = Reduce(t._num, s._den);
            return Normal(s._num * t._num, s._den * t._den);
        }
        public static FractionSimple SafeDivide(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            var s = x.Reduced;
            var t = y.Reduced; // notice that t is inversed on the next step
            s = Reduce(s._num, t._num);
            t = Reduce(t._den, s._den);
            return Normal(x._num * y._den, x._den * y._num);
        }

        public override string ToString() { return _den == 0 ? "Invalid" : $"{_num}/{_den}"; }
        public int[] ToArray() => new[] { _num, _den };
        
        private static readonly ThreadLocal<List<int>> threadLocalCachedList = new(() => new List<int>());
    }
}