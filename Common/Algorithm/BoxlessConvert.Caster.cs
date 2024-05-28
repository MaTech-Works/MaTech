// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Optional;

// ReSharper disable StaticMemberInGenericType

namespace MaTech.Common.Algorithm {
    public static partial class BoxlessConvert {
        // Here we do book-keeping of casting delegates on each pair of casted types

        // ReSharper disable once TypeParameterCanBeVariant
        private delegate TResult Caster<TSource, TResult>(in TSource source, IFormatProvider? provider);

        private static Caster<TSource, TResult> CreateIdentityCaster<TSource, TResult>() {
            return (Caster<TSource, TResult>)(Delegate)(Caster<TSource, TSource>)((in TSource source, IFormatProvider? provider) => source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<TResult, Exception> InvokeCaster<TSource, TResult>(Caster<TSource, TResult>? caster, in TSource? source, IFormatProvider? provider = null) {
            if (source is null) return FailedConversion<TSource, TResult>.NullSource;
            if (caster is null) return FailedConversion<TSource, TResult>.Unsupported;
            try {
                return Option.Some<TResult, Exception>(caster.Invoke(source, provider));
            } catch (Exception e) {
                return FailedConversion<TSource, TResult>.Caught(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<TResult, Exception> InvokeIdentityCaster<TSource, TResult>(in TSource? source) {
            var caster = IdentityCast<TSource, TResult>.caster;
            if (caster is null) return FailedConversion<TSource, TResult>.NotIdentity;
            if (source is null) return Option.Some<TResult, Exception>(default!);
            try {
                return Option.Some<TResult, Exception>(caster.Invoke(source, null));
            } catch (Exception e) {
                return FailedConversion<TSource, TResult>.Caught(e);
            }
        }

        private static class IdentityCast<TSource, TResult> {
            public static readonly Caster<TSource, TResult>? caster = typeof(TSource) == typeof(TResult) ? CreateIdentityCaster<TSource, TResult>() : null;
        }

        private static class Cast<TSource, TResult> {
            public static readonly Caster<TSource, TResult>? caster = IdentityCast<TSource, TResult>.caster ?? CasterFactory<TSource>.Create<TResult>();
        }
        private static class ConvertibleCast<TSource, TResult> where TSource : IConvertible {
            public static readonly Caster<TSource, TResult>? caster = IdentityCast<TSource, TResult>.caster ?? ConvertibleCasterFactory<TSource>.Create<TResult>();
        }
        private static class BoxlessConvertibleCast<TSource, TResult> where TSource : IBoxlessConvertible {
            public static readonly Caster<TSource, TResult>? caster = IdentityCast<TSource, TResult>.caster ?? BoxlessConvertibleCasterFactory<TSource>.Create<TResult>();
        }

        private static class FailedConversion<TSource, TResult> {
            public static Option<TResult, Exception> Caught(Exception e) => Option.None<TResult, Exception>(e);
            public static Option<TResult, Exception> Unsupported => Option.None<TResult, Exception>(new InvalidCastException(
                $"[BoxlessConvert] Conversion from {nameof(TSource)} to {nameof(TResult)} is unsupported. Implement IBoxlessConvertible and/or IConvertible to support boxless conversion."));
            public static Option<TResult, Exception> NotIdentity => Option.None<TResult, Exception>(new InvalidCastException(
                $"[BoxlessConvert] Invalid identity cast from {nameof(TSource)} to {nameof(TResult)}; they are not the same type."));
            public static Option<TResult, Exception> NullSource => Option.None<TResult, Exception>(new InvalidCastException(
                $"[BoxlessConvert] Cannot convert null except for identity cast."));
        }
    }
}
