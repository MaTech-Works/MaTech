// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.Scripting;

#nullable enable

namespace MaTech.Common.Algorithm {
    public static partial class BoxlessConvert {
        private static class EnumCasterFactory<TEnum> where TEnum : unmanaged, Enum, IConvertible {
            private static Dictionary<Type, (Func<Delegate?>, Func<Delegate?>)> dict; // T --> (Func<Caster<TEnum, T>>, Func<Caster<T, TEnum>>)

            private static unsafe TResult ReinterpretCast<TSource, TResult>(TSource value) where TSource : unmanaged where TResult : unmanaged {
                Assert.AreEqual(sizeof(TSource), sizeof(TResult));
                return *(TResult*)&value;
            }

            private static (Func<Delegate?>, Func<Delegate?>) CreateCasterFactoryFuncTuple<TUnderlying, T>() where TUnderlying : unmanaged {
                Assert.AreEqual(typeof(TUnderlying), Enum.GetUnderlyingType(typeof(TEnum)));
                return (
                    () => Cast<TUnderlying, T>.caster is null ? null : new Caster<TEnum, T>(delegate (in TEnum source, IFormatProvider? provider) {
                        return Cast<TUnderlying, T>.caster(ReinterpretCast<TEnum, TUnderlying>(source), provider);
                    }),
                    () => Cast<T, TUnderlying>.caster is null ? null : new Caster<T, TEnum>(delegate (in T source, IFormatProvider? provider) {
                        return ReinterpretCast<TUnderlying, TEnum>(Cast<T, TUnderlying>.caster(source, provider));
                    })
                );
            }

            private static void InitializeDictForUnderlyingType<TUnderlying>() where TUnderlying : unmanaged {
                dict = new Dictionary<Type, (Func<Delegate?>, Func<Delegate?>)>{
                    { typeSByte, CreateCasterFactoryFuncTuple<TUnderlying, SByte>() },
                    { typeByte, CreateCasterFactoryFuncTuple<TUnderlying, Byte>() },
                    { typeInt16, CreateCasterFactoryFuncTuple<TUnderlying, Int16>() },
                    { typeUInt16, CreateCasterFactoryFuncTuple<TUnderlying, UInt16>() },
                    { typeInt32, CreateCasterFactoryFuncTuple<TUnderlying, Int32>() },
                    { typeUInt32, CreateCasterFactoryFuncTuple<TUnderlying, UInt32>() },
                    { typeInt64, CreateCasterFactoryFuncTuple<TUnderlying, Int64>() },
                    { typeUInt64, CreateCasterFactoryFuncTuple<TUnderlying, UInt64>() },
                };
            }

            static EnumCasterFactory() {
                var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
                if (underlyingType == typeSByte) InitializeDictForUnderlyingType<SByte>();
                if (underlyingType == typeByte) InitializeDictForUnderlyingType<Byte>();
                if (underlyingType == typeInt16) InitializeDictForUnderlyingType<Int16>();
                if (underlyingType == typeUInt16) InitializeDictForUnderlyingType<UInt16>();
                if (underlyingType == typeInt32) InitializeDictForUnderlyingType<Int32>();
                if (underlyingType == typeUInt32) InitializeDictForUnderlyingType<UInt32>();
                if (underlyingType == typeInt64) InitializeDictForUnderlyingType<Int64>();
                if (underlyingType == typeUInt64) InitializeDictForUnderlyingType<UInt64>();
                throw new NotSupportedException($"[BoxlessConvert] The underlying type of enum is {underlyingType}, which is not supported. Is this something new in C#?");
            }

            [Preserve]
            public static Caster<TEnum, TResult>? Create<TResult>() {
                if (dict.TryGetValue(typeof(TResult), out var factory))
                    return (Caster<TEnum, TResult>?)factory.Item1();
                return null;
            }

            [Preserve]
            public static Caster<TSource, TEnum>? CreateReversed<TSource>() {
                if (dict.TryGetValue(typeof(TSource), out var factory))
                    return (Caster<TSource, TEnum>?)factory.Item2();
                return null;
            }
        }
    }
}
