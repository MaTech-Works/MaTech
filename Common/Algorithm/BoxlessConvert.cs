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
    
    /// A generic wrapper of System.Convert.ChangeType, boxless for value types.
    /// Also supports passthrough conversions between two same generic types.
    /// Ideas from https://stackoverflow.com/a/45508419 and https://stackoverflow.com/a/60395130
    public static partial class BoxlessConvert {
        // todo: support nullable source types (not only a nullable value), since nullable is a value type as well
        //       see https://stackoverflow.com/questions/3531318/convert-changetype-fails-on-nullable-types

        public static class To<TResult> {
            /// Convert to the same or a different type;
            ///
            /// Firstly, a specific ToXXX method in IConvertible is called if matching the type,
            /// then invokes IBoxlessConvertible if implemented.
            /// Returns null if TSource does not implement IConvertible or IBoxlessConvertible.
            public static TResult? From<TSource>(in TSource source, IFormatProvider? provider) {
                return InvokeCasterNullable(MaybeConvertibleCast<TSource, TResult>.caster, source, provider);
            }
            
            /// Constrain TSource to be a IConvertible
            public static TResult? FromConvertible<TSource>(in TSource source, IFormatProvider? provider) where TSource : IConvertible {
                return InvokeCasterNullable(ConvertibleCast<TSource, TResult>.caster, source, provider);
            }

            /// Constrain TSource to be a IBoxlessConvertible
            public static TResult FromBoxlessConvertible<TSource>(in TSource source, IFormatProvider? provider) where TSource : IBoxlessConvertible {
                return InvokeCaster(BoxlessConvertibleCast<TSource, TResult>.caster, source, provider);
            }

            /// Convert to the same type, useful for generic types.
            public static TResult? FromIdentity<TSource>(in TSource source) {
                return InvokeCasterNullable(IdentityCast<TSource, TResult>.caster, source);
            }
        }

        /// Method similar to System.Convert.ChangeType.
        ///
        /// Firstly, a specific ToXXX method in IConvertible is called if matching the type,
        /// then invokes IBoxlessConvertible if implemented.
        /// If all failed, a InvalidCastException is thrown.
        public static TResult ChangeType<TSource, TResult>(in TSource source, IFormatProvider? provider) where TSource : IConvertible {
            TResult? result = InvokeCasterNullable(ConvertibleCast<TSource, TResult>.caster, source, provider);
            if (result is null) {
                throw new InvalidCastException($"BoxlessConvert: Cast from {nameof(TSource)} to {nameof(TResult)} undefined in IConvertible or IBoxlessConvertible.");
            }
            return result;
        }

        /// Convert to the same type, useful for generic types.
        ///
        /// TSource can be an arbitrary type that does not implement IConvertible or IBoxlessConvertible.
        /// Throws a InvalidCastException if TSource is not TResult; use BoxlessConvert.From.ToIdentity for a nullable result.
        public static TResult Identity<TSource, TResult>(in TSource source) {
            TResult? result = InvokeCasterNullable(IdentityCast<TSource, TResult>.caster, source);
            if (result is null) {
                throw new InvalidCastException($"BoxlessConvert: Invalid identity cast from {nameof(TSource)} to {nameof(TResult)}, they are not the same type.");
            }
            return result;
        }
    }
}
