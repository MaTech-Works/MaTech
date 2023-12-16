// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using Newtonsoft.Json;
using Sirenix.Serialization;
using UnityEngine;
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
    [Serializable]
    [JsonConverter(typeof(Variant.JsonConverter))]
    public partial struct Variant : IEquatable<Variant>, IConvertible, IBoxlessConvertible, IFormattable {
        [field: SerializeField]
        public VariantType Type { get; private set; }

        [SerializeField] private FractionSimple f;
        [SerializeField] private double d;
        [OdinSerialize] private object o;

        public static Variant None => new Variant();

        public readonly bool Bool => IsNumeralOrBoolean ? !f.IsZero : (o != null);
        public readonly int Int => f.Rounded;
        public readonly float Float => (float)d;
        public readonly double Double => d;
        public readonly Fraction Fraction => f;
        public readonly FractionSimple FractionSimple => f;
        public readonly string String => IsString ? (string)o : null; // no conversion by default
        public readonly object Object => IsObject ? o : null; // no conversion by default

        public readonly bool IsNone => Type == VariantType.None;
        public readonly bool IsBoolean => Type == VariantType.Bool;
        public readonly bool IsInteger => Type == VariantType.Int;
        public readonly bool IsFloatPoint => Type == VariantType.Float || Type == VariantType.Double;
        public readonly bool IsFraction => Type == VariantType.Fraction || Type == VariantType.FractionSimple;
        public readonly bool IsNumeral => IsInteger || IsFloatPoint || IsFraction;
        public readonly bool IsNumeralOrBoolean => IsNumeral || IsBoolean;
        public readonly bool IsString => Type == VariantType.String;
        public readonly bool IsStringEmpty => IsString && string.IsNullOrEmpty((string)o);
        public readonly bool IsStringWhiteSpace => IsString && string.IsNullOrWhiteSpace((string)o);
        public readonly bool IsObject => Type == VariantType.Object;

        public readonly T To<T>(IFormatProvider provider = null) => BoxlessConvert.To<T>.From(this, provider);

        public override readonly string ToString() => ToString(null, null);

        public static Variant FromObject<T>(T value) where T : class => new Variant(value);
        public readonly T ToObject<T>() where T : class => ToObject() as T;
        public readonly T GetObject<T>() where T : class => Object as T;
        
        public static Variant Box<T>(T value) where T : struct => new Variant((object)value);
        public readonly T Unbox<T>() where T : struct => Object is T t ? t : default;

        public static implicit operator Variant(bool value) => new Variant(value);
        public static implicit operator Variant(int value) => new Variant(value);
        public static implicit operator Variant(float value) => new Variant(value);
        public static implicit operator Variant(double value) => new Variant(value);
        public static implicit operator Variant(Fraction value) => new Variant(value);
        public static implicit operator Variant(FractionSimple value) => new Variant(value);
        public static implicit operator Variant(string value) => new Variant(value);

        readonly bool IConvertible.ToBoolean(IFormatProvider provider) => Bool;

        readonly sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte)Int;
        readonly short IConvertible.ToInt16(IFormatProvider provider) => (short)Int;
        readonly int IConvertible.ToInt32(IFormatProvider provider) => Int;
        readonly long IConvertible.ToInt64(IFormatProvider provider) => Int;

        readonly byte IConvertible.ToByte(IFormatProvider provider) => (byte)Int;
        readonly ushort IConvertible.ToUInt16(IFormatProvider provider) => (ushort)Int;
        readonly uint IConvertible.ToUInt32(IFormatProvider provider) => (uint)Int;
        readonly ulong IConvertible.ToUInt64(IFormatProvider provider) => (ulong)Int;

        readonly float IConvertible.ToSingle(IFormatProvider provider) => Float;
        readonly double IConvertible.ToDouble(IFormatProvider provider) => Double;
        readonly decimal IConvertible.ToDecimal(IFormatProvider provider) => (decimal)Double;

        readonly string IConvertible.ToString(IFormatProvider provider) => ToString(null, provider);

        readonly char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException("Variant: Conversion to char is undefined. Try a string or manually convert the charcode to a numeric type.");
        readonly DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException("Variant: Conversion to DateTime is undefined. Try a string or a numeric timecode; choose wisely.");

        readonly object IConvertible.ToType(Type type, IFormatProvider provider) {
            if (type == typeFraction) return Fraction;
            if (type == typeFractionSimple) return FractionSimple;
            if (IsObject) return Convert.ChangeType(o, type, provider);
            throw new InvalidCastException($"Variant: Conversion to type {type} is undefined.");
        }

        [Preserve]
        public static bool IsBoxlessConvertibleToType(Type type) => typesConvertible.Contains(type);
        
        readonly T IBoxlessConvertible.ToType<T>(IFormatProvider provider) {
            var type = typeof(T);
            if (type == typeFraction) return BoxlessConvert.Identity<Fraction, T>(Fraction);
            if (type == typeFractionSimple) return BoxlessConvert.Identity<FractionSimple, T>(FractionSimple);
            if (IsObject) return (T)Convert.ChangeType(o, type, provider);
            return BoxlessConvert.To<T>.FromIConvertible(this, provider); // fallback to IConvertible
        }

        public readonly TypeCode GetTypeCode() {
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

        public readonly bool HasValueOfType(VariantType targetType, bool allowConversion = false) {
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

        public readonly string ToString(string format, IFormatProvider formatProvider) {
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

        public readonly object ToObject(bool avoidValueTypes = false) {
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

        public readonly bool Equals(Variant other) {
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

        public override readonly bool Equals(object obj) {
            return obj is Variant other && Equals(other);
        }

        public override readonly int GetHashCode() => GetHashCode(true);
        public readonly int GetHashCode(bool withVariantType) {
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

}