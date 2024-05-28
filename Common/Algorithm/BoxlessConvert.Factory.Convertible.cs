// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine.Scripting;

namespace MaTech.Common.Algorithm {
    public static partial class BoxlessConvert {
        // Here we implement factories of casting delegates for IConvertible and IBoxlessConvertible.
        
        private static class BoxlessConvertibleCasterFactory<TSource> where TSource : IBoxlessConvertible {
            private static bool IsBoxlessConvertibleToType<TResult>() {
                try {
                    var method = typeof(TSource).GetMethod("IsBoxlessConvertibleToType");
                    if (method == null) return true; // undefined means all supported
                    var constraints = method.GetGenericArguments()[0].GetGenericParameterConstraints();
                    if (constraints.Any(constraint => !constraint.IsAssignableFrom(typeof(TResult)))) {
                        return false; // failed constraint means not supported
                    }
                    var methodSpecific = method.MakeGenericMethod(typeof(TResult));
                    return (bool)methodSpecific.Invoke(null, null);
                } catch (Exception e) {
                    throw new AmbiguousImplementationException($"[BoxlessConvert] Wrong definition of `public static bool IsBoxlessConvertibleToType<T>()` in type {typeof(TSource)} implementing IBoxlessConvertible.", e);
                }
            }
            
            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                if (!IsBoxlessConvertibleToType<TResult>()) return null;
                return (in TSource source, IFormatProvider? provider) => source.ToType<TResult>(provider);
            }
        }

        private static class ConvertibleCasterFactory<TSource> where TSource : IConvertible {
            private static readonly Dictionary<Type, Func<Delegate>> dict = new Dictionary<Type, Func<Delegate>>{
                { typeBoolean, () => new Caster<TSource, Boolean>((in TSource source, IFormatProvider? provider) => source.ToBoolean(provider)) },
                { typeChar, () => new Caster<TSource, Char>((in TSource source, IFormatProvider? provider) => source.ToChar(provider)) },

                { typeSByte, () => new Caster<TSource, SByte>((in TSource source, IFormatProvider? provider) => source.ToSByte(provider)) },
                { typeByte, () => new Caster<TSource, Byte>((in TSource source, IFormatProvider? provider) => source.ToByte(provider)) },
                { typeInt16, () => new Caster<TSource, Int16>((in TSource source, IFormatProvider? provider) => source.ToInt16(provider)) },
                { typeUInt16, () => new Caster<TSource, UInt16>((in TSource source, IFormatProvider? provider) => source.ToUInt16(provider)) },
                { typeInt32, () => new Caster<TSource, Int32>((in TSource source, IFormatProvider? provider) => source.ToInt32(provider)) },
                { typeUInt32, () => new Caster<TSource, UInt32>((in TSource source, IFormatProvider? provider) => source.ToUInt32(provider)) },
                { typeInt64, () => new Caster<TSource, Int64>((in TSource source, IFormatProvider? provider) => source.ToInt64(provider)) },
                { typeUInt64, () => new Caster<TSource, UInt64>((in TSource source, IFormatProvider? provider) => source.ToUInt64(provider)) },

                { typeSingle, () => new Caster<TSource, Single>((in TSource source, IFormatProvider? provider) => source.ToSingle(provider)) },
                { typeDouble, () => new Caster<TSource, Double>((in TSource source, IFormatProvider? provider) => source.ToDouble(provider)) },
                { typeDecimal, () => new Caster<TSource, Decimal>((in TSource source, IFormatProvider? provider) => source.ToDecimal(provider)) },

                { typeDateTime, () => new Caster<TSource, DateTime>((in TSource source, IFormatProvider? provider) => source.ToDateTime(provider)) },
                { typeString, () => new Caster<TSource, String>((in TSource source, IFormatProvider? provider) => source.ToString(provider)) },
            };

            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                return dict.TryGetValue(typeof(TResult), out var factoryFunc) ? (Caster<TSource, TResult>)factoryFunc() : null;
            }
        }
    }
}
