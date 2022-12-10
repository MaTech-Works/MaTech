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
    /// A generic wrapper of System.Convert, boxless for primitive types.
    /// Ideas from https://stackoverflow.com/a/45508419 and https://stackoverflow.com/a/60395130
    public static partial class BoxlessConvert {
        private static class BoxlessConvertibleCasterFactory<TSource> where TSource : IBoxlessConvertible {
            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                return delegate (in TSource source, IFormatProvider? provider) { return source.ToType<TResult>(provider); };
            }
        }

        private static class ConvertibleCasterFactory<TSource> where TSource : IConvertible {
            private static readonly Dictionary<Type, Func<Delegate>> dict = new Dictionary<Type, Func<Delegate>>{
                { typeof(bool), () => new Caster<TSource, bool>(delegate (in TSource source, IFormatProvider? provider) { return source.ToBoolean(provider); }) },

                { typeof(sbyte), () => new Caster<TSource, sbyte>(delegate (in TSource source, IFormatProvider? provider) { return source.ToSByte(provider); }) },
                { typeof(short), () => new Caster<TSource, short>(delegate (in TSource source, IFormatProvider? provider) { return source.ToInt16(provider); }) },
                { typeof(int), () => new Caster<TSource, int>(delegate (in TSource source, IFormatProvider? provider) { return source.ToInt32(provider); }) },
                { typeof(long), () => new Caster<TSource, long>(delegate (in TSource source, IFormatProvider? provider) { return source.ToInt64(provider); }) },

                { typeof(byte), () => new Caster<TSource, byte>(delegate (in TSource source, IFormatProvider? provider) { return source.ToByte(provider); }) },
                { typeof(ushort), () => new Caster<TSource, ushort>(delegate (in TSource source, IFormatProvider? provider) { return source.ToUInt16(provider); }) },
                { typeof(uint), () => new Caster<TSource, uint>(delegate (in TSource source, IFormatProvider? provider) { return source.ToUInt32(provider); }) },
                { typeof(ulong), () => new Caster<TSource, ulong>(delegate (in TSource source, IFormatProvider? provider) { return source.ToUInt64(provider); }) },

                { typeof(float), () => new Caster<TSource, float>(delegate (in TSource source, IFormatProvider? provider) { return source.ToSingle(provider); }) },
                { typeof(double), () => new Caster<TSource, double>(delegate (in TSource source, IFormatProvider? provider) { return source.ToDouble(provider); }) },
                { typeof(decimal), () => new Caster<TSource, decimal>(delegate (in TSource source, IFormatProvider? provider) { return source.ToDecimal(provider); }) },

                { typeof(char), () => new Caster<TSource, char>(delegate (in TSource source, IFormatProvider? provider) { return source.ToChar(provider); }) },
                { typeof(DateTime), () => new Caster<TSource, DateTime>(delegate (in TSource source, IFormatProvider? provider) { return source.ToDateTime(provider); }) },

                { typeof(string), () => new Caster<TSource, string>(delegate (in TSource source, IFormatProvider? provider) { return source.ToString(provider); }) },
            };

            private static ReflectedFactory<TSource> factoryBoxless = new ReflectedFactory<TSource>(typeof(BoxlessConvertibleCasterFactory<>));

            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                if (dict.TryGetValue(typeof(TResult), out var factoryFunc))
                    return (Caster<TSource, TResult>)factoryFunc();
                return factoryBoxless.CreateIfValid<TResult>();
            }
        }
        
        private static class MaybeConvertibleCasterFactory<TSource> {
            private static ReflectedFactory<TSource> factoryNormal = new ReflectedFactory<TSource>(typeof(ConvertibleCasterFactory<>));

            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                return factoryNormal.CreateIfValid<TResult>();
            }
        }

        /// To allow calling the generic method "Create" with a stricter constraint.
        /// "dynamic" keyword is not viable, so we need to do it with reflections.
        /// MakeGenericType/MakeGenericMethod still checks against the constraints, but dynamically, which we assume with IsAssignableFrom.
        private readonly struct ReflectedFactory<TSource> {
            private readonly MethodInfo? methodCreate;

            public ReflectedFactory(Type typeGenericFactory) {
                var contraints = typeGenericFactory.GetGenericArguments()[0].GetGenericParameterConstraints();
                foreach (var constraint in contraints) {
                    if (!constraint.IsAssignableFrom(typeof(TSource))) {
                        methodCreate = null;
                        return;
                    }
                }
                methodCreate = typeGenericFactory.MakeGenericType(typeof(TSource)).GetMethod("Create");
            }

            public Caster<TSource, TResult>? CreateIfValid<TResult>() {
                return (Caster<TSource, TResult>?)methodCreate?.MakeGenericMethod(typeof(TResult))?.Invoke(null, null);
            }
        }
    }
}
