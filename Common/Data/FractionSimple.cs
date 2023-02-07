// Copyright (c) 2022, LuiCat (as MaTech)
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
    [JsonConverter(typeof(FractionJsonConverter))]
    public struct FractionSimple : IComparable<FractionSimple>, IEquatable<FractionSimple> {
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

        public FractionSimple(int numer, int denom) {
            _num = numer;
            _den = denom;
        }

        public void Set(int numer, int denom) {
            _num = numer;
            _den = denom;
        }

        public int Numer { get => _num; set => _num = value; }
        public int Denom { get => _den; set => _den = value; }

        public float Float => (float)_num / _den;
        public double Double => (double)_num / _den;

        public FractionSimple Decimal => ((Fraction)this).Decimal;
        public float DecimalFloat => Decimal.Float;
        public double DecimalDouble => Decimal.Double;

        public int Rounded => _den == 0 ? 0 : (_num + _den / 2) / _den;

        public static explicit operator float(FractionSimple frac) { return frac.Float; }
        public static explicit operator double(FractionSimple frac) { return frac.Double; }

        public void Normalize() {
            switch (_den) {
            case 0:
                this = invalid;
                break;
            case < 0:
                _den = -_den;
                _num = -_num;
                break;
            }
        }

        public void Reduce() {
            if (_den == 0) {
                this = invalid;
                return;
            }
            int t = MathUtil.GCD(_num, _den);
            _num /= t;
            _den /= t;
        }

        public void Inverse() {
            (_num, _den) = (_den, _num);
        }

        public FractionSimple Normalized {
            get {
                var result = this;
                result.Normalize();
                return result;
            }
        }
        public FractionSimple Reduced {
            get {
                var result = this;
                result.Reduce();
                return result;
            }
        }
        public FractionSimple Inversed {
            get {
                var result = this;
                result.Inverse();
                return result;
            }
        }

        public static FractionSimple operator+(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            if (x._den == y._den) {
                return new FractionSimple(x._num + y._num, x._den);
            }
            int d = MathUtil.GCD(x._den, y._den);
            int xd = x._den / d;
            int yd = y._den / d;
            return new FractionSimple(x._num * yd + y._num * xd, x._den * yd);
        }

        public static FractionSimple operator-(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            if (x._den == y._den) {
                return new FractionSimple(x._num - y._num, x._den);
            }
            int d = MathUtil.GCD(x._den, y._den);
            int xd = x._den / d;
            int yd = y._den / d;
            return new FractionSimple(x._num * yd - y._num * xd, x._den * yd);
        }

        public static FractionSimple operator*(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            var result = new FractionSimple(x._num * y._num, x._den * y._den);
            result.Normalize();
            result.Reduce();
            return result;
        }

        public static FractionSimple operator/(FractionSimple x, FractionSimple y) {
            if (x._den == 0 || y._den == 0) return invalid;
            var result = new FractionSimple(x._num * y._den, x._den * y._num);
            result.Normalize();
            result.Reduce();
            return result;
        }

        public static FractionSimple operator+(FractionSimple x, int value) {
            if (x._den == 0) return invalid;
            return new FractionSimple(x._num + value * x._den, x._den);
        }

        public static FractionSimple operator-(FractionSimple x, int value) {
            if (x._den == 0) return invalid;
            return new FractionSimple(x._num - value * x._den, x._den);
        }

        public static FractionSimple operator*(FractionSimple x, int scale) {
            if (x._den == 0) return invalid;
            var result = new FractionSimple(x._num * scale, x._den);
            result.Reduce();
            return result;
        }

        public static FractionSimple operator/(FractionSimple x, int denom) {
            if (x._den == 0) return invalid;
            var result = new FractionSimple(x._num, x._den * denom);
            result.Reduce();
            return result;
        }

        public static FractionSimple Max(FractionSimple x, FractionSimple y) => x > y ? x : y;
        public static FractionSimple Min(FractionSimple x, FractionSimple y) => x < y ? x : y;

        public bool IsZero => _num == 0;
        public bool IsValid => _den != 0;

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
            var x = this;
            x.Normalize();
            x.Reduce();
            return HashCode.Combine(x._num, x._den);
        }

        private static readonly ThreadLocal<List<int>> threadLocalCachedList = new ThreadLocal<List<int>>(() => new List<int>());

        /// <summary>
        /// 用连分数法寻找分母在 maxDenom 以内离 value 最近的分数
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="maxDenom"> Maximum denominator that can be produced </param>
        public static FractionSimple FromFloat(double value, int maxDenom = 1000) {
            try {
                int a0 = (int)Math.Round(value);
                FractionSimple result = new FractionSimple(a0);

                // 连分数 a[0]+1/(a[1]+1/(a[2]+1/(...+1/(a[n] + 1/r)))) 中的剩余展开值 r
                double remain = value - a0;
                double invMaxDenom = 1.0 / maxDenom;
                if (Math.Abs(remain) < invMaxDenom) { // 做一个浮点数比较，如果 remain * 2 小于 1/maxDenom 了，那分母势必大于 maxDenom，而且有可能爆int
                    if (Math.Abs(remain) * 2 < invMaxDenom) return result; // 分母太小，不忍直视
                    return new FractionSimple(Math.Sign(remain), maxDenom); // 0 与 1/maxDenom 比起来，1/maxDenom 更优的情况
                }

                List<int> arr = threadLocalCachedList.Value;
                arr.Clear();
                arr.Add(a0);

                while (Math.Abs(remain) * 2 >= invMaxDenom) { // 先为 remain 判断，避免爆int
                    double invRemain = 1 / remain;
                    int a = (int)Math.Round(invRemain);
                    remain = invRemain - a;
                    arr.Add(a);

                    FractionSimple next = FromContinuousFraction(arr);
                    if (Math.Abs(next._den) > maxDenom) break;
                    result = next;
                }

                result.Normalize();
                return result;
            } catch (OverflowException) {
                return new FractionSimple();
            }
        }

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator.
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="denom"> Denominator for rounding </param>
        public static FractionSimple FromFloatRounded(double value, int denom, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            int numer = MathUtil.RoundToInt(value * denom, mode);
            var result = new FractionSimple(numer, denom);
            result.Normalize();
            return result;
        }

        /// <summary>
        /// 计算连分数合并后的分数
        /// </summary>
        public static FractionSimple FromContinuousFraction(List<int> values) {
            var result = zero;
            for (int i = values.Count - 1; i >= 0; --i) {
                result += values[i];
                if (i != 0) result.Inverse();
            }
            return result;
        }

        public override string ToString() { return _den == 0 ? "Invalid" : $"{_num}/{_den}"; }
        public int[] ToArray() => new[] { _num, _den };
    }
}