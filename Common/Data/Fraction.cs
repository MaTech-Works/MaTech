// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MaTech.Common.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace MaTech.Common.Data {
    // todo: rename to FractionMixed and consider to rename fields to n,a,b
    // todo: add readonly to most methods and properties
    // todo: make all arithmetics checked
    [Serializable]
    [JsonConverter(typeof(FractionJsonConverter))]
    public struct Fraction : IComparable<Fraction>, IEquatable<Fraction> {
        public static readonly Fraction invalid = default;
        public static readonly Fraction zero = new(0);
        public static readonly Fraction maxValue = new(int.MaxValue);
        public static readonly Fraction minValue = new(int.MinValue);

        // ReSharper disable InconsistentNaming
        [SerializeField]
        private int _int, _num, _den;
        // ReSharper restore InconsistentNaming

        #if UNITY_EDITOR
        static Fraction() {
            Assert.IsFalse(default(Fraction).IsValid);
        }
        #endif

        public Fraction(int value) {
            _int = value;
            _num = 0;
            _den = 1;
        }
        
        public Fraction(int integer, int numerator, int denominator) {
            _int = integer;
            _num = numerator;
            _den = denominator;
        }
        
        public int Integer { get => _int; set => _int = value; }
        public int Numerator { get => _num; set => _num = value; }
        public int Denominator { get => _den; set => _den = value; }

        public bool IsZero => _int == 0 && _num == 0;
        public bool IsValid => _den != 0;
        public bool IsInvalid => _den == 0;

        public float Float => _int + (float)_num / _den;
        public double Double => _int + (double)_num / _den;

        public FractionSimple Decimal => new(_num, _den);
        public float DecimalFloat => (float)_num / _den;
        public double DecimalDouble => (double)_num / _den;
        
        public FractionSimple Improper => new(_num + _int * _den, _den);
        
        public static bool operator true(Fraction fraction) => fraction.IsValid;
        public static bool operator false(Fraction fraction) => fraction.IsInvalid;

        public static explicit operator float(Fraction fraction) => fraction.Float;
        public static explicit operator double(Fraction fraction) => fraction.Double;

        public static implicit operator FractionSimple(Fraction fraction) => fraction.Improper;
        public static implicit operator Fraction(FractionSimple fraction) => fraction.Mixed;

        public static implicit operator Fraction(int integer) => new(integer);
        public static implicit operator Fraction((int integer, int numerator, int denominator) t) => new(t.integer, t.numerator, t.denominator);

        private void Normalize() {
            if (_den == 0) return;
            _int += _num / _den;
            _num %= _den;
            if (_den < 0) {
                _num = -_num;
                _den = -_den;
            }
            if (_num < 0) {
                _int--;
                _num += _den;
            }
        }
        private void Reduce() {
            if (_den == 0) return;
            int t = MathUtil.GCD(_num, _den);
            _num /= t; _den /= t;
        }

        public Fraction Validated => IsValid ? this : invalid;
        public Fraction Normalized { get { var clone = this; clone.Normalize(); return clone; } }
        public Fraction Reduced { get { var clone = this; clone.Reduce(); return clone; } }
        public Fraction Simplified { get { var clone = this; clone.Normalize(); clone.Reduce(); return clone; } }
        
        public Fraction Negated => Improper.Negated;
        public Fraction Inversed => Improper.Inversed;
        
        public int Floored => Improper.Floored;
        public int Rounded => Improper.Rounded;
        public int Ceiling => Improper.Ceiling;

        public static Fraction operator-(Fraction x) => x.Negated;
        
        public static Fraction operator+(Fraction x, Fraction y) => x.Improper + y.Improper;
        public static Fraction operator-(Fraction x, Fraction y) => x.Improper - y.Improper;
        public static Fraction operator*(Fraction x, Fraction y) => x.Improper * y.Improper;
        public static Fraction operator/(Fraction x, Fraction y) => x.Improper / y.Improper;
        
        public static Fraction operator+(Fraction x, int value) => Valid(x._int + value, x._num, x._den);
        public static Fraction operator-(Fraction x, int value) => Valid(x._int - value, x._num, x._den);
        public static Fraction operator*(Fraction x, int value) => Normal(x._int * value, x._num * value, x._den);
        public static Fraction operator/(Fraction x, int value) => Normal(0, x._int * x._den + x._num, x._den * value);
        
        public static Fraction operator+(int value, Fraction x) => Valid(x._int + value, x._num, x._den);
        public static Fraction operator-(int value, Fraction x) => Valid(x._int - value, x._num, x._den);
        public static Fraction operator*(int value, Fraction x) => Normal(x._int * value, x._num * value, x._den);
        public static Fraction operator/(int value, Fraction x) => Normal(0, x._den * value, x._num + x._int * x._den); // == Normalize(1 / x.Simple);

        public static Fraction Max(Fraction x, Fraction y) => x > y ? x : y;
        public static Fraction Min(Fraction x, Fraction y) => x < y ? x : y;

        public int CompareTo(Fraction other) {
            long x = _den == 0 ? long.MaxValue : ((long)_int * _den + _num) * other._den;
            long y = other._den == 0 ? long.MaxValue : ((long)other._int * other._den + other._num) * _den;
            return x.CompareTo(y);
        }

        public bool Equals(Fraction x) => CompareTo(x) == 0;
        public static bool operator<(Fraction x, Fraction y) => x._den != 0 && y._den != 0 && x.CompareTo(y) < 0;
        public static bool operator>(Fraction x, Fraction y) => x._den != 0 && y._den != 0 && x.CompareTo(y) > 0;
        public static bool operator<=(Fraction x, Fraction y) => x._den != 0 && y._den != 0 && x.CompareTo(y) <= 0;
        public static bool operator>=(Fraction x, Fraction y) => x._den != 0 && y._den != 0 && x.CompareTo(y) >= 0;
        public static bool operator==(Fraction x, Fraction y) => x._den != 0 && y._den != 0 && x.CompareTo(y) == 0;
        public static bool operator!=(Fraction x, Fraction y) => x._den != 0 && y._den != 0 && x.CompareTo(y) != 0;

        public override bool Equals(object obj) => obj is Fraction other && Equals(other);
        public override int GetHashCode() => Improper.GetHashCode();
        
        public static Fraction New(int integer, int numerator, int denominator) => new(integer, numerator, denominator);
        public static Fraction Valid(int integer, int numerator, int denominator) => denominator == 0 ? invalid : new(integer, numerator, denominator);
        public static Fraction Normal(int integer, int numerator, int denominator) => new Fraction(integer, numerator, denominator).Normalized;
        public static Fraction Reduce(int integer, int numerator, int denominator) => new Fraction(integer, numerator, denominator).Reduced;
        public static Fraction Simple(int integer, int numerator, int denominator) => new Fraction(integer, numerator, denominator).Simplified;
        
        public static Fraction Division(int division) => new(0, 1, division);
        
        /// <summary>
        /// 用连分数法寻找分母在含 maxDenominator 以内离 value 最近的分数
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="maxDenominator"> Maximum denominator that can be produced </param>
        public static Fraction FromFloat(double value, int maxDenominator = 1000) => FractionSimple.FromFloat(value, maxDenominator);

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator.
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="denominator"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode as used in MathUtil.RoundToInt </param>
        public static Fraction FromFloatRounded(double value, int denominator, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            return FractionSimple.FromFloatRounded(value, denominator, mode);
        }

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator, with integer and decimal parts separated.
        /// </summary>
        /// <param name="integerPart"> The integer part of the sum </param>
        /// <param name="decimalPart"> The decimal part of the sum </param>
        /// <param name="denominator"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode as used in MathUtil.RoundToInt </param>
        public static Fraction FromFloatRounded(int integerPart, float decimalPart, int denominator, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            return FromFloatRounded(decimalPart, denominator, mode) + new Fraction(integerPart);
        }

        public static Fraction FromIntArray(int[] arr) {
            return new Fraction(arr[0], arr[1], arr[2]);
        }

        public override string ToString() => $"{_int} {_num}/{_den}";
        public int[] ToArray() => new[] { _int, _num, _den };
    }
}