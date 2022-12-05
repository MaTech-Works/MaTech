// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace MaTech.Common.Algorithm {
    public enum VariantType {
        None = 0, Bool, Int, Float, Double, Fraction, FractionSimple, String, Object
    }

    /// <summary>
    /// A boolean, an integer, a float-point, a fraction, a string, an object, or nothing ("None" type).
    /// 
    /// Numeral types can be casted between each other.
    /// Every type can convert to string.
    /// 
    /// String and Object values are non-null; a None value
    /// The equality method compares the data strictly, including float-point values.
    ///
    /// todo: implement IConvertible and conversion operators
    /// </summary>
    [JsonConverter(typeof(VariantJsonConverter))]
    public readonly struct Variant : IEquatable<Variant> {
        public VariantType Type { get; }

        private readonly FractionSimple f;
        private readonly double d;
        private readonly object o;

        public static Variant None => new Variant();

        public Variant(bool value) {
            Type = VariantType.Bool;
            f = new FractionSimple(value ? 1 : 0);
            d = f.Numer;
            o = null;
        }

        public Variant(int value) {
            Type = VariantType.Int;
            f = new FractionSimple(value);
            d = f.Numer;
            o = null;
        }

        public Variant(float value) {
            Type = VariantType.Float;
            f = FractionSimple.FromFloat(value);
            d = value;
            o = null;
        }

        public Variant(double value) {
            Type = VariantType.Double;
            f = FractionSimple.FromFloat(value);
            d = value;
            o = null;
        }

        public Variant(Fraction value) {
            Type = VariantType.Fraction;
            f = value;
            d = f.Double;
            o = null;
        }

        public Variant(FractionSimple value) {
            Type = VariantType.FractionSimple;
            f = value;
            d = f.Double;
            o = null;
        }

        public Variant(string value) {
            if (value == null) this = None;
            else {
                Type = VariantType.String;
                f = FractionSimple.invalid;
                d = Double.NaN;
                o = value;
            }
        }

        public Variant(object value) {
            if (value == null) this = None;
            else {
                Type = VariantType.Object;
                f = FractionSimple.invalid;
                d = Double.NaN;
                o = value;
            }
        }

        public bool Bool => IsNumeralOrBoolean ? !f.IsZero : (o != null);
        public int Int => f.Rounded;
        public float Float => (float)d;
        public double Double => d;
        public Fraction Fraction => f;
        public FractionSimple FractionSimple => f;
        public string String => ToString();

        public object Object {
            get {
                switch (Type) {
                case VariantType.Bool: return Bool;
                case VariantType.Int: return Int;
                case VariantType.Float: return Float;
                case VariantType.Double: return Double;
                case VariantType.Fraction: return Fraction;
                case VariantType.FractionSimple: return FractionSimple;
                case VariantType.String:
                case VariantType.Object: return o;
                default: return null;
                }
            }
        }

        public static implicit operator Variant(bool value) => new Variant(value);
        public static implicit operator Variant(int value) => new Variant(value);
        public static implicit operator Variant(float value) => new Variant(value);
        public static implicit operator Variant(double value) => new Variant(value);
        public static implicit operator Variant(Fraction value) => new Variant(value);
        public static implicit operator Variant(FractionSimple value) => new Variant(value);
        public static implicit operator Variant(string value) => new Variant(value);

        public T GetObject<T>() where T : class => Object as T;
        public static Variant FromObject<T>(T value) => new Variant(value);

        public bool IsNothing => Type == VariantType.None;
        public bool IsBoolean => Type == VariantType.Bool;
        public bool IsInteger => Type == VariantType.Int;
        public bool IsFloatPoint => Type == VariantType.Float || Type == VariantType.Double;
        public bool IsFraction => Type == VariantType.Fraction || Type == VariantType.FractionSimple;
        public bool IsNumeral => IsInteger || IsFloatPoint || IsFraction;
        public bool IsNumeralOrBoolean => IsNumeral || IsBoolean;
        public bool IsNone => Type == VariantType.None;
        public bool IsString => Type == VariantType.String;
        public bool IsStringEmpty => IsString && string.IsNullOrEmpty((string)o);
        public bool IsStringWhiteSpace => IsString && string.IsNullOrWhiteSpace((string)o);

        public bool CanConvertTo(VariantType targetType) {
            if (Type == targetType) return true;
            if (Type == VariantType.None) return true; // false, 0, 0.0, "<None Variant>", null
            switch (targetType) {
            case VariantType.None: return false;
            case VariantType.Bool: return true; // != 0, != null
            case VariantType.Int:
            case VariantType.Float:
            case VariantType.Double:
            case VariantType.Fraction:
            case VariantType.FractionSimple: return IsNumeralOrBoolean;
            case VariantType.String: return true;
            case VariantType.Object: return true; // null when not a string or object
            default: return false;
            }
        }

        public override string ToString() {
            switch (Type) {
            case VariantType.None: return "<None Variant>";
            case VariantType.Bool: return Bool ? "True" : "False";
            case VariantType.Int: return Int.ToString(CultureInfo.InvariantCulture);
            case VariantType.Float: return Float.ToString(CultureInfo.InvariantCulture);
            case VariantType.Double: return Double.ToString(CultureInfo.InvariantCulture);
            case VariantType.Fraction: return Fraction.ToString();
            case VariantType.FractionSimple: return FractionSimple.ToString();
            case VariantType.String: return (string)o;
            case VariantType.Object: return o.ToString();
            default: return "<Unknown Variant>";
            }
        }

        public bool Equals(Variant other) {
            if (Type != other.Type) return false;
            switch (Type) {
            case VariantType.None: return true;
            case VariantType.Bool: return Bool.Equals(other.Bool);
            case VariantType.Int: return Int.Equals(other.Int);
            case VariantType.Float: return Float.Equals(other.Float);
            case VariantType.Double: return Double.Equals(other.Double);
            case VariantType.Fraction: return Fraction.Equals(other.Fraction);
            case VariantType.FractionSimple: return FractionSimple.Equals(other.FractionSimple);
            case VariantType.String: return String.Equals(other.String);
            case VariantType.Object: return Equals(o, other.o);
            default: return false;
            }
        }

        public override bool Equals(object obj) {
            return obj is Variant other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode;
                switch (Type) {
                case VariantType.None:
                    hashCode = 0;
                    break;
                case VariantType.Bool:
                    hashCode = Bool.GetHashCode();
                    break;
                case VariantType.Int:
                    hashCode = Int.GetHashCode();
                    break;
                case VariantType.Float:
                    hashCode = Float.GetHashCode();
                    break;
                case VariantType.Double:
                    hashCode = Double.GetHashCode();
                    break;
                case VariantType.Fraction:
                    hashCode = Fraction.GetHashCode();
                    break;
                case VariantType.FractionSimple:
                    hashCode = FractionSimple.GetHashCode();
                    break;
                case VariantType.String:
                case VariantType.Object:
                    hashCode = o.GetHashCode();
                    break;
                default: return 0;
                }
                return  HashCode.Combine(hashCode, Type.GetHashCode());
            }
        }

        public static bool operator==(Variant left, Variant right) {
            return left.Equals(right);
        }

        public static bool operator!=(Variant left, Variant right) {
            return !left.Equals(right);
        }
    }

    public class VariantJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Variant);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var variant = (Variant)value;
            switch (variant.Type) {
            case VariantType.None:
                writer.WriteNull();
                break;
            case VariantType.Bool:
                writer.WriteValue(variant.Bool);
                break;
            case VariantType.Int:
                writer.WriteValue(variant.Int);
                break;
            case VariantType.Float:
                writer.WriteValue(variant.Float);
                break;
            case VariantType.Double:
                writer.WriteValue(variant.Double);
                break;
            case VariantType.Fraction:
                serializer.Serialize(writer, variant.Fraction);
                break;
            case VariantType.FractionSimple:
                serializer.Serialize(writer, variant.FractionSimple);
                break;
            case VariantType.String:
                writer.WriteValue(variant.String);
                break;
            case VariantType.Object:
                writer.WriteValue(variant.Object);
                break;
            default:
                writer.WriteNull();
                break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var variant = (Variant?)existingValue ?? Variant.None;
            switch (reader.TokenType) {
            case JsonToken.Boolean:
                variant = serializer.Deserialize<bool>(reader);
                break;
            case JsonToken.Integer:
                variant = serializer.Deserialize<int>(reader);
                break;
            case JsonToken.Float:
                variant = serializer.Deserialize<double>(reader);
                break; // unfortunately we cannot distinguish between f32 & f64
            case JsonToken.String:
                variant = serializer.Deserialize<string>(reader);
                break;

            case JsonToken.Raw:
            case JsonToken.Date:
            case JsonToken.Bytes:
                variant = Variant.FromObject(serializer.Deserialize(reader));
                break;

            case JsonToken.StartArray:
                var arr = FractionJsonConverter.ReadIntArrayForFraction(reader);
                switch (arr.Count) {
                case 2:
                    variant = new FractionSimple(arr[0], arr[1]);
                    break;
                case 3:
                    variant = new Fraction(arr[0], arr[1], arr[2]);
                    break;
                default:
                    variant = Variant.None;
                    break;
                }
                break;
            }
            return variant;
        }
    }
}