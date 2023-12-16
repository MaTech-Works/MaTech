// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

namespace MaTech.Common.Data {
    public partial struct DataEnum<TEnum> :
        IComparable, IComparable<DataEnum<TEnum>>, IComparable<TEnum>,
        IEquatable<DataEnum<TEnum>>, IEquatable<TEnum>,
        IConvertible, IBoxlessConvertible, IFormattable {

        private static EqualityComparer<TEnum> valueEqualityComparer = EqualityComparer<TEnum>.Default;
        
        public override string ToString() => ToString(null, null);
        public override int GetHashCode() => valueEqualityComparer.GetHashCode(Value);

        public override bool Equals(object? obj) {
            switch (obj) {
            case DataEnum<TEnum> ex: return Equals(ex);
            case TEnum e: return Equals(e);
            default: return false;
            }
        }

        public bool Equals(DataEnum<TEnum> other) => valueEqualityComparer.Equals(Value, other.Value);
        public bool Equals(TEnum other) => valueEqualityComparer.Equals(Value, other);

        public int CompareTo(object? obj) {
            switch (obj) {
            case DataEnum<TEnum> ex: return CompareTo(ex);
            case TEnum e: return CompareTo(e);
            case null: return 1; // as a tradition of .net
            default: throw new InvalidOperationException($"[DataEnum] Cannot compare to an object of type {obj.GetType()}.");
            }
        }

        public int CompareTo(DataEnum<TEnum> other) => Comparer<TEnum>.Default.Compare(Value, other.Value);
        public int CompareTo(TEnum other) => Comparer<TEnum>.Default.Compare(Value, other);

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
        String IConvertible.ToString(IFormatProvider? provider) => ToString(null, provider);
        
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Value.ToType(conversionType, provider);
        
        TResult IBoxlessConvertible.ToType<TResult>(IFormatProvider? provider) => BoxlessConvert.ChangeType<TEnum, TResult>(Value, provider);

        public string ToString(string? format, IFormatProvider? formatProvider) {
            if (predefinedEnumToName.TryGetValue(Value, out var name)) return name;
            using (var lockRAII = ReaderLockRAII.EnterRead(lockMetadata)) {
                return mapEnumToName.GetValueOrDefault(Value) ?? Value.ToString(format);
            }
        }

        public static bool operator<(DataEnum<TEnum> left, DataEnum<TEnum> right) => left.CompareTo(right) < 0;
        public static bool operator<=(DataEnum<TEnum> left, DataEnum<TEnum> right) => left.CompareTo(right) <= 0;
        public static bool operator>(DataEnum<TEnum> left, DataEnum<TEnum> right) => left.CompareTo(right) > 0;
        public static bool operator>=(DataEnum<TEnum> left, DataEnum<TEnum> right) => left.CompareTo(right) >= 0;

        public static bool operator==(DataEnum<TEnum> left, DataEnum<TEnum> right) => left.Equals(right);
        public static bool operator!=(DataEnum<TEnum> left, DataEnum<TEnum> right) => !left.Equals(right);
    }
}