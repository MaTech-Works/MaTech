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
        // Here we implement factories of casting delegates for IConvertible and IBoxlessConvertible.
        
        private static class BoxlessConvertibleCasterFactory<TSource> where TSource : IBoxlessConvertible {
            [Preserve]
            public static Caster<TSource, TResult> Create<TResult>() {
                return delegate (in TSource source, IFormatProvider? provider) { return source.ToType<TResult>(provider); };
            }
        }

        private static class ConvertibleCasterFactory<TSource> where TSource : IConvertible {
            private static readonly Dictionary<Type, Func<Delegate>> dict = new Dictionary<Type, Func<Delegate>>{
                { typeBoolean, () => new Caster<TSource, Boolean>(delegate (in TSource source, IFormatProvider? provider) { return source.ToBoolean(provider); }) },
                { typeChar, () => new Caster<TSource, Char>(delegate (in TSource source, IFormatProvider? provider) { return source.ToChar(provider); }) },

                { typeSByte, () => new Caster<TSource, SByte>(delegate (in TSource source, IFormatProvider? provider) { return source.ToSByte(provider); }) },
                { typeByte, () => new Caster<TSource, Byte>(delegate (in TSource source, IFormatProvider? provider) { return source.ToByte(provider); }) },
                { typeInt16, () => new Caster<TSource, Int16>(delegate (in TSource source, IFormatProvider? provider) { return source.ToInt16(provider); }) },
                { typeUInt16, () => new Caster<TSource, UInt16>(delegate (in TSource source, IFormatProvider? provider) { return source.ToUInt16(provider); }) },
                { typeInt32, () => new Caster<TSource, Int32>(delegate (in TSource source, IFormatProvider? provider) { return source.ToInt32(provider); }) },
                { typeUInt32, () => new Caster<TSource, UInt32>(delegate (in TSource source, IFormatProvider? provider) { return source.ToUInt32(provider); }) },
                { typeInt64, () => new Caster<TSource, Int64>(delegate (in TSource source, IFormatProvider? provider) { return source.ToInt64(provider); }) },
                { typeUInt64, () => new Caster<TSource, UInt64>(delegate (in TSource source, IFormatProvider? provider) { return source.ToUInt64(provider); }) },

                { typeSingle, () => new Caster<TSource, Single>(delegate (in TSource source, IFormatProvider? provider) { return source.ToSingle(provider); }) },
                { typeDouble, () => new Caster<TSource, Double>(delegate (in TSource source, IFormatProvider? provider) { return source.ToDouble(provider); }) },
                { typeDecimal, () => new Caster<TSource, Decimal>(delegate (in TSource source, IFormatProvider? provider) { return source.ToDecimal(provider); }) },

                { typeDateTime, () => new Caster<TSource, DateTime>(delegate (in TSource source, IFormatProvider? provider) { return source.ToDateTime(provider); }) },
                { typeString, () => new Caster<TSource, String>(delegate (in TSource source, IFormatProvider? provider) { return source.ToString(provider); }) },
            };

            private static readonly FactoryWithStricterConstraint<TSource> factoryBoxless = new FactoryWithStricterConstraint<TSource>(typeof(BoxlessConvertibleCasterFactory<>));

            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                if (SimpleTypeCasterFactory<TSource>.Create<TResult>() is Caster<TSource, TResult> caster)
                    return caster;
                if (dict.TryGetValue(typeof(TResult), out var factoryFunc))
                    return (Caster<TSource, TResult>)factoryFunc();
                return factoryBoxless.CreateIfValid<TResult>();
            }
        }
    }
}
