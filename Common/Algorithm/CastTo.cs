// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;

namespace MaTech.Common.Algorithm {
    // Code from https://stackoverflow.com/questions/1189144/c-sharp-non-boxing-conversion-of-generic-enum-to-int

    /// <summary>
    /// Class to cast to type <see cref="TTarget"/>
    /// </summary>
    /// <typeparam name="TTarget">Target type</typeparam>
    public static class CastTo<TTarget> {
        /// <summary>
        /// Casts value of type <see cref="TSource"/> to <see cref="TTarget"/>.
        /// This does not cause boxing for value types.
        /// Useful in generic methods.
        /// </summary>
        /// <typeparam name="TSource">Source type to cast from. Usually a generic type.</typeparam>
        public static TTarget From<TSource>(TSource s) {
            return Cache<TSource, TTarget>.caster(s);
        }

        // ReSharper disable once InconsistentNaming
        private static class Cache<S, T> {
            public static readonly Func<S, T> caster = Get();

            private static Func<S, T> Get() {
                var p = Expression.Parameter(typeof(S));
                var c = Expression.ConvertChecked(p, typeof(T));
                return Expression.Lambda<Func<S, T>>(c, p).Compile();
            }
        }
    }
}