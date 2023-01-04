// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;

#nullable enable

namespace MaTech.Common.Algorithm {
    public readonly partial struct EnumEx<T>
        : IComparable, IComparable<EnumEx<T>>, IComparable<T>
        , IEquatable<EnumEx<T>>, IEquatable<T>
        , IBoxlessConvertible, IFormattable {
        
        public override string ToString() => ToString(null, null);
        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object? obj) {
            switch (obj) {
            case EnumEx<T> ex: return Equals(ex);
            case T e: return Equals(e);
            default: return false;
            }
        }

        public bool Equals(EnumEx<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);
        public bool Equals(T other) => EqualityComparer<T>.Default.Equals(Value, other);

        public int CompareTo(object? obj) {
            switch (obj) {
            case EnumEx<T> ex: return CompareTo(ex);
            case T e: return CompareTo(e);
            case null: return 1; // as a tradition of .net
            default: throw new InvalidOperationException($"EnumEx: Cannot compare to an object of type {obj.GetType()}");
            }
        }

        public int CompareTo(EnumEx<T> other) => Comparer<T>.Default.Compare(Value, other.Value);
        public int CompareTo(T other) => Comparer<T>.Default.Compare(Value, other);

        TypeCode IConvertible.GetTypeCode() => Value.GetTypeCode(); // this will match the underlying type

        Boolean IConvertible.ToBoolean(IFormatProvider? provider) => Value.ToBoolean(provider);
        Char IConvertible.ToChar(IFormatProvider? provider) => Value.ToChar(provider);
        SByte IConvertible.ToSByte(IFormatProvider? provider) => Value.ToSByte(provider);
        Byte IConvertible.ToByte(IFormatProvider? provider) => Value.ToByte(provider);
        Int16 IConvertible.ToInt16(IFormatProvider? provider) => Value.ToInt16(provider);
        UInt16 IConvertible.ToUInt16(IFormatProvider? provider) => Value.ToUInt16(provider);
        Int32 IConvertible.ToInt32(IFormatProvider? provider) => Value.ToInt32(provider);
        UInt32 IConvertible.ToUInt32(IFormatProvider? provider) => Value.ToUInt32(provider);
        Int64 IConvertible.ToInt64(IFormatProvider? provider) => Value.ToInt64(provider);
        UInt64 IConvertible.ToUInt64(IFormatProvider? provider) => Value.ToUInt64(provider);
        Single IConvertible.ToSingle(IFormatProvider? provider) => Value.ToSingle(provider);
        Double IConvertible.ToDouble(IFormatProvider? provider) => Value.ToDouble(provider);
        Decimal IConvertible.ToDecimal(IFormatProvider? provider) => Value.ToDecimal(provider);
        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => Value.ToDateTime(provider);
        String IConvertible.ToString(IFormatProvider? provider) => ToString();

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Value.ToType(conversionType, provider);
        TResult IBoxlessConvertible.ToType<TResult>(IFormatProvider? provider) => BoxlessConvert.To<TResult>.From(Value, provider) ?? throw new InvalidCastException($"EnumEx: Boxless conversion to type {typeof(TResult)} is undefined.");

        public string ToString(string? format, IFormatProvider? formatProvider) {
            if (predefinedEnums.Contains(Value)) return Value.ToString(format);
            using (var lockRAII = ReaderLockRAII.Read()) {
                return mapEnumToName.GetValueOrDefault(Value) ?? Value.ToString(format);
            }
        }

        public static bool operator<(EnumEx<T> left, EnumEx<T> right) => left.CompareTo(right) < 0;
        public static bool operator<=(EnumEx<T> left, EnumEx<T> right) => left.CompareTo(right) <= 0;
        public static bool operator>(EnumEx<T> left, EnumEx<T> right) => left.CompareTo(right) > 0;
        public static bool operator>=(EnumEx<T> left, EnumEx<T> right) => left.CompareTo(right) >= 0;

        public static bool operator==(EnumEx<T> left, EnumEx<T> right) => left.Equals(right);
        public static bool operator!=(EnumEx<T> left, EnumEx<T> right) => !left.Equals(right);
    }
}