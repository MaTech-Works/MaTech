// Copyright (c) 2022, LuiCat (as MaTech)
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

namespace MaTech.Common.Algorithm {
    [Serializable]
    [JsonConverter(typeof(FractionJsonConverter))]
    public struct Fraction : IComparable<Fraction>, IEquatable<Fraction> {
        public static readonly Fraction invalid = new Fraction();
        public static readonly Fraction zero = new Fraction(0);
        public static readonly Fraction maxValue = new Fraction(int.MaxValue);
        public static readonly Fraction minValue = new Fraction(int.MinValue);

        #if UNITY_EDITOR
        static Fraction() {
            Assert.IsFalse(default(Fraction).IsValid);
        }
        #endif

        // ReSharper disable InconsistentNaming
        [SerializeField]
        private int _int, _num, _den;
        // ReSharper restore InconsistentNaming

        public Fraction(int value) {
            _int = value;
            _num = 0;
            _den = 1;
        }

        public Fraction(int integer, int numer, int denom) {
            _int = integer;
            _num = numer;
            _den = denom;
        }

        public void Set(int integer, int numer, int denom) {
            _int = integer;
            _num = numer;
            _den = denom;
        }

        public void SetSigned(int integer, int numer, int denom) {
            _int = integer;
            _num = Math.Sign(denom) * numer;
            _den = Math.Abs(denom);
        }

        public int Int { get => _int; set => _int = value; }
        public int Numer { get => _num; set => _num = value; }
        public int Denom { get => _den; set => _den = value; }

        public float Float => _int + (float)_num / _den;
        public double Double => _int + (double)_num / _den;
        public float Float01 => (float)_num / _den;
        public double Double01 => (double)_num / _den;

        public static explicit operator float(Fraction frac) { return frac.Float; }
        public static explicit operator double(Fraction frac) { return frac.Double; }

        public static implicit operator FractionSimple(Fraction frac) { return new FractionSimple(frac._num + frac._int * frac._den, frac._den); }
        public static implicit operator Fraction(FractionSimple frac) {
            var result = new Fraction(0, frac.Numer, frac.Denom);
            result.Normalize();
            return result;
        }

        public void Normalize() {
            if (_den == 0) {
                this = invalid;
                return;
            }
            _int += _num / _den;
            _num %= _den;
            if (_num < 0) {
                _int--;
                _num += _den;
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

        public Fraction Normalized {
            get {
                Fraction result = this;
                result.Normalize();
                return result;
            }
        }
        public Fraction Reduced {
            get {
                Fraction result = this;
                result.Reduce();
                return result;
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
            Fraction result = new Fraction(x._int * scale, x._num * scale, x._den);
            result.Normalize();
            return result;
        }

        public static Fraction operator/(Fraction x, int denom) {
            if (x._den == 0) return invalid;
            Fraction result = new Fraction(0, x._int * x._den + x._num, x._den * denom);
            result.Normalize();
            return result;
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
        /// 用连分数法寻找分母在 maxDenom 以内离 value 最近的分数
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="maxDenom"> Maximum denominator that can be produced </param>
        public static Fraction FromFloat(double value, int maxDenom = 1000) => FractionSimple.FromFloat(value, maxDenom);

        /// <summary>
        /// Get a normalized fraction from float-point values rounded to the denominator.
        /// </summary>
        /// <param name="value"> The float-point value </param>
        /// <param name="denom"> Denominator for rounding </param>
        /// <param name="mode"> Rounding mode as used in MathUtil.RoundToInt </param>
        public static Fraction FromFloatRounded(double value, int denom, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            int i = MathUtil.RoundToInt(value, mode);
            int numer = MathUtil.RoundToInt((value - i) * denom, mode);
            return numer < 0 ? new Fraction(i - 1, numer + denom, denom) : new Fraction(i, numer, denom);
        }

        /// <summary>
        /// Get a normalized fraction as the sum of a integer and a float-point values rounded to the denominator.
        /// By keeping the float value in [0, 1), this keeps the consistency of precisions of the fraction part of the value.
        /// </summary>
        /// <param name="valueInt"> The integer value in the sum </param>
        /// <param name="valueFloat"> The float-point value in the sum </param>
        /// <param name="denom"> Denominator for rounding </param>
        public static Fraction FromIntFloatRounded(int valueInt, double valueFloat, int denom) {
            int i = (int)Math.Round(valueFloat);
            int numer = (int)Math.Round((valueFloat - i) * denom);
            return numer < 0 ? new Fraction(valueInt + i - 1, numer + denom, denom) : new Fraction(valueInt + i, numer, denom);
        }

        public static Fraction FromIntArray(int[] arr) {
            return new Fraction(arr[0], arr[1], arr[2]);
        }

        public override string ToString() => $"{_int}:{_num}/{_den}";
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