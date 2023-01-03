// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

#nullable enable

namespace MaTech.Common.Algorithm {
    public static partial class BoxlessConvert {
        // Here we implement factories of casting delegates for simple value types

        private static Type typeBoolean = typeof(Boolean);
        private static Type typeChar = typeof(Char);
        private static Type typeSByte = typeof(SByte);
        private static Type typeByte = typeof(Byte);
        private static Type typeInt16 = typeof(Int16);
        private static Type typeUInt16 = typeof(UInt16);
        private static Type typeInt32 = typeof(Int32);
        private static Type typeUInt32 = typeof(UInt32);
        private static Type typeInt64 = typeof(Int64);
        private static Type typeUInt64 = typeof(UInt64);
        private static Type typeSingle = typeof(Single);
        private static Type typeDouble = typeof(Double);
        private static Type typeDecimal = typeof(Decimal);
        private static Type typeDateTime = typeof(DateTime); // unsupported here, use IdentityCast instead
        private static Type typeString = typeof(String); // unsupported here, use ConvertibleCast instead

        // Comments from https://referencesource.microsoft.com/#mscorlib/system/convert.cs
        //
        // The statically typed conversion methods provided by the Value class are all
        // of the form:
        //
        //    public static XXX ToXXX(YYY value)
        //
        // where XXX is the target type and YYY is the source type. The matrix below
        // shows the set of supported conversions. The set of conversions is symmetric
        // such that for every ToXXX(YYY) there is also a ToYYY(XXX).
        //
        // From:  To: Bol Chr SBy Byt I16 U16 I32 U32 I64 U64 Sgl Dbl Dec Dat Str
        // ----------------------------------------------------------------------
        // Boolean     x       x   x   x   x   x   x   x   x   x   x   x       x
        // Char            x   x   x   x   x   x   x   x   x                   x
        // SByte       x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // Byte        x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // Int16       x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // UInt16      x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // Int32       x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // UInt32      x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // Int64       x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // UInt64      x   x   x   x   x   x   x   x   x   x   x   x   x       x
        // Single      x       x   x   x   x   x   x   x   x   x   x   x       x
        // Double      x       x   x   x   x   x   x   x   x   x   x   x       x
        // Decimal     x       x   x   x   x   x   x   x   x   x   x   x       x
        // DateTime                                                        x   x
        // String      x   x   x   x   x   x   x   x   x   x   x   x   x   x   x
        // ----------------------------------------------------------------------
        private static readonly Dictionary<(Type, Type), Func<Delegate>> matrixSimpleTypeCasterFactoryFunc = new Dictionary<(Type, Type), Func<Delegate>> { // 13 x 13
            { (typeBoolean, typeBoolean), () => new Caster<Boolean, Boolean>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            //{ (typeBoolean, typeChar), () => new Caster<Boolean, Char>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeBoolean, typeSByte), () => new Caster<Boolean, SByte>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeBoolean, typeByte), () => new Caster<Boolean, Byte>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeBoolean, typeInt16), () => new Caster<Boolean, Int16>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeBoolean, typeUInt16), () => new Caster<Boolean, UInt16>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeBoolean, typeInt32), () => new Caster<Boolean, Int32>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeBoolean, typeUInt32), () => new Caster<Boolean, UInt32>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeBoolean, typeInt64), () => new Caster<Boolean, Int64>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeBoolean, typeUInt64), () => new Caster<Boolean, UInt64>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeBoolean, typeSingle), () => new Caster<Boolean, Single>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeBoolean, typeDouble), () => new Caster<Boolean, Double>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeBoolean, typeDecimal), () => new Caster<Boolean, Decimal>(delegate (in Boolean source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            //{ (typeChar, typeBoolean), () => new Caster<Char, Boolean>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeChar, typeChar), () => new Caster<Char, Char>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeChar, typeSByte), () => new Caster<Char, SByte>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeChar, typeByte), () => new Caster<Char, Byte>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeChar, typeInt16), () => new Caster<Char, Int16>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeChar, typeUInt16), () => new Caster<Char, UInt16>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeChar, typeInt32), () => new Caster<Char, Int32>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeChar, typeUInt32), () => new Caster<Char, UInt32>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeChar, typeInt64), () => new Caster<Char, Int64>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeChar, typeUInt64), () => new Caster<Char, UInt64>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            //{ (typeChar, typeSingle), () => new Caster<Char, Single>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            //{ (typeChar, typeDouble), () => new Caster<Char, Double>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            //{ (typeChar, typeDecimal), () => new Caster<Char, Decimal>(delegate (in Char source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeSByte, typeBoolean), () => new Caster<SByte, Boolean>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeSByte, typeChar), () => new Caster<SByte, Char>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeSByte, typeSByte), () => new Caster<SByte, SByte>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeSByte, typeByte), () => new Caster<SByte, Byte>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeSByte, typeInt16), () => new Caster<SByte, Int16>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeSByte, typeUInt16), () => new Caster<SByte, UInt16>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeSByte, typeInt32), () => new Caster<SByte, Int32>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeSByte, typeUInt32), () => new Caster<SByte, UInt32>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeSByte, typeInt64), () => new Caster<SByte, Int64>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeSByte, typeUInt64), () => new Caster<SByte, UInt64>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeSByte, typeSingle), () => new Caster<SByte, Single>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeSByte, typeDouble), () => new Caster<SByte, Double>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeSByte, typeDecimal), () => new Caster<SByte, Decimal>(delegate (in SByte source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeByte, typeBoolean), () => new Caster<Byte, Boolean>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeByte, typeChar), () => new Caster<Byte, Char>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeByte, typeSByte), () => new Caster<Byte, SByte>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeByte, typeByte), () => new Caster<Byte, Byte>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeByte, typeInt16), () => new Caster<Byte, Int16>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeByte, typeUInt16), () => new Caster<Byte, UInt16>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeByte, typeInt32), () => new Caster<Byte, Int32>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeByte, typeUInt32), () => new Caster<Byte, UInt32>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeByte, typeInt64), () => new Caster<Byte, Int64>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeByte, typeUInt64), () => new Caster<Byte, UInt64>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeByte, typeSingle), () => new Caster<Byte, Single>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeByte, typeDouble), () => new Caster<Byte, Double>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeByte, typeDecimal), () => new Caster<Byte, Decimal>(delegate (in Byte source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeInt16, typeBoolean), () => new Caster<Int16, Boolean>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeInt16, typeChar), () => new Caster<Int16, Char>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeInt16, typeSByte), () => new Caster<Int16, SByte>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeInt16, typeByte), () => new Caster<Int16, Byte>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeInt16, typeInt16), () => new Caster<Int16, Int16>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeInt16, typeUInt16), () => new Caster<Int16, UInt16>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeInt16, typeInt32), () => new Caster<Int16, Int32>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeInt16, typeUInt32), () => new Caster<Int16, UInt32>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeInt16, typeInt64), () => new Caster<Int16, Int64>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeInt16, typeUInt64), () => new Caster<Int16, UInt64>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeInt16, typeSingle), () => new Caster<Int16, Single>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeInt16, typeDouble), () => new Caster<Int16, Double>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeInt16, typeDecimal), () => new Caster<Int16, Decimal>(delegate (in Int16 source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeUInt16, typeBoolean), () => new Caster<UInt16, Boolean>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeUInt16, typeChar), () => new Caster<UInt16, Char>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeUInt16, typeSByte), () => new Caster<UInt16, SByte>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeUInt16, typeByte), () => new Caster<UInt16, Byte>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeUInt16, typeInt16), () => new Caster<UInt16, Int16>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeUInt16, typeUInt16), () => new Caster<UInt16, UInt16>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeUInt16, typeInt32), () => new Caster<UInt16, Int32>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeUInt16, typeUInt32), () => new Caster<UInt16, UInt32>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeUInt16, typeInt64), () => new Caster<UInt16, Int64>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeUInt16, typeUInt64), () => new Caster<UInt16, UInt64>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeUInt16, typeSingle), () => new Caster<UInt16, Single>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeUInt16, typeDouble), () => new Caster<UInt16, Double>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeUInt16, typeDecimal), () => new Caster<UInt16, Decimal>(delegate (in UInt16 source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeInt32, typeBoolean), () => new Caster<Int32, Boolean>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeInt32, typeChar), () => new Caster<Int32, Char>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeInt32, typeSByte), () => new Caster<Int32, SByte>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeInt32, typeByte), () => new Caster<Int32, Byte>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeInt32, typeInt16), () => new Caster<Int32, Int16>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeInt32, typeUInt16), () => new Caster<Int32, UInt16>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeInt32, typeInt32), () => new Caster<Int32, Int32>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeInt32, typeUInt32), () => new Caster<Int32, UInt32>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeInt32, typeInt64), () => new Caster<Int32, Int64>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeInt32, typeUInt64), () => new Caster<Int32, UInt64>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeInt32, typeSingle), () => new Caster<Int32, Single>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeInt32, typeDouble), () => new Caster<Int32, Double>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeInt32, typeDecimal), () => new Caster<Int32, Decimal>(delegate (in Int32 source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeUInt32, typeBoolean), () => new Caster<UInt32, Boolean>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeUInt32, typeChar), () => new Caster<UInt32, Char>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeUInt32, typeSByte), () => new Caster<UInt32, SByte>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeUInt32, typeByte), () => new Caster<UInt32, Byte>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeUInt32, typeInt16), () => new Caster<UInt32, Int16>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeUInt32, typeUInt16), () => new Caster<UInt32, UInt16>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeUInt32, typeInt32), () => new Caster<UInt32, Int32>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeUInt32, typeUInt32), () => new Caster<UInt32, UInt32>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeUInt32, typeInt64), () => new Caster<UInt32, Int64>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeUInt32, typeUInt64), () => new Caster<UInt32, UInt64>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeUInt32, typeSingle), () => new Caster<UInt32, Single>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeUInt32, typeDouble), () => new Caster<UInt32, Double>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeUInt32, typeDecimal), () => new Caster<UInt32, Decimal>(delegate (in UInt32 source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeInt64, typeBoolean), () => new Caster<Int64, Boolean>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeInt64, typeChar), () => new Caster<Int64, Char>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeInt64, typeSByte), () => new Caster<Int64, SByte>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeInt64, typeByte), () => new Caster<Int64, Byte>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeInt64, typeInt16), () => new Caster<Int64, Int16>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeInt64, typeUInt16), () => new Caster<Int64, UInt16>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeInt64, typeInt32), () => new Caster<Int64, Int32>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeInt64, typeUInt32), () => new Caster<Int64, UInt32>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeInt64, typeInt64), () => new Caster<Int64, Int64>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeInt64, typeUInt64), () => new Caster<Int64, UInt64>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeInt64, typeSingle), () => new Caster<Int64, Single>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeInt64, typeDouble), () => new Caster<Int64, Double>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeInt64, typeDecimal), () => new Caster<Int64, Decimal>(delegate (in Int64 source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeUInt64, typeBoolean), () => new Caster<UInt64, Boolean>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            { (typeUInt64, typeChar), () => new Caster<UInt64, Char>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeUInt64, typeSByte), () => new Caster<UInt64, SByte>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeUInt64, typeByte), () => new Caster<UInt64, Byte>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeUInt64, typeInt16), () => new Caster<UInt64, Int16>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeUInt64, typeUInt16), () => new Caster<UInt64, UInt16>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeUInt64, typeInt32), () => new Caster<UInt64, Int32>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeUInt64, typeUInt32), () => new Caster<UInt64, UInt32>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeUInt64, typeInt64), () => new Caster<UInt64, Int64>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeUInt64, typeUInt64), () => new Caster<UInt64, UInt64>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeUInt64, typeSingle), () => new Caster<UInt64, Single>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeUInt64, typeDouble), () => new Caster<UInt64, Double>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeUInt64, typeDecimal), () => new Caster<UInt64, Decimal>(delegate (in UInt64 source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeSingle, typeBoolean), () => new Caster<Single, Boolean>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            //{ (typeSingle, typeChar), () => new Caster<Single, Char>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeSingle, typeSByte), () => new Caster<Single, SByte>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeSingle, typeByte), () => new Caster<Single, Byte>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeSingle, typeInt16), () => new Caster<Single, Int16>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeSingle, typeUInt16), () => new Caster<Single, UInt16>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeSingle, typeInt32), () => new Caster<Single, Int32>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeSingle, typeUInt32), () => new Caster<Single, UInt32>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeSingle, typeInt64), () => new Caster<Single, Int64>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeSingle, typeUInt64), () => new Caster<Single, UInt64>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeSingle, typeSingle), () => new Caster<Single, Single>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeSingle, typeDouble), () => new Caster<Single, Double>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeSingle, typeDecimal), () => new Caster<Single, Decimal>(delegate (in Single source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeDouble, typeBoolean), () => new Caster<Double, Boolean>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            //{ (typeDouble, typeChar), () => new Caster<Double, Char>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeDouble, typeSByte), () => new Caster<Double, SByte>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeDouble, typeByte), () => new Caster<Double, Byte>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeDouble, typeInt16), () => new Caster<Double, Int16>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeDouble, typeUInt16), () => new Caster<Double, UInt16>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeDouble, typeInt32), () => new Caster<Double, Int32>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeDouble, typeUInt32), () => new Caster<Double, UInt32>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeDouble, typeInt64), () => new Caster<Double, Int64>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeDouble, typeUInt64), () => new Caster<Double, UInt64>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeDouble, typeSingle), () => new Caster<Double, Single>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeDouble, typeDouble), () => new Caster<Double, Double>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeDouble, typeDecimal), () => new Caster<Double, Decimal>(delegate (in Double source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },

            { (typeDecimal, typeBoolean), () => new Caster<Decimal, Boolean>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToBoolean(source, provider); }) },
            //{ (typeDecimal, typeChar), () => new Caster<Decimal, Char>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToChar(source, provider); }) },
            { (typeDecimal, typeSByte), () => new Caster<Decimal, SByte>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToSByte(source, provider); }) },
            { (typeDecimal, typeByte), () => new Caster<Decimal, Byte>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToByte(source, provider); }) },
            { (typeDecimal, typeInt16), () => new Caster<Decimal, Int16>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToInt16(source, provider); }) },
            { (typeDecimal, typeUInt16), () => new Caster<Decimal, UInt16>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToUInt16(source, provider); }) },
            { (typeDecimal, typeInt32), () => new Caster<Decimal, Int32>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToInt32(source, provider); }) },
            { (typeDecimal, typeUInt32), () => new Caster<Decimal, UInt32>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToUInt32(source, provider); }) },
            { (typeDecimal, typeInt64), () => new Caster<Decimal, Int64>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToInt64(source, provider); }) },
            { (typeDecimal, typeUInt64), () => new Caster<Decimal, UInt64>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToUInt64(source, provider); }) },
            { (typeDecimal, typeSingle), () => new Caster<Decimal, Single>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToSingle(source, provider); }) },
            { (typeDecimal, typeDouble), () => new Caster<Decimal, Double>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToDouble(source, provider); }) },
            { (typeDecimal, typeDecimal), () => new Caster<Decimal, Decimal>(delegate (in Decimal source, IFormatProvider? provider) { return Convert.ToDecimal(source, provider); }) },
        };

        private static class SimpleTypeCasterFactory<TSource> {
            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                if (matrixSimpleTypeCasterFactoryFunc.TryGetValue((typeof(TSource), typeof(TResult)), out var factoryFunc))
                    return (Caster<TSource, TResult>)factoryFunc();
                return null;
            }
        }
        
    }
}
