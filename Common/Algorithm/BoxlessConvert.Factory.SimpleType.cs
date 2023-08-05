// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace MaTech.Common.Algorithm {
    public static partial class BoxlessConvert {
        // Here we implement factories of casting delegates for simple value types

        private static readonly Type typeBoolean = typeof(Boolean);
        private static readonly Type typeChar = typeof(Char);
        private static readonly Type typeSByte = typeof(SByte);
        private static readonly Type typeByte = typeof(Byte);
        private static readonly Type typeInt16 = typeof(Int16);
        private static readonly Type typeUInt16 = typeof(UInt16);
        private static readonly Type typeInt32 = typeof(Int32);
        private static readonly Type typeUInt32 = typeof(UInt32);
        private static readonly Type typeInt64 = typeof(Int64);
        private static readonly Type typeUInt64 = typeof(UInt64);
        private static readonly Type typeSingle = typeof(Single);
        private static readonly Type typeDouble = typeof(Double);
        private static readonly Type typeDecimal = typeof(Decimal);
        private static readonly Type typeDateTime = typeof(DateTime); // unsupported here, use IdentityCast instead
        private static readonly Type typeString = typeof(String); // unsupported here, use ConvertibleCast instead

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
            { (typeBoolean, typeBoolean), () => new Caster<Boolean, Boolean>((in Boolean source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            //{ (typeBoolean, typeChar), () => new Caster<Boolean, Char>((in Boolean source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeBoolean, typeSByte), () => new Caster<Boolean, SByte>((in Boolean source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeBoolean, typeByte), () => new Caster<Boolean, Byte>((in Boolean source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeBoolean, typeInt16), () => new Caster<Boolean, Int16>((in Boolean source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeBoolean, typeUInt16), () => new Caster<Boolean, UInt16>((in Boolean source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeBoolean, typeInt32), () => new Caster<Boolean, Int32>((in Boolean source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeBoolean, typeUInt32), () => new Caster<Boolean, UInt32>((in Boolean source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeBoolean, typeInt64), () => new Caster<Boolean, Int64>((in Boolean source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeBoolean, typeUInt64), () => new Caster<Boolean, UInt64>((in Boolean source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeBoolean, typeSingle), () => new Caster<Boolean, Single>((in Boolean source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeBoolean, typeDouble), () => new Caster<Boolean, Double>((in Boolean source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeBoolean, typeDecimal), () => new Caster<Boolean, Decimal>((in Boolean source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            //{ (typeChar, typeBoolean), () => new Caster<Char, Boolean>((in Char source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeChar, typeChar), () => new Caster<Char, Char>((in Char source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeChar, typeSByte), () => new Caster<Char, SByte>((in Char source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeChar, typeByte), () => new Caster<Char, Byte>((in Char source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeChar, typeInt16), () => new Caster<Char, Int16>((in Char source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeChar, typeUInt16), () => new Caster<Char, UInt16>((in Char source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeChar, typeInt32), () => new Caster<Char, Int32>((in Char source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeChar, typeUInt32), () => new Caster<Char, UInt32>((in Char source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeChar, typeInt64), () => new Caster<Char, Int64>((in Char source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeChar, typeUInt64), () => new Caster<Char, UInt64>((in Char source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            //{ (typeChar, typeSingle), () => new Caster<Char, Single>((in Char source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            //{ (typeChar, typeDouble), () => new Caster<Char, Double>((in Char source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            //{ (typeChar, typeDecimal), () => new Caster<Char, Decimal>((in Char source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeSByte, typeBoolean), () => new Caster<SByte, Boolean>((in SByte source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeSByte, typeChar), () => new Caster<SByte, Char>((in SByte source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeSByte, typeSByte), () => new Caster<SByte, SByte>((in SByte source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeSByte, typeByte), () => new Caster<SByte, Byte>((in SByte source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeSByte, typeInt16), () => new Caster<SByte, Int16>((in SByte source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeSByte, typeUInt16), () => new Caster<SByte, UInt16>((in SByte source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeSByte, typeInt32), () => new Caster<SByte, Int32>((in SByte source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeSByte, typeUInt32), () => new Caster<SByte, UInt32>((in SByte source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeSByte, typeInt64), () => new Caster<SByte, Int64>((in SByte source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeSByte, typeUInt64), () => new Caster<SByte, UInt64>((in SByte source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeSByte, typeSingle), () => new Caster<SByte, Single>((in SByte source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeSByte, typeDouble), () => new Caster<SByte, Double>((in SByte source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeSByte, typeDecimal), () => new Caster<SByte, Decimal>((in SByte source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeByte, typeBoolean), () => new Caster<Byte, Boolean>((in Byte source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeByte, typeChar), () => new Caster<Byte, Char>((in Byte source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeByte, typeSByte), () => new Caster<Byte, SByte>((in Byte source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeByte, typeByte), () => new Caster<Byte, Byte>((in Byte source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeByte, typeInt16), () => new Caster<Byte, Int16>((in Byte source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeByte, typeUInt16), () => new Caster<Byte, UInt16>((in Byte source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeByte, typeInt32), () => new Caster<Byte, Int32>((in Byte source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeByte, typeUInt32), () => new Caster<Byte, UInt32>((in Byte source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeByte, typeInt64), () => new Caster<Byte, Int64>((in Byte source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeByte, typeUInt64), () => new Caster<Byte, UInt64>((in Byte source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeByte, typeSingle), () => new Caster<Byte, Single>((in Byte source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeByte, typeDouble), () => new Caster<Byte, Double>((in Byte source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeByte, typeDecimal), () => new Caster<Byte, Decimal>((in Byte source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeInt16, typeBoolean), () => new Caster<Int16, Boolean>((in Int16 source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeInt16, typeChar), () => new Caster<Int16, Char>((in Int16 source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeInt16, typeSByte), () => new Caster<Int16, SByte>((in Int16 source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeInt16, typeByte), () => new Caster<Int16, Byte>((in Int16 source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeInt16, typeInt16), () => new Caster<Int16, Int16>((in Int16 source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeInt16, typeUInt16), () => new Caster<Int16, UInt16>((in Int16 source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeInt16, typeInt32), () => new Caster<Int16, Int32>((in Int16 source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeInt16, typeUInt32), () => new Caster<Int16, UInt32>((in Int16 source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeInt16, typeInt64), () => new Caster<Int16, Int64>((in Int16 source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeInt16, typeUInt64), () => new Caster<Int16, UInt64>((in Int16 source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeInt16, typeSingle), () => new Caster<Int16, Single>((in Int16 source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeInt16, typeDouble), () => new Caster<Int16, Double>((in Int16 source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeInt16, typeDecimal), () => new Caster<Int16, Decimal>((in Int16 source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeUInt16, typeBoolean), () => new Caster<UInt16, Boolean>((in UInt16 source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeUInt16, typeChar), () => new Caster<UInt16, Char>((in UInt16 source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeUInt16, typeSByte), () => new Caster<UInt16, SByte>((in UInt16 source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeUInt16, typeByte), () => new Caster<UInt16, Byte>((in UInt16 source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeUInt16, typeInt16), () => new Caster<UInt16, Int16>((in UInt16 source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeUInt16, typeUInt16), () => new Caster<UInt16, UInt16>((in UInt16 source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeUInt16, typeInt32), () => new Caster<UInt16, Int32>((in UInt16 source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeUInt16, typeUInt32), () => new Caster<UInt16, UInt32>((in UInt16 source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeUInt16, typeInt64), () => new Caster<UInt16, Int64>((in UInt16 source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeUInt16, typeUInt64), () => new Caster<UInt16, UInt64>((in UInt16 source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeUInt16, typeSingle), () => new Caster<UInt16, Single>((in UInt16 source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeUInt16, typeDouble), () => new Caster<UInt16, Double>((in UInt16 source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeUInt16, typeDecimal), () => new Caster<UInt16, Decimal>((in UInt16 source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeInt32, typeBoolean), () => new Caster<Int32, Boolean>((in Int32 source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeInt32, typeChar), () => new Caster<Int32, Char>((in Int32 source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeInt32, typeSByte), () => new Caster<Int32, SByte>((in Int32 source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeInt32, typeByte), () => new Caster<Int32, Byte>((in Int32 source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeInt32, typeInt16), () => new Caster<Int32, Int16>((in Int32 source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeInt32, typeUInt16), () => new Caster<Int32, UInt16>((in Int32 source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeInt32, typeInt32), () => new Caster<Int32, Int32>((in Int32 source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeInt32, typeUInt32), () => new Caster<Int32, UInt32>((in Int32 source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeInt32, typeInt64), () => new Caster<Int32, Int64>((in Int32 source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeInt32, typeUInt64), () => new Caster<Int32, UInt64>((in Int32 source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeInt32, typeSingle), () => new Caster<Int32, Single>((in Int32 source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeInt32, typeDouble), () => new Caster<Int32, Double>((in Int32 source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeInt32, typeDecimal), () => new Caster<Int32, Decimal>((in Int32 source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeUInt32, typeBoolean), () => new Caster<UInt32, Boolean>((in UInt32 source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeUInt32, typeChar), () => new Caster<UInt32, Char>((in UInt32 source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeUInt32, typeSByte), () => new Caster<UInt32, SByte>((in UInt32 source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeUInt32, typeByte), () => new Caster<UInt32, Byte>((in UInt32 source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeUInt32, typeInt16), () => new Caster<UInt32, Int16>((in UInt32 source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeUInt32, typeUInt16), () => new Caster<UInt32, UInt16>((in UInt32 source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeUInt32, typeInt32), () => new Caster<UInt32, Int32>((in UInt32 source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeUInt32, typeUInt32), () => new Caster<UInt32, UInt32>((in UInt32 source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeUInt32, typeInt64), () => new Caster<UInt32, Int64>((in UInt32 source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeUInt32, typeUInt64), () => new Caster<UInt32, UInt64>((in UInt32 source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeUInt32, typeSingle), () => new Caster<UInt32, Single>((in UInt32 source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeUInt32, typeDouble), () => new Caster<UInt32, Double>((in UInt32 source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeUInt32, typeDecimal), () => new Caster<UInt32, Decimal>((in UInt32 source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeInt64, typeBoolean), () => new Caster<Int64, Boolean>((in Int64 source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeInt64, typeChar), () => new Caster<Int64, Char>((in Int64 source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeInt64, typeSByte), () => new Caster<Int64, SByte>((in Int64 source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeInt64, typeByte), () => new Caster<Int64, Byte>((in Int64 source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeInt64, typeInt16), () => new Caster<Int64, Int16>((in Int64 source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeInt64, typeUInt16), () => new Caster<Int64, UInt16>((in Int64 source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeInt64, typeInt32), () => new Caster<Int64, Int32>((in Int64 source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeInt64, typeUInt32), () => new Caster<Int64, UInt32>((in Int64 source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeInt64, typeInt64), () => new Caster<Int64, Int64>((in Int64 source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeInt64, typeUInt64), () => new Caster<Int64, UInt64>((in Int64 source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeInt64, typeSingle), () => new Caster<Int64, Single>((in Int64 source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeInt64, typeDouble), () => new Caster<Int64, Double>((in Int64 source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeInt64, typeDecimal), () => new Caster<Int64, Decimal>((in Int64 source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeUInt64, typeBoolean), () => new Caster<UInt64, Boolean>((in UInt64 source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            { (typeUInt64, typeChar), () => new Caster<UInt64, Char>((in UInt64 source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeUInt64, typeSByte), () => new Caster<UInt64, SByte>((in UInt64 source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeUInt64, typeByte), () => new Caster<UInt64, Byte>((in UInt64 source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeUInt64, typeInt16), () => new Caster<UInt64, Int16>((in UInt64 source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeUInt64, typeUInt16), () => new Caster<UInt64, UInt16>((in UInt64 source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeUInt64, typeInt32), () => new Caster<UInt64, Int32>((in UInt64 source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeUInt64, typeUInt32), () => new Caster<UInt64, UInt32>((in UInt64 source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeUInt64, typeInt64), () => new Caster<UInt64, Int64>((in UInt64 source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeUInt64, typeUInt64), () => new Caster<UInt64, UInt64>((in UInt64 source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeUInt64, typeSingle), () => new Caster<UInt64, Single>((in UInt64 source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeUInt64, typeDouble), () => new Caster<UInt64, Double>((in UInt64 source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeUInt64, typeDecimal), () => new Caster<UInt64, Decimal>((in UInt64 source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeSingle, typeBoolean), () => new Caster<Single, Boolean>((in Single source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            //{ (typeSingle, typeChar), () => new Caster<Single, Char>((in Single source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeSingle, typeSByte), () => new Caster<Single, SByte>((in Single source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeSingle, typeByte), () => new Caster<Single, Byte>((in Single source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeSingle, typeInt16), () => new Caster<Single, Int16>((in Single source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeSingle, typeUInt16), () => new Caster<Single, UInt16>((in Single source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeSingle, typeInt32), () => new Caster<Single, Int32>((in Single source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeSingle, typeUInt32), () => new Caster<Single, UInt32>((in Single source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeSingle, typeInt64), () => new Caster<Single, Int64>((in Single source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeSingle, typeUInt64), () => new Caster<Single, UInt64>((in Single source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeSingle, typeSingle), () => new Caster<Single, Single>((in Single source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeSingle, typeDouble), () => new Caster<Single, Double>((in Single source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeSingle, typeDecimal), () => new Caster<Single, Decimal>((in Single source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeDouble, typeBoolean), () => new Caster<Double, Boolean>((in Double source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            //{ (typeDouble, typeChar), () => new Caster<Double, Char>((in Double source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeDouble, typeSByte), () => new Caster<Double, SByte>((in Double source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeDouble, typeByte), () => new Caster<Double, Byte>((in Double source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeDouble, typeInt16), () => new Caster<Double, Int16>((in Double source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeDouble, typeUInt16), () => new Caster<Double, UInt16>((in Double source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeDouble, typeInt32), () => new Caster<Double, Int32>((in Double source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeDouble, typeUInt32), () => new Caster<Double, UInt32>((in Double source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeDouble, typeInt64), () => new Caster<Double, Int64>((in Double source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeDouble, typeUInt64), () => new Caster<Double, UInt64>((in Double source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeDouble, typeSingle), () => new Caster<Double, Single>((in Double source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeDouble, typeDouble), () => new Caster<Double, Double>((in Double source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeDouble, typeDecimal), () => new Caster<Double, Decimal>((in Double source, IFormatProvider? provider) => Convert.ToDecimal(source)) },

            { (typeDecimal, typeBoolean), () => new Caster<Decimal, Boolean>((in Decimal source, IFormatProvider? provider) => Convert.ToBoolean(source)) },
            //{ (typeDecimal, typeChar), () => new Caster<Decimal, Char>((in Decimal source, IFormatProvider? provider) => Convert.ToChar(source)) },
            { (typeDecimal, typeSByte), () => new Caster<Decimal, SByte>((in Decimal source, IFormatProvider? provider) => Convert.ToSByte(source)) },
            { (typeDecimal, typeByte), () => new Caster<Decimal, Byte>((in Decimal source, IFormatProvider? provider) => Convert.ToByte(source)) },
            { (typeDecimal, typeInt16), () => new Caster<Decimal, Int16>((in Decimal source, IFormatProvider? provider) => Convert.ToInt16(source)) },
            { (typeDecimal, typeUInt16), () => new Caster<Decimal, UInt16>((in Decimal source, IFormatProvider? provider) => Convert.ToUInt16(source)) },
            { (typeDecimal, typeInt32), () => new Caster<Decimal, Int32>((in Decimal source, IFormatProvider? provider) => Convert.ToInt32(source)) },
            { (typeDecimal, typeUInt32), () => new Caster<Decimal, UInt32>((in Decimal source, IFormatProvider? provider) => Convert.ToUInt32(source)) },
            { (typeDecimal, typeInt64), () => new Caster<Decimal, Int64>((in Decimal source, IFormatProvider? provider) => Convert.ToInt64(source)) },
            { (typeDecimal, typeUInt64), () => new Caster<Decimal, UInt64>((in Decimal source, IFormatProvider? provider) => Convert.ToUInt64(source)) },
            { (typeDecimal, typeSingle), () => new Caster<Decimal, Single>((in Decimal source, IFormatProvider? provider) => Convert.ToSingle(source)) },
            { (typeDecimal, typeDouble), () => new Caster<Decimal, Double>((in Decimal source, IFormatProvider? provider) => Convert.ToDouble(source)) },
            { (typeDecimal, typeDecimal), () => new Caster<Decimal, Decimal>((in Decimal source, IFormatProvider? provider) => Convert.ToDecimal(source)) },
        };
        
        private static class SimpleTypeCasterFactory<TSource> {
            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                if (matrixSimpleTypeCasterFactoryFunc.TryGetValue((typeof(TSource), typeof(TResult)), out var factoryFunc))
                    return (Caster<TSource, TResult>)factoryFunc();
                return null;
            }
        }

        [Preserve]
        public static void PreserveSimpleType_DoNotCall() {
            SimpleTypeCasterFactory<Boolean>.Create<Boolean>();
            SimpleTypeCasterFactory<Boolean>.Create<Char>();
            SimpleTypeCasterFactory<Boolean>.Create<SByte>();
            SimpleTypeCasterFactory<Boolean>.Create<Byte>();
            SimpleTypeCasterFactory<Boolean>.Create<Int16>();
            SimpleTypeCasterFactory<Boolean>.Create<UInt16>();
            SimpleTypeCasterFactory<Boolean>.Create<Int32>();
            SimpleTypeCasterFactory<Boolean>.Create<UInt32>();
            SimpleTypeCasterFactory<Boolean>.Create<Int64>();
            SimpleTypeCasterFactory<Boolean>.Create<UInt64>();
            SimpleTypeCasterFactory<Boolean>.Create<Single>();
            SimpleTypeCasterFactory<Boolean>.Create<Double>();
            SimpleTypeCasterFactory<Boolean>.Create<Decimal>();
            SimpleTypeCasterFactory<Char>.Create<Boolean>();
            SimpleTypeCasterFactory<Char>.Create<Char>();
            SimpleTypeCasterFactory<Char>.Create<SByte>();
            SimpleTypeCasterFactory<Char>.Create<Byte>();
            SimpleTypeCasterFactory<Char>.Create<Int16>();
            SimpleTypeCasterFactory<Char>.Create<UInt16>();
            SimpleTypeCasterFactory<Char>.Create<Int32>();
            SimpleTypeCasterFactory<Char>.Create<UInt32>();
            SimpleTypeCasterFactory<Char>.Create<Int64>();
            SimpleTypeCasterFactory<Char>.Create<UInt64>();
            SimpleTypeCasterFactory<Char>.Create<Single>();
            SimpleTypeCasterFactory<Char>.Create<Double>();
            SimpleTypeCasterFactory<Char>.Create<Decimal>();
            SimpleTypeCasterFactory<SByte>.Create<Boolean>();
            SimpleTypeCasterFactory<SByte>.Create<Char>();
            SimpleTypeCasterFactory<SByte>.Create<SByte>();
            SimpleTypeCasterFactory<SByte>.Create<Byte>();
            SimpleTypeCasterFactory<SByte>.Create<Int16>();
            SimpleTypeCasterFactory<SByte>.Create<UInt16>();
            SimpleTypeCasterFactory<SByte>.Create<Int32>();
            SimpleTypeCasterFactory<SByte>.Create<UInt32>();
            SimpleTypeCasterFactory<SByte>.Create<Int64>();
            SimpleTypeCasterFactory<SByte>.Create<UInt64>();
            SimpleTypeCasterFactory<SByte>.Create<Single>();
            SimpleTypeCasterFactory<SByte>.Create<Double>();
            SimpleTypeCasterFactory<SByte>.Create<Decimal>();
            SimpleTypeCasterFactory<Byte>.Create<Boolean>();
            SimpleTypeCasterFactory<Byte>.Create<Char>();
            SimpleTypeCasterFactory<Byte>.Create<SByte>();
            SimpleTypeCasterFactory<Byte>.Create<Byte>();
            SimpleTypeCasterFactory<Byte>.Create<Int16>();
            SimpleTypeCasterFactory<Byte>.Create<UInt16>();
            SimpleTypeCasterFactory<Byte>.Create<Int32>();
            SimpleTypeCasterFactory<Byte>.Create<UInt32>();
            SimpleTypeCasterFactory<Byte>.Create<Int64>();
            SimpleTypeCasterFactory<Byte>.Create<UInt64>();
            SimpleTypeCasterFactory<Byte>.Create<Single>();
            SimpleTypeCasterFactory<Byte>.Create<Double>();
            SimpleTypeCasterFactory<Byte>.Create<Decimal>();
            SimpleTypeCasterFactory<Int16>.Create<Boolean>();
            SimpleTypeCasterFactory<Int16>.Create<Char>();
            SimpleTypeCasterFactory<Int16>.Create<SByte>();
            SimpleTypeCasterFactory<Int16>.Create<Byte>();
            SimpleTypeCasterFactory<Int16>.Create<Int16>();
            SimpleTypeCasterFactory<Int16>.Create<UInt16>();
            SimpleTypeCasterFactory<Int16>.Create<Int32>();
            SimpleTypeCasterFactory<Int16>.Create<UInt32>();
            SimpleTypeCasterFactory<Int16>.Create<Int64>();
            SimpleTypeCasterFactory<Int16>.Create<UInt64>();
            SimpleTypeCasterFactory<Int16>.Create<Single>();
            SimpleTypeCasterFactory<Int16>.Create<Double>();
            SimpleTypeCasterFactory<Int16>.Create<Decimal>();
            SimpleTypeCasterFactory<UInt16>.Create<Boolean>();
            SimpleTypeCasterFactory<UInt16>.Create<Char>();
            SimpleTypeCasterFactory<UInt16>.Create<SByte>();
            SimpleTypeCasterFactory<UInt16>.Create<Byte>();
            SimpleTypeCasterFactory<UInt16>.Create<Int16>();
            SimpleTypeCasterFactory<UInt16>.Create<UInt16>();
            SimpleTypeCasterFactory<UInt16>.Create<Int32>();
            SimpleTypeCasterFactory<UInt16>.Create<UInt32>();
            SimpleTypeCasterFactory<UInt16>.Create<Int64>();
            SimpleTypeCasterFactory<UInt16>.Create<UInt64>();
            SimpleTypeCasterFactory<UInt16>.Create<Single>();
            SimpleTypeCasterFactory<UInt16>.Create<Double>();
            SimpleTypeCasterFactory<UInt16>.Create<Decimal>();
            SimpleTypeCasterFactory<Int32>.Create<Boolean>();
            SimpleTypeCasterFactory<Int32>.Create<Char>();
            SimpleTypeCasterFactory<Int32>.Create<SByte>();
            SimpleTypeCasterFactory<Int32>.Create<Byte>();
            SimpleTypeCasterFactory<Int32>.Create<Int16>();
            SimpleTypeCasterFactory<Int32>.Create<UInt16>();
            SimpleTypeCasterFactory<Int32>.Create<Int32>();
            SimpleTypeCasterFactory<Int32>.Create<UInt32>();
            SimpleTypeCasterFactory<Int32>.Create<Int64>();
            SimpleTypeCasterFactory<Int32>.Create<UInt64>();
            SimpleTypeCasterFactory<Int32>.Create<Single>();
            SimpleTypeCasterFactory<Int32>.Create<Double>();
            SimpleTypeCasterFactory<Int32>.Create<Decimal>();
            SimpleTypeCasterFactory<UInt32>.Create<Boolean>();
            SimpleTypeCasterFactory<UInt32>.Create<Char>();
            SimpleTypeCasterFactory<UInt32>.Create<SByte>();
            SimpleTypeCasterFactory<UInt32>.Create<Byte>();
            SimpleTypeCasterFactory<UInt32>.Create<Int16>();
            SimpleTypeCasterFactory<UInt32>.Create<UInt16>();
            SimpleTypeCasterFactory<UInt32>.Create<Int32>();
            SimpleTypeCasterFactory<UInt32>.Create<UInt32>();
            SimpleTypeCasterFactory<UInt32>.Create<Int64>();
            SimpleTypeCasterFactory<UInt32>.Create<UInt64>();
            SimpleTypeCasterFactory<UInt32>.Create<Single>();
            SimpleTypeCasterFactory<UInt32>.Create<Double>();
            SimpleTypeCasterFactory<UInt32>.Create<Decimal>();
            SimpleTypeCasterFactory<Int64>.Create<Boolean>();
            SimpleTypeCasterFactory<Int64>.Create<Char>();
            SimpleTypeCasterFactory<Int64>.Create<SByte>();
            SimpleTypeCasterFactory<Int64>.Create<Byte>();
            SimpleTypeCasterFactory<Int64>.Create<Int16>();
            SimpleTypeCasterFactory<Int64>.Create<UInt16>();
            SimpleTypeCasterFactory<Int64>.Create<Int32>();
            SimpleTypeCasterFactory<Int64>.Create<UInt32>();
            SimpleTypeCasterFactory<Int64>.Create<Int64>();
            SimpleTypeCasterFactory<Int64>.Create<UInt64>();
            SimpleTypeCasterFactory<Int64>.Create<Single>();
            SimpleTypeCasterFactory<Int64>.Create<Double>();
            SimpleTypeCasterFactory<Int64>.Create<Decimal>();
            SimpleTypeCasterFactory<UInt64>.Create<Boolean>();
            SimpleTypeCasterFactory<UInt64>.Create<Char>();
            SimpleTypeCasterFactory<UInt64>.Create<SByte>();
            SimpleTypeCasterFactory<UInt64>.Create<Byte>();
            SimpleTypeCasterFactory<UInt64>.Create<Int16>();
            SimpleTypeCasterFactory<UInt64>.Create<UInt16>();
            SimpleTypeCasterFactory<UInt64>.Create<Int32>();
            SimpleTypeCasterFactory<UInt64>.Create<UInt32>();
            SimpleTypeCasterFactory<UInt64>.Create<Int64>();
            SimpleTypeCasterFactory<UInt64>.Create<UInt64>();
            SimpleTypeCasterFactory<UInt64>.Create<Single>();
            SimpleTypeCasterFactory<UInt64>.Create<Double>();
            SimpleTypeCasterFactory<UInt64>.Create<Decimal>();
            SimpleTypeCasterFactory<Single>.Create<Boolean>();
            SimpleTypeCasterFactory<Single>.Create<Char>();
            SimpleTypeCasterFactory<Single>.Create<SByte>();
            SimpleTypeCasterFactory<Single>.Create<Byte>();
            SimpleTypeCasterFactory<Single>.Create<Int16>();
            SimpleTypeCasterFactory<Single>.Create<UInt16>();
            SimpleTypeCasterFactory<Single>.Create<Int32>();
            SimpleTypeCasterFactory<Single>.Create<UInt32>();
            SimpleTypeCasterFactory<Single>.Create<Int64>();
            SimpleTypeCasterFactory<Single>.Create<UInt64>();
            SimpleTypeCasterFactory<Single>.Create<Single>();
            SimpleTypeCasterFactory<Single>.Create<Double>();
            SimpleTypeCasterFactory<Single>.Create<Decimal>();
            SimpleTypeCasterFactory<Double>.Create<Boolean>();
            SimpleTypeCasterFactory<Double>.Create<Char>();
            SimpleTypeCasterFactory<Double>.Create<SByte>();
            SimpleTypeCasterFactory<Double>.Create<Byte>();
            SimpleTypeCasterFactory<Double>.Create<Int16>();
            SimpleTypeCasterFactory<Double>.Create<UInt16>();
            SimpleTypeCasterFactory<Double>.Create<Int32>();
            SimpleTypeCasterFactory<Double>.Create<UInt32>();
            SimpleTypeCasterFactory<Double>.Create<Int64>();
            SimpleTypeCasterFactory<Double>.Create<UInt64>();
            SimpleTypeCasterFactory<Double>.Create<Single>();
            SimpleTypeCasterFactory<Double>.Create<Double>();
            SimpleTypeCasterFactory<Double>.Create<Decimal>();
            SimpleTypeCasterFactory<Decimal>.Create<Boolean>();
            SimpleTypeCasterFactory<Decimal>.Create<Char>();
            SimpleTypeCasterFactory<Decimal>.Create<SByte>();
            SimpleTypeCasterFactory<Decimal>.Create<Byte>();
            SimpleTypeCasterFactory<Decimal>.Create<Int16>();
            SimpleTypeCasterFactory<Decimal>.Create<UInt16>();
            SimpleTypeCasterFactory<Decimal>.Create<Int32>();
            SimpleTypeCasterFactory<Decimal>.Create<UInt32>();
            SimpleTypeCasterFactory<Decimal>.Create<Int64>();
            SimpleTypeCasterFactory<Decimal>.Create<UInt64>();
            SimpleTypeCasterFactory<Decimal>.Create<Single>();
            SimpleTypeCasterFactory<Decimal>.Create<Double>();
            SimpleTypeCasterFactory<Decimal>.Create<Decimal>();
        }
    }
}
