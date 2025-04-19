// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using MaTech.Common.Algorithm;
using Newtonsoft.Json;
using Optional;
using UnityEngine.Scripting;

namespace MaTech.Common.Data {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class VariantType {
        public static readonly Type None = null;
        public static readonly Type Bool = typeof(bool);
        public static readonly Type Int = typeof(int);
        public static readonly Type Float = typeof(float);
        public static readonly Type Double = typeof(double);
        public static readonly Type Scalar = typeof(Scalar);
        public static readonly Type Enum = typeof(MetaEnum);
        public static readonly Type Mixed = typeof(FractionMixed);
        public static readonly Type Improper = typeof(FractionImproper);
        public static readonly Type String = typeof(string);
        public static readonly Type Object = typeof(object);
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
    /// todo: 支持16字节内任意类型的unmanaged类型，用object成员记录类型，并用IBoxlessConvert支持可扩展的默认转型
    /// todo: 支持IMeta/IMetaVisitable接口，提取类型信息与相应转型后数据
    /// todo: 实现序列化与编辑器支持（先行决定是否实现unmanaged支持）
    /// todo: 在需要readonly struct的地方提供基于Variant的额外编辑器支持
    /// -->
    [Serializable]
    [JsonConverter(typeof(JsonConverter))]
    public partial struct Variant : IEquatable<Variant>, IConvertible, IBoxlessConvertible, IFormattable {
        public readonly Type Type => o as Type ?? o.GetType();

        private Scalar s; // todo: 与UnmanagedMemory合并
        private object o;

        /*
        [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 16), Serializable]
        private struct UnmanagedMemory {
            static UnmanagedMemory() => Assert.IsTrue(Marshal.SizeOf<UnmanagedMemory>() <= 16);
            
            // todo: implement with actual unsafe to avoid issues in Unity 2021

            public const int MaxSize = 16;
            public static bool Fit<T>() => Unsafe.SizeOf<T>() <= MaxSize && !RuntimeHelpers.IsReferenceOrContainsReferences<T>();

            public readonly T? As<T>() where T : unmanaged => Fit<T>() ? Unsafe.As<UnmanagedMemory, T>(readonly ref this) : null;
            public void Set<T>(in T value) where T : unmanaged { if (Fit<T>()) Unsafe.As<UnmanagedMemory, T>(ref this) = value; }

            [SerializeField, HideInInspector, FieldOffset(0)] private int pack0;
            [SerializeField, HideInInspector, FieldOffset(4)] private int pack1;
            [SerializeField, HideInInspector, FieldOffset(8)] private int pack2;
            [SerializeField, HideInInspector, FieldOffset(12)] private int pack3;
        }
        */

        public static Variant None => default;

        public readonly bool Bool => IsNumeralOrBoolean ? !s.IsZero : (o != null);
        public readonly int Int => s;
        public readonly float Float => s;
        public readonly double Double => s;
        public readonly Scalar Scalar => s;
        public readonly FractionMixed Mixed => s;
        public readonly FractionImproper Improper => s;
        public readonly string String => o as string;
        public readonly object Object => o;

        public readonly Option<bool> AsBool => Type == VariantType.Bool ? Option.Some(!s.IsZero) : Option.None<bool>();
        public readonly Option<int> AsInt => Type == VariantType.Int ? Option.Some(Int) : Option.None<int>();
        public readonly Option<float> AsFloat => Type == VariantType.Float ? Option.Some(Float) : Option.None<float>();
        public readonly Option<double> AsDouble => Type == VariantType.Double ? Option.Some(Double) : Option.None<double>();
        public readonly Option<MetaEnum> AsEnum => Type == VariantType.Enum ? Option.Some(ToEnum()) : Option.None<MetaEnum>();
        public readonly Option<FractionMixed> AsMixed => Type == VariantType.Mixed ? Option.Some(Mixed) : Option.None<FractionMixed>();
        public readonly Option<FractionImproper> AsImproper => Type == VariantType.Improper ? Option.Some(Improper) : Option.None<FractionImproper>();
        public readonly Option<string> AsString => Type == VariantType.String ? Option.Some((string)o) : Option.None<string>();
        public readonly Option<object> AsObject => Type == VariantType.Object ? Option.Some(o) : Option.None<object>();

        public readonly bool IsNone => o is null; // o is either valid object or a Type
        public readonly bool IsSome => o is not null;
        public readonly bool IsScalar => Type == VariantType.Scalar;
        public readonly bool IsBoolean => Type == VariantType.Bool;
        public readonly bool IsInteger => Type == VariantType.Int;
        public readonly bool IsFloat => Type == VariantType.Float;
        public readonly bool IsDouble => Type == VariantType.Double;
        public readonly bool IsFloatPoint => Type == VariantType.Float || Type ==  VariantType.Double;
        public readonly bool IsEnum => Type == VariantType.Enum;
        public readonly bool IsMixed => Type == VariantType.Mixed;
        public readonly bool IsImproper => Type == VariantType.Improper;
        public readonly bool IsFraction => Type == VariantType.Mixed || Type ==  VariantType.Improper;
        public readonly bool IsNumeral => IsScalar || IsInteger || IsFloatPoint || IsFraction;
        public readonly bool IsNumeralOrBoolean => IsNumeral || IsBoolean;
        public readonly bool IsString => Type == VariantType.String;
        public readonly bool IsStringEmpty => IsString && string.IsNullOrEmpty((string)o);
        public readonly bool IsStringWhiteSpace => IsString && string.IsNullOrWhiteSpace((string)o);
        public readonly bool IsObject => Type == VariantType.Object;

        public readonly override string ToString() => ToString(null, null);
        
        public static Variant FromEnum(MetaEnum value) => new(value);
        public static Variant FromEnum<TEnum>(DataEnum<TEnum> value) where TEnum : unmanaged, Enum, IConvertible => new(MetaEnum.FromEnum(value));
        public static Variant FromEnum<TEnum>(TEnum value) where TEnum : unmanaged, Enum, IConvertible => new(MetaEnum.FromEnum(value));
        public readonly MetaEnum ToEnum() => IsEnum ? MetaEnum.FromValue((string)o, Int) : MetaEnum.Empty;
        public readonly DataEnum<TEnum>? ToEnum<TEnum>() where TEnum : unmanaged, Enum, IConvertible => ToEnum().As<TEnum>();
        
        public static Variant From(object value) => new(value); // note: null resolves into Variant.None
        public static Variant Box<T>(T value) => new(value); // note: Nullable<T> is also boxed; can be handled with To<T> conversion
        
        public readonly bool Is<T>() => o is T;
        
        public readonly Option<T> As<T>() => o is T t ? t.Some() : Option.None<T>();
        public readonly T To<T>() => o is T t ? t : default;

        public static implicit operator Variant(bool value) => new(value ? 0 : 1, typeBool);
        public static implicit operator Variant(int value) => new(value, typeInt);
        public static implicit operator Variant(float value) => new(value, typeFloat);
        public static implicit operator Variant(double value) => new(value, typeDouble);
        public static implicit operator Variant(Scalar value) => new(value, typeScalar);
        public static implicit operator Variant(MetaEnum value) => new(value.ID, typeEnum);
        public static implicit operator Variant(FractionMixed value) => new(value, typeMixed);
        public static implicit operator Variant(FractionImproper value) => new(value, typeImproper);
        public static implicit operator Variant(string value) => new(value); // note: also handles null

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
            if (type == typeMixed) return Mixed;
            if (type == typeImproper) return Improper;
            if (type == typeScalar) return Scalar;
            if (type == typeEnum) return ToEnum();
            if (IsObject) return Convert.ChangeType(o, type, provider);
            throw new InvalidCastException($"Variant: Conversion to type {type} is unsupported.");
        }

        [Preserve]
        public static bool IsBoxlessConvertibleToType(Type type) => typesConvertible.Contains(type);
        
        readonly T IBoxlessConvertible.ToType<T>(IFormatProvider provider) {
            var type = typeof(T);
            if (type == typeMixed) return BoxlessConvert.Identity<FractionMixed, T>(Mixed);
            if (type == typeImproper) return BoxlessConvert.Identity<FractionImproper, T>(Improper);
            if (type == typeScalar) return BoxlessConvert.Identity<Scalar, T>(Scalar);
            if (type == typeEnum) return BoxlessConvert.Identity<MetaEnum, T>(ToEnum());
            // todo: 如何支持DataEnum<T>？是否在BoxlessConvert中实现这一操作？
            return BoxlessConvert.To<T>.FromIConvertible(this, provider); // fallback to IConvertible
        }
        
        public readonly TypeCode GetTypeCode() {
            var type = Type;
            if (type == typeBool) return TypeCode.Boolean;
            if (type == typeInt) return TypeCode.Int32;
            if (type == typeFloat) return TypeCode.Single;
            if (type == typeDouble) return TypeCode.Double;
            if (type == typeEnum) return TypeCode.Int32; // same as MetaEnum
            if (o is not System.Type) return o is string ? TypeCode.String : TypeCode.Object;
            return TypeCode.Empty; // all other non-IConvertible-trivial types
        }

        public readonly string ToString(string format, IFormatProvider formatProvider) {
            var type = Type;
            if (type == VariantType.None) return "<None Variant>";
            if (type == VariantType.Bool) return Bool.ToString(formatProvider);
            if (type == VariantType.Int) return Int.ToString(format, formatProvider);
            if (type == VariantType.Float) return Float.ToString(format, formatProvider);
            if (type == VariantType.Double) return Double.ToString(format, formatProvider);
            if (type == VariantType.Enum) return ToEnum().ToString();
            if (type == VariantType.Mixed) return Mixed.ToString();
            if (type == VariantType.Improper) return Improper.ToString();
            if (type == VariantType.String) return (string)o;
            if (type == VariantType.Object) return o switch {
                IFormattable formattable => formattable.ToString(format, formatProvider),
                IConvertible oc => oc.ToString(formatProvider),
                _ => o.ToString()
            };
            return "<Undefined Variant>";
        }

        public readonly object ToObject(bool avoidValueTypes = false) {
            if (avoidValueTypes) return o;
            var type = Type;
            if (type == VariantType.Bool) return Bool;
            if (type == VariantType.Int) return Int;
            if (type == VariantType.Float) return Float;
            if (type == VariantType.Double) return Double;
            if (type == VariantType.Enum) return ToEnum();
            if (type == VariantType.Mixed) return Mixed;
            if (type == VariantType.Improper) return Improper;
            return o; // String and Object
        }

        public readonly bool Equals(Variant other) {
            var type = Type;
            if (type != other.Type) return false;
            if (type == VariantType.None) return true;
            if (type == VariantType.Bool) return Bool.Equals(other.Bool);
            if (type == VariantType.Int) return Int.Equals(other.Int);
            if (type == VariantType.Float) return Float.Equals(other.Float);
            if (type == VariantType.Double) return Double.Equals(other.Double);
            if (type == VariantType.Enum) return ToEnum().Equals(other.ToEnum());
            if (type == VariantType.Mixed) return Mixed.Equals(other.Mixed);
            if (type == VariantType.Improper) return Improper.Equals(other.Improper);
            return Equals(o, other.o);
        }

        public override readonly bool Equals(object obj) {
            return obj is Variant other && Equals(other);
        }

        public override readonly int GetHashCode() => GetHashCode(true);
        public readonly int GetHashCode(bool withVariantType) {
            if (withVariantType) return HashCode.Combine(GetHashCode(false), Type);
            var type = Type;
            if (type == VariantType.Bool) return Bool.GetHashCode();
            if (type == VariantType.Int) return Int.GetHashCode();
            if (type == VariantType.Float) return Float.GetHashCode();
            if (type == VariantType.Double) return Double.GetHashCode();
            if (type == VariantType.Enum) return ToEnum().GetHashCode();
            if (type == VariantType.Mixed) return Mixed.GetHashCode();
            if (type == VariantType.Improper) return Improper.GetHashCode();
            return o?.GetHashCode() ?? 0; // None
        }

        public static bool operator==(Variant left, Variant right) => left.Equals(right);
        public static bool operator!=(Variant left, Variant right) => !left.Equals(right);
    }
}