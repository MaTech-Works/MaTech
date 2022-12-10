// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

#nullable enable

namespace MaTech.Common.Algorithm {
    /// A generic wrapper of System.Convert, boxless for primitive types.
    /// Ideas from https://stackoverflow.com/a/45508419 and https://stackoverflow.com/a/60395130
    public static partial class BoxlessConvert {
        private delegate TResult Caster<TSource, TResult>(in TSource source, IFormatProvider? provider);

        private static Caster<TSource, TResult> CreateIdentityCaster<TSource, TResult>() {
            return (Caster<TSource, TResult>)(Delegate)(Caster<TSource, TSource>) delegate (in TSource source, IFormatProvider? provider) { return source; };
        }
        private static Caster<TSource, TResult> CreateInvalidCaster<TSource, TResult>(string message) {
            return delegate (in TSource source, IFormatProvider? provider) { throw new InvalidCastException(message); };
        }

        private static TResult InvokeCaster<TSource, TResult>(Caster<TSource, TResult> caster, in TSource source, IFormatProvider? provider = null) {
            return caster.Invoke(source, provider);
        }
        private static TResult? InvokeCasterNullable<TSource, TResult>(Caster<TSource, TResult>? caster, in TSource source, IFormatProvider? provider = null) {
            return caster is null ? default : caster.Invoke(source, provider);
        }

        private static class IdentityCast<TSource, TResult> {
            public static readonly Caster<TSource, TResult>? caster = typeof(TSource) == typeof(TResult) ? CreateIdentityCaster<TSource, TResult>() : null;
        }

        private static class MaybeConvertibleCast<TSource, TResult> {
            public static readonly Caster<TSource, TResult>? caster = IdentityCast<TSource, TResult>.caster ?? MaybeConvertibleCasterFactory<TSource>.Create<TResult>();
        }

        private static class ConvertibleCast<TSource, TResult> where TSource : IConvertible {
            public static readonly Caster<TSource, TResult>? caster = IdentityCast<TSource, TResult>.caster ?? ConvertibleCasterFactory<TSource>.Create<TResult>();
        }

        private static class BoxlessConvertibleCast<TSource, TResult> where TSource : IBoxlessConvertible {
            public static readonly Caster<TSource, TResult> caster = IdentityCast<TSource, TResult>.caster ?? BoxlessConvertibleCasterFactory<TSource>.Create<TResult>();
        }
    }
}
