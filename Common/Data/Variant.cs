// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace MaTech.Common.Data {
    public enum VariantType {
        None = 0, Bool, Int, Float, Double, Fraction, FractionSimple, String, Object
    }

    /// A boolean, an integer, a float-point, a fraction, a string, an object, or nothing ("None" type).
    /// 
    /// String and Object typed values are non-null; null values are of None type.
    /// 
    /// Numeral types can be converted between each other.
    /// Every type can convert to a string by formatting or an object by boxing.
    ///
    /// The equality method compares the type and value strictly, not tolerant to float-point errors.
    ///
    /// TODO: 分裂出Number类，并移除对于string和object的支持
    /// TODO: 重构成新Variant类，支持4字节内任意类型的struct，用object成员记录类型
    [JsonConverter(typeof(VariantJsonConverter))]
    public readonly struct Variant : IEquatable<Variant>, IConvertible, IBoxlessConvertible, IFormattable {
        public VariantType Type { get; }

        private readonly FractionSimple f;
        private readonly double d;
        private readonly object o;

        public static Variant None => new Variant();

        private Variant(bool value) {
            Type = VariantType.Bool;
            f = new FractionSimple(value ? 1 : 0);
            d = f.Numerator;
            o = null;
        }

        private Variant(int value) {
            Type = VariantType.Int;
            f = new FractionSimple(value);
            d = f.Numerator;
            o = null;
        }

        private Variant(float value) {
            Type = VariantType.Float;
            f = FractionSimple.FromFloat(value);
            d = value;
            o = null;
        }

        private Variant(double value) {
            Type = VariantType.Double;
            f = FractionSimple.FromFloat(value);
            d = value;
            o = null;
        }

        private Variant(Fraction value) {
            Type = VariantType.Fraction;
            f = value;
            d = f.Double;
            o = null;
        }

        private Variant(FractionSimple value) {
            Type = VariantType.FractionSimple;
            f = value;
            d = f.Double;
            o = null;
        }

        private Variant(string value) {
            if (value == null) this = None;
            else {
                Type = VariantType.String;
                f = FractionSimple.invalid;
                d = Double.NaN;
                o = value;
            }
        }

        private Variant(object value) {
            if (value == null) this = None;
            else {
                // no type infer, object in object out
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
        public string String => IsString ? (string)o : null; // no conversion by default
        public object Object => IsObject ? o : null; // no conversion by default

        public bool IsNone => Type == VariantType.None;
        public bool IsBoolean => Type == VariantType.Bool;
        public bool IsInteger => Type == VariantType.Int;
        public bool IsFloatPoint => Type == VariantType.Float || Type == VariantType.Double;
        public bool IsFraction => Type == VariantType.Fraction || Type == VariantType.FractionSimple;
        public bool IsNumeral => IsInteger || IsFloatPoint || IsFraction;
        public bool IsNumeralOrBoolean => IsNumeral || IsBoolean;
        public bool IsString => Type == VariantType.String;
        public bool IsStringEmpty => IsString && string.IsNullOrEmpty((string)o);
        public bool IsStringWhiteSpace => IsString && string.IsNullOrWhiteSpace((string)o);
        public bool IsObject => Type == VariantType.Object;

        public T To<T>(IFormatProvider provider = null) => BoxlessConvert.To<T>.From(this, provider);

        public override string ToString() => ToString(null, null);

        public static Variant FromObject<T>(T value) where T : class => new Variant(value);
        public T ToObject<T>() where T : class => ToObject() as T;
        public T GetObject<T>() where T : class => Object as T;
        
        public static Variant Box<T>(T value) where T : struct => new Variant((object)value);
        public T Unbox<T>() where T : struct => Object is T t ? t : default;

        public static implicit operator Variant(bool value) => new Variant(value);
        public static implicit operator Variant(int value) => new Variant(value);
        public static implicit operator Variant(float value) => new Variant(value);
        public static implicit operator Variant(double value) => new Variant(value);
        public static implicit operator Variant(Fraction value) => new Variant(value);
        public static implicit operator Variant(FractionSimple value) => new Variant(value);
        public static implicit operator Variant(string value) => new Variant(value);

        private static readonly Type typeFraction = typeof(Fraction);
        private static readonly Type typeFractionSimple = typeof(FractionSimple);
        
        private static readonly HashSet<Type> typesConvertible = new HashSet<Type>() {
            typeof(bool),
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeFraction,
            typeFractionSimple,
        };

        bool IConvertible.ToBoolean(IFormatProvider provider) => Bool;

        sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte)Int;
        short IConvertible.ToInt16(IFormatProvider provider) => (short)Int;
        int IConvertible.ToInt32(IFormatProvider provider) => Int;
        long IConvertible.ToInt64(IFormatProvider provider) => Int;

        byte IConvertible.ToByte(IFormatProvider provider) => (byte)Int;
        ushort IConvertible.ToUInt16(IFormatProvider provider) => (ushort)Int;
        uint IConvertible.ToUInt32(IFormatProvider provider) => (uint)Int;
        ulong IConvertible.ToUInt64(IFormatProvider provider) => (ulong)Int;

        float IConvertible.ToSingle(IFormatProvider provider) => Float;
        double IConvertible.ToDouble(IFormatProvider provider) => Double;
        decimal IConvertible.ToDecimal(IFormatProvider provider) => (decimal)Double;

        string IConvertible.ToString(IFormatProvider provider) => ToString(null, provider);

        char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException("Variant: Conversion to char is undefined. Try a string or manually convert the charcode to a numeric type.");
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException("Variant: Conversion to DateTime is undefined. Try a string or a numeric timecode; choose wisely.");

        object IConvertible.ToType(Type type, IFormatProvider provider) {
            if (type == typeFraction) return Fraction;
            if (type == typeFractionSimple) return FractionSimple;
            if (IsObject) return Convert.ChangeType(o, type, provider);
            throw new InvalidCastException($"Variant: Conversion to type {type} is undefined.");
        }

        [Preserve]
        public static bool IsBoxlessConvertibleToType(Type type) => typesConvertible.Contains(type);
        
        T IBoxlessConvertible.ToType<T>(IFormatProvider provider) {
            var type = typeof(T);
            if (type == typeFraction) return BoxlessConvert.Identity<Fraction, T>(Fraction);
            if (type == typeFractionSimple) return BoxlessConvert.Identity<FractionSimple, T>(FractionSimple);
            if (IsObject) return (T)Convert.ChangeType(o, type, provider);
            return BoxlessConvert.To<T>.FromIConvertible(this, provider); // fallback to IConvertible
        }

        public TypeCode GetTypeCode() {
            switch (Type) {
            case VariantType.None: return TypeCode.Empty;
            case VariantType.Bool: return TypeCode.Boolean;
            case VariantType.Int: return TypeCode.Int32;
            case VariantType.Float: return TypeCode.Single;
            case VariantType.Double: return TypeCode.Double;
            case VariantType.String: return TypeCode.String;
            default: return TypeCode.Object; // Fraction, FractionSimple, Object
            }
        }

        public bool HasValueOfType(VariantType targetType, bool allowConversion = false) {
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
            case VariantType.String: return allowConversion;
            case VariantType.Object: return allowConversion;
            default: return false;
            }
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            switch (Type) {
            case VariantType.None: return "<None Variant>";
            case VariantType.Bool: return Bool.ToString(formatProvider);
            case VariantType.Int: return Int.ToString(format, formatProvider);
            case VariantType.Float: return Float.ToString(format, formatProvider);
            case VariantType.Double: return Double.ToString(format, formatProvider);
            case VariantType.Fraction: return Fraction.ToString();
            case VariantType.FractionSimple: return FractionSimple.ToString();
            case VariantType.String: return String;
            case VariantType.Object: return o is IFormattable of ? of.ToString(format, formatProvider) : o is IConvertible oc ? oc.ToString(formatProvider) : o.ToString();
            default: return "<Undefined Variant>";
            }
        }

        public object ToObject(bool avoidValueTypes = false) {
            if (avoidValueTypes) return o;
            switch (Type) {
            case VariantType.Bool: return Bool;
            case VariantType.Int: return Int;
            case VariantType.Float: return Float;
            case VariantType.Double: return Double;
            case VariantType.Fraction: return Fraction;
            case VariantType.FractionSimple: return FractionSimple;
            default: return o; // String and Object
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

        public override int GetHashCode() => GetHashCode(true);
        public int GetHashCode(bool withVariantType) {
            if (withVariantType) return HashCode.Combine(GetHashCode(false), (int)Type);
            switch (Type) {
            case VariantType.Bool: return Bool.GetHashCode();
            case VariantType.Int: return Int.GetHashCode();
            case VariantType.Float: return Float.GetHashCode();
            case VariantType.Double: return Double.GetHashCode();
            case VariantType.Fraction: return Fraction.GetHashCode();
            case VariantType.FractionSimple: return FractionSimple.GetHashCode();
            case VariantType.String:
            case VariantType.Object: return o.GetHashCode();
            default: return 0; // None
            }
        }

        public static bool operator==(Variant left, Variant right) => left.Equals(right);
        public static bool operator!=(Variant left, Variant right) => !left.Equals(right);
    }

    public class VariantJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Variant);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var variant = (Variant)value;
            switch (variant.Type) {
            case VariantType.None: writer.WriteNull(); break;
            case VariantType.Bool: writer.WriteValue(variant.Bool); break;
            case VariantType.Int: writer.WriteValue(variant.Int); break;
            case VariantType.Float: writer.WriteValue(variant.Float); break;
            case VariantType.Double: writer.WriteValue(variant.Double); break;
            case VariantType.Fraction: serializer.Serialize(writer, variant.Fraction); break;
            case VariantType.FractionSimple: serializer.Serialize(writer, variant.FractionSimple); break;
            case VariantType.String: writer.WriteValue(variant.String); break;
            case VariantType.Object: writer.WriteValue(variant.Object); break;
            default: writer.WriteNull(); break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var variant = (Variant?)existingValue ?? Variant.None;
            switch (reader.TokenType) {
            case JsonToken.Boolean: variant = serializer.Deserialize<bool>(reader); break;
            case JsonToken.Integer: variant = serializer.Deserialize<int>(reader); break;
            case JsonToken.Float: variant = serializer.Deserialize<double>(reader); break; // unfortunately we cannot distinguish between f32 & f64
            case JsonToken.String: variant = serializer.Deserialize<string>(reader); break;

            case JsonToken.Raw:
            case JsonToken.Date:
            case JsonToken.Bytes: variant = Variant.FromObject(serializer.Deserialize(reader)); break;

            case JsonToken.StartArray:
                var arr = FractionJsonConverter.ReadIntArrayForFraction(reader);
                switch (arr.Count) {
                case 2: variant = new FractionSimple(arr[0], arr[1]); break;
                case 3: variant = new Fraction(arr[0], arr[1], arr[2]); break;
                default: variant = Variant.None; break;
                }
                break;
            }
            return variant;
        }
    }
}