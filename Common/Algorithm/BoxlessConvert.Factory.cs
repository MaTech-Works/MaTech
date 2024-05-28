// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;

namespace MaTech.Common.Algorithm {
    public static partial class BoxlessConvert {
        // Here is the entry point of factories of caster delegates.
        
        private static class CasterFactory<TSource> {
            private static readonly FactoryWithStricterConstraint<TSource> factoryEnum = new FactoryWithStricterConstraint<TSource>(typeof(EnumCasterFactory<>));
            private static readonly FactoryWithStricterConstraint<TSource> factorySimpleType = new FactoryWithStricterConstraint<TSource>(typeof(SimpleTypeCasterFactory<>));
            private static readonly FactoryWithStricterConstraint<TSource> factoryConvertible = new FactoryWithStricterConstraint<TSource>(typeof(ConvertibleCasterFactory<>));
            private static readonly FactoryWithStricterConstraint<TSource> factoryBoxless = new FactoryWithStricterConstraint<TSource>(typeof(BoxlessConvertibleCasterFactory<>));

            [Preserve]
            public static Caster<TSource, TResult>? Create<TResult>() {
                return factoryEnum.CreateIfValid<TResult>()
                    ?? CasterFactory<TResult>.factoryEnum.CreateReversedIfValid<TSource>()
                    ?? factorySimpleType.CreateIfValid<TResult>()
                    ?? factoryConvertible.CreateIfValid<TResult>()
                    ?? factoryBoxless.CreateIfValid<TResult>();
            }
        }
        
        /// Magic.
        /// Allow calling the generic method "Create" and "CreateReversed" with a stricter constraint.
        /// Done with reflections since il2cpp does not support "dynamic" keyword.
        /// MakeGenericType/MakeGenericMethod still checks against the constraints, but dynamically, which we assume with IsAssignableFrom.
        private readonly struct FactoryWithStricterConstraint<TSource> {
            private readonly MethodInfo? methodCreate;
            private readonly MethodInfo? methodCreateReversed;

            public FactoryWithStricterConstraint(Type typeGenericFactory) {
                var constraints = typeGenericFactory.GetGenericArguments()[0].GetGenericParameterConstraints();
                if (constraints.Any(constraint => !constraint.IsAssignableFrom(typeof(TSource)))) {
                    methodCreate = null;
                    methodCreateReversed = null;
                    return;
                }
                var typeSpecific = typeGenericFactory.MakeGenericType(typeof(TSource));
                methodCreate = typeSpecific.GetMethod("Create");
                methodCreateReversed = typeSpecific.GetMethod("CreateReversed");
            }

            public Caster<TSource, TResult>? CreateIfValid<TResult>() {
                return (Caster<TSource, TResult>?)methodCreate?.MakeGenericMethod(typeof(TResult)).Invoke(null, null);
            }

            public Caster<TResult, TSource>? CreateReversedIfValid<TResult>() {
                return (Caster<TResult, TSource>?)methodCreateReversed?.MakeGenericMethod(typeof(TResult)).Invoke(null, null);
            }
        }
    }
}
