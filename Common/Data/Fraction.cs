// Copyright (c) 2023, LuiCat (as MaTech)
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
    [Serializable]
    [JsonConverter(typeof(FractionJsonConverter))]
    public struct Fraction : IComparable<Fraction>, IEquatable<Fraction> {
        // todo: make immutable? readonly struct cannot be serializable
        // todo: normalize on construct by default

        public static readonly Fraction invalid = new Fraction();
        public static readonly Fraction zero = new Fraction(0);
        public static readonly Fraction maxValue = new Fraction(int.MaxValue);
        public static readonly Fraction minValue = new Fraction(int.MinValue);

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

        public Fraction(int integer, FractionSimple decimalPart) {
            _int = integer;
            _num = decimalPart.Numerator;
            _den = decimalPart.Denominator;
        }

        public Fraction(int integer, int numerator, int denominator) {
            _int = integer;
            _num = numerator;
            _den = denominator;
        }

        public void Set(int integer, int numerator, int denominator) {
            _int = integer;
            _num = numerator;
            _den = denominator;
        }

        public void SetSigned(int integer, int numerator, int denominator) {
            _int = integer;
            _num = Math.Sign(denominator) * numerator;
            _den = Math.Abs(denominator);
        }

        public int Integer { get => _int; set => _int = value; }
        public int Numerator { get => _num; set => _num = value; }
        public int Denominator { get => _den; set => _den = value; }

        public float Float => _int + (float)_num / _den;
        public double Double => _int + (double)_num / _den;

        public FractionSimple Decimal => new FractionSimple(_num, _den);
        public float DecimalFloat => (float)_num / _den;
        public double DecimalDouble => (double)_num / _den;

        public static explicit operator float(Fraction fraction) => fraction.Float;
        public static explicit operator double(Fraction fraction) => fraction.Double;

        public static implicit operator FractionSimple(Fraction fraction) => new FractionSimple(fraction._num + fraction._int * fraction._den, fraction._den);
        public static implicit operator Fraction(FractionSimple fraction) => new Fraction(0, fraction.Numerator, fraction.Denominator).Normalized;

        public Fraction Normalized {
            get {
                if (_den == 0) {
                    return invalid;
                }
                Fraction result = this;
                result._int += _num / _den;
                result._num %= _den;
                if (result._num < 0) {
                    result._int--;
                    result._num += _den;
                }
                return result;
            }
        }
        public Fraction Reduced {
            get {
                if (_den == 0) {
                    return invalid;
                }
                int t = MathUtil.GCD(_num, _den);
                return new Fraction(_int, _num / t, _den / t);
            }
        }

        public static Fraction operator+(Fraction x, Fraction y) => (FractionSimple)x + (FractionSimple)y;
        public static Fraction operator-(Fraction x, Fraction y) => (FractionSimple)x - (FractionSimple)y;
        public static Fraction operator*(Fraction x, Fraction y) => (FractionSimple)x * (FractionSimple)y;
        public static Fraction operator/(Fraction x, Fraction y) => (FractionSimple)x / (FractionSimple)y;
        
        public static Fraction operator+(Fraction x, int value) {
            if (x._den == 0) return invalid;
            return new Fraction(x._int + value, x._num, x._den);
        }

        public static Fraction operator-(Fraction x, int value) {
            if (x._den == 0) return invalid;
            return new Fraction(x._int - value, x._num, x._den);
        }

        public static Fraction operator*(Fraction x, int scale) {
            if (x._den == 0) return invalid;
            return new Fraction(x._int * scale, x._num * scale, x._den).Normalized;
        }

        public static Fraction operator/(Fraction x, int denominator) {
            if (x._den == 0) return invalid;
            return new Fraction(0, x._int * x._den + x._num, x._den * denominator).Normalized;
        }

        public static Fraction Max(Fraction x, Fraction y) => x > y ? x : y;
        public static Fraction Min(Fraction x, Fraction y) => x < y ? x : y;

        public bool IsZero => _int == 0 && _num == 0;
        public bool IsValid => _den != 0;

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
        public override int GetHashCode() => ((FractionSimple)this).GetHashCode();

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

    public class FractionJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Fraction) || objectType == typeof(FractionSimple);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            switch (value) {
            case FractionSimple fs:
                serializer.Serialize(writer, fs.ToArray());
                break;
            case Fraction f:
                serializer.Serialize(writer, f.ToArray());
                break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return FractionOfType(objectType, FractionSimple.invalid);
            }

            var arrInt = ReadIntArrayForFraction(reader);
            if (arrInt.Count == 2) {
                var value = new FractionSimple(arrInt[0], arrInt[1]);
                return FractionOfType(objectType, value);
            }
            if (arrInt.Count == 3) {
                var value = new Fraction(arrInt[0], arrInt[1], arrInt[2]);
                return FractionOfType(objectType, value);
            }

            throw reader is IJsonLineInfo lineInfo ?
                new JsonSerializationException($"Cannot read beat value '{reader.Path}'. It needs to be an array of 2 or 3 integers.", reader.Path, lineInfo.LineNumber, lineInfo.LinePosition, null) :
                new JsonSerializationException($"Cannot read beat value '{reader.Path}'. It needs to be an array of 2 or 3 integers.");
        }

        private object FractionOfType(Type type, Fraction value) => type == typeof(Fraction) ? value : (FractionSimple)value;
        private object FractionOfType(Type type, FractionSimple value) => type == typeof(Fraction) ? (Fraction)value : value;

        public static List<int> ReadIntArrayForFraction([NotNull] JsonReader reader) {
            List<int> arrInt = new List<int>(3);

            reader.AssumeToken(JsonToken.StartArray);

            reader.ReadAndAssumeToken(JsonToken.Integer);
            Assert.IsNotNull(reader.Value);
            arrInt.Add((int)(long)reader.Value);

            reader.ReadAndAssumeToken(JsonToken.Integer);
            Assert.IsNotNull(reader.Value);
            arrInt.Add((int)(long)reader.Value);

            reader.ReadAndAssertSuccess();

            if (reader.TokenType == JsonToken.Integer) {
                Assert.IsNotNull(reader.Value);
                arrInt.Add((int)(long)reader.Value);
                reader.ReadAndAssumeToken(JsonToken.EndArray);
            } else {
                reader.AssumeToken(JsonToken.EndArray);
            }

            return arrInt;
        }
    }
}