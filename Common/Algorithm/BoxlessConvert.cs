// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace MaTech.Common.Algorithm {
    /// Implement this interface to extend the behavior of BoxlessConvert.
    /// Any result type for BoxlessConvert will match a interface in IConvertible first, then fallback to this generic method.
    ///
    /// Please try to use BoxlessConvert.IdentityCast when casting a known-typed variable to the generic type.
    public interface IBoxlessConvertible : IConvertible {
        TResult ToType<TResult>(IFormatProvider? provider);
    }
    
    /// A generic wrapper of System.Convert, boxless for value types.
    /// Ideas from https://stackoverflow.com/a/45508419 and https://stackoverflow.com/a/60395130
    public static partial class BoxlessConvert {
        // todo: support nullable source types (not only a nullable value), since nullable is a value type as well
        //       see https://stackoverflow.com/questions/3531318/convert-changetype-fails-on-nullable-types

        public static class From<TSource> {
            /// Convert to the same type, useful for generic types.
            public static class ToIdentity<TResult> {
                public static bool IsDefined => IdentityCast<TSource, TResult>.caster != null;
                public static TResult? Cast(in TSource source) => InvokeCaster(IdentityCast<TSource, TResult>.caster, source);
            }

            /// Convert to the same or a different type;
            /// will fail if TSource does not implement IConvertible or IBoxlessConvertible.
            public static class To<TResult> {
                public static bool IsDefined => MaybeConvertibleCast<TSource, TResult>.caster != null;
                public static TResult? Cast(in TSource source, IFormatProvider? provider = null) => InvokeCaster(MaybeConvertibleCast<TSource, TResult>.caster, source, provider);
            }
        }

        /// Constrain TSource to be a IConvertible
        public static class FromConvertible<TSource> where TSource : IConvertible {
            public static class To<TResult> {
                public static bool IsDefined => ConvertibleCast<TSource, TResult>.caster != null;
                public static TResult? Cast(in TSource source, IFormatProvider? provider = null) => InvokeCaster(ConvertibleCast<TSource, TResult>.caster, source, provider);
            }
        }

        /// Constrain TSource to be a IBoxlessConvertible
        public static class FromBoxlessConvertible<TSource> where TSource : IBoxlessConvertible {
            public static class To<TResult> {
                public static bool IsDefined => BoxlessConvertibleCast<TSource, TResult>.caster != null;
                public static TResult? Cast(in TSource source, IFormatProvider? provider = null) => InvokeCaster(BoxlessConvertibleCast<TSource, TResult>.caster, source, provider);
            }
        }

        /// Method similar to Convert.ChangeType
        public static TResult ChangeType<TSource, TResult>(in TSource source, IFormatProvider provider) where TSource : IConvertible {
            TResult? result = InvokeCaster(ConvertibleCast<TSource, TResult>.caster, source, provider);
            if (result is null) {
                throw new InvalidCastException($"BoxlessConvert: Cast from {nameof(TSource)} to {nameof(TResult)} undefined in IConvertible or IBoxlessConvertible.");
            }
            return result;
        }

        /// Convert to the same type, useful for generic types.
        /// Throws if cast undefined, use BoxlessConvert.From.ToIdentity for a nullable result.
        public static TResult Identity<TSource, TResult>(in TSource source) {
            TResult? result = InvokeCaster(IdentityCast<TSource, TResult>.caster, source);
            if (result is null) {
                throw new InvalidCastException($"BoxlessConvert: Invalid identity cast from {nameof(TSource)} to {nameof(TResult)}, they are not the same type.");
            }
            return result;
        }
    }
}
