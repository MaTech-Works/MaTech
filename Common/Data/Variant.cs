// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MaTech.Common.Algorithm;
using Newtonsoft.Json;
using Optional;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Scripting;

namespace MaTech.Common.Data {
    public enum VariantType {
        None = 0, Bool, Int, Float, Double, Enum, Fraction, FractionSimple, String, Object
    }

    /// A boolean, an integer, a float-point, a fraction, a string, an object, or nothing ("None" type).
    /// <br/>
    /// String and Object typed values are non-null; null values are of None type.
    /// Numeral types can be converted between each other.
    /// Every type can convert to a string via formatting, as well as to an object by boxing or taken as-is.
    /// <br/>
    /// The equality method compares the type and value strictly, not tolerant to float-point errors.
    /// 
    /// <!--
    /// todo: 是否支持常见类型的反向转型？
    /// todo: 提取一个Rational结构（Fraction + fixed point decimal），将数字转型方法移动过去并支持转型至各标量类型
    /// todo: 支持16字节内任意类型的unmanaged类型，用object成员记录类型，并用IBoxlessConvert支持可扩展的默认转型
    /// todo: 支持IMeta/IMetaVisitable接口，提取类型信息与相应转型后数据
    /// todo: 实现序列化与编辑器支持（思考：在需要readonly struct的地方提供基于Variant的额外编辑器支持）
    /// todo: 实现Box<T>与静态类Boxer<T>重用箱子，缓存每种类型的装箱拆箱过程（参数传入整个Variant，无箱struct给一个全局类型信息，object原样传出）
    /// -->
    [Serializable]
    [JsonConverter(typeof(JsonConverter))]
    public partial struct Variant : IEquatable<Variant>, IConvertible, IBoxlessConvertible, IFormattable {
        public VariantType Type { get; private set; }

        private FractionSimple f;
        private double d;
        private object o;

        [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 16)]
        private struct UnmanagedMemory {
            static UnmanagedMemory() => Assert.IsTrue(Marshal.SizeOf<UnmanagedMemory>() <= 4);

            public const int MaxSize = 16;
            public static bool Fit<T>() => Unsafe.SizeOf<T>() <= MaxSize && !RuntimeHelpers.IsReferenceOrContainsReferences<T>();

            public T? As<T>() where T : unmanaged => Fit<T>() ? Unsafe.As<UnmanagedMemory, T>(ref this) : null;
            public void Set<T>(in T value) where T : unmanaged { if (Fit<T>()) Unsafe.As<UnmanagedMemory, T>(ref this) = value; }
            
            // todo: make serializable (add hidden fields) and apply to Variant
        }

        public static Variant None => default;
        
        // todo: rename getters to ToXX and remove try- prefix on optional getters

        public readonly bool Bool => IsNumeralOrBoolean ? !f.IsZero : (o != null);
        public readonly int Int => f.Rounded;
        public readonly float Float => (float)d;
        public readonly double Double => d;
        public readonly MetaEnum Enum => IsEnum ? MetaEnum.FromValue((string)o, f.Numerator) : MetaEnum.Empty;
        public readonly Fraction Fraction => f;
        public readonly FractionSimple FractionSimple => f;
        public readonly string String => o as string;
        public readonly object Object => o;

        public readonly Option<bool> TryBool => Type == VariantType.Bool ? Option.Some(!f.IsZero) : Option.None<bool>();
        public readonly Option<int> TryInt => Type == VariantType.Int ? Option.Some(f.Numerator) : Option.None<int>();
        public readonly Option<float> TryFloat => Type == VariantType.Float ? Option.Some((float)d) : Option.None<float>();
        public readonly Option<double> TryDouble => Type == VariantType.Double ? Option.Some(d) : Option.None<double>();
        public readonly Option<MetaEnum> TryEnum => Type == VariantType.Enum ? Option.Some(ToEnum()) : Option.None<MetaEnum>();
        public readonly Option<Fraction> TryFraction => Type == VariantType.Fraction ? Option.Some((Fraction)f) : Option.None<Fraction>();
        public readonly Option<FractionSimple> TryFractionSimple => Type == VariantType.FractionSimple ? Option.Some(f) : Option.None<FractionSimple>();
        public readonly Option<string> TryString => Type == VariantType.String ? Option.Some((string)o) : Option.None<string>();
        public readonly Option<object> TryObject => Type == VariantType.Object ? Option.Some(o) : Option.None<object>();

        public readonly bool IsNone => Type is VariantType.None;
        public readonly bool IsSome => Type is not VariantType.None;
        public readonly bool IsBoolean => Type is VariantType.Bool;
        public readonly bool IsInteger => Type is VariantType.Int;
        public readonly bool IsFloat => Type is VariantType.Float;
        public readonly bool IsDouble => Type is VariantType.Double;
        public readonly bool IsFloatPoint => Type is VariantType.Float or VariantType.Double;
        public readonly bool IsEnum => Type is VariantType.Enum;
        public readonly bool IsFraction => Type is VariantType.Fraction or VariantType.FractionSimple;
        public readonly bool IsNumeral => IsInteger || IsFloatPoint || IsFraction;
        public readonly bool IsNumeralOrBoolean => IsNumeral || IsBoolean;
        public readonly bool IsString => Type is VariantType.String;
        public readonly bool IsStringEmpty => IsString && string.IsNullOrEmpty((string)o);
        public readonly bool IsStringWhiteSpace => IsString && string.IsNullOrWhiteSpace((string)o);
        public readonly bool IsObject => Type is VariantType.Object;

        public readonly override string ToString() => ToString(null, null);

        public static Variant FromEnum(MetaEnum value) => new(value);
        public static Variant FromEnum<TEnum>(DataEnum<TEnum> value) where TEnum : unmanaged, Enum, IConvertible => new(MetaEnum.FromEnum(value));
        public static Variant FromEnum<TEnum>(TEnum value) where TEnum : unmanaged, Enum, IConvertible => new(MetaEnum.FromEnum(value));
        public readonly MetaEnum ToEnum() => IsEnum ? MetaEnum.FromValue((string)o, f.Numerator) : MetaEnum.Empty;
        public readonly DataEnum<TEnum>? ToEnum<TEnum>() where TEnum : unmanaged, Enum, IConvertible => ToEnum().As<TEnum>();
        
        public static Variant From<T>(T value) where T : class => new(value);
        public readonly T As<T>() where T : class => o as T;
        
        // todo: in theory we can cache boxer func for each type and hide boxing process, in the end not having separate method for generic struct and class
        // todo: do we immediately recycle the box upon Unbox<T>? provide Ref<T> and To<T> in this case
        public static Variant Box<T>(T value) where T : struct => new(value);
        public readonly T Unbox<T>() where T : struct => o is T t ? t : default;
        
        // todo: a nested Box<T> and thread local reuse boxes and remove boxed methods?
        // todo: a ref method, how to do it for class, mixed struct, 16-byte unmanaged struct, and trivial types all?
        //public ref T Ref<T>() { } 
        
        // todo: a To<T> method with BoxlessConvert
        //public Optional<T> To<T>() { } 

        public static implicit operator Variant(bool value) => new(value);
        public static implicit operator Variant(int value) => new(value);
        public static implicit operator Variant(float value) => new(value);
        public static implicit operator Variant(double value) => new(value);
        public static implicit operator Variant(MetaEnum value) => new(value);
        public static implicit operator Variant(Fraction value) => new(value);
        public static implicit operator Variant(FractionSimple value) => new(value);
        public static implicit operator Variant(string value) => new(value);

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
            // TODO: 支持enum
            if (IsObject) return Convert.ChangeType(o, type, provider);
            throw new InvalidCastException($"Variant: Conversion to type {type} is unsupported.");
        }

        [Preserve]
        public static bool IsBoxlessConvertibleToType(Type type) => typesConvertible.Contains(type);
        
        readonly T IBoxlessConvertible.ToType<T>(IFormatProvider provider) {
            var type = typeof(T);
            if (type == typeFraction) return BoxlessConvert.Identity<Fraction, T>(Fraction);
            if (type == typeFractionSimple) return BoxlessConvert.Identity<FractionSimple, T>(FractionSimple);
            // TODO: 支持enum
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
            case VariantType.Enum: return TypeCode.Int32; // same as MetaEnum
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
            case VariantType.Enum: return Enum.ToString(format, formatProvider);
            case VariantType.Fraction: return Fraction.ToString();
            case VariantType.FractionSimple: return FractionSimple.ToString();
            case VariantType.String: return (string)o;
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
            case VariantType.Enum: return Enum;
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
            case VariantType.Enum: return Enum.Equals(other.Enum);
            case VariantType.Fraction: return Fraction.Equals(other.Fraction);
            case VariantType.FractionSimple: return FractionSimple.Equals(other.FractionSimple);
            case VariantType.String:
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
            case VariantType.Enum: return Enum.GetHashCode();
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