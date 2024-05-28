// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using Optional;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// Implement this interface to extend the behavior of BoxlessConvert.
    /// Additionally, you can specify which types are convertible by implementing a static method <c>[Preserve] public static bool IsBoxlessConvertibleToType(Type type)</c>
    /// in the converted struct/class or any of its parent class if exists; constraint on <c>T</c> is allowed.
    /// Attribute <c>[Preserve]</c> (under namespace UnityEngine.Scripting) is required to prevent static method trimmed by code trimming.
    /// <example>
    /// <code>
    /// public struct Foo {
    ///     [Preserve] // to avoid trimming on AOT platforms -- IsBoxlessConvertibleToType is called by reflection since no static abstract support for now
    ///     public static bool IsBoxlessConvertibleToType(Type type) => type == typeof(int) || type == typeof(float);
    /// 
    ///     public T ToType&lt;T&gt;(IFormatProvider? provider) {
    ///         if (typeof(T) == typeof(int)) return BoxlessConvert.Identity&lt;int, T&gt;(765);
    ///         if (typeof(T) == typeof(float)) return BoxlessConvert.ChangeType&lt;int, T&gt;(573);
    ///         throw new InvalidCastException("this branch should not be reached");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public interface IBoxlessConvertible {
        #if NET7_0_OR_GREATER
        //static abstract bool IsBoxlessConvertibleToType<TResult>();  // we will use this one day eventually...
        #endif
        TResult ToType<TResult>(IFormatProvider? provider);
    }
    
    /// <summary>
    /// A set of generic methods that mimics System.Convert.ChangeType, while stay boxless in the conversion process, and doing passthrough between two same generic types.
    /// Inspired from https://stackoverflow.com/a/45508419 and https://stackoverflow.com/a/60395130.
    /// <para/>
    /// Not all conversion cases are covered.
    /// At present, these conversions are supported (listed in order of priority of selection):
    /// <list type="bullet">
    /// <item> From enum to any integral type </item>
    /// <item> From any integral type to enum </item>
    /// <item> Between value types supported by non-boxing methods <c>Convert.ToXXX</c> </item>
    /// <item> From a <c>IConvertible</c> to a type supported by any method in <c>IConvertible</c> except <c>IConvertible.ToType</c> </item>
    /// <item> From a <c>IBoxlessConvertible</c> to a type supported by non-boxing methods <c>IConvertible.ToXXX</c> (no down-casting is performed for structs input) </item>
    /// </list>
    /// </summary>
    public static partial class BoxlessConvert {
        // TODO: Support IntPtr and UIntPtr
        // TODO: Support passthrough for assignable
        // TODO: Support unpacking Nullable
        // TODO: Support IntPtr and UIntPtr

        /// Throws when cast is not supported.
        public static class To<TResult> {
            /// Return default when conversion is not supported.
            public static TResult From<TSource>(in TSource? source, IFormatProvider? provider = null) {
                return InvokeCaster(Cast<TSource, TResult>.caster, source, provider).ValueOrThrow();
            }

            /// Return the source when converting between the same type; return default otherwise. Useful for generic types.
            public static TResult FromIdentity<TSource>(in TSource? source) {
                return InvokeIdentityCaster<TSource, TResult>(source).ValueOrThrow();
            }
            
            /// Convert only using IConvertible interface
            public static TResult FromIConvertible<TSource>(in TSource? source, IFormatProvider? provider = null) where TSource : IConvertible {
                return InvokeCaster(ConvertibleCast<TSource, TResult>.caster, source, provider).ValueOrThrow();
            }

            /// Convert only using IBoxlessConvertible interface
            public static TResult FromIBoxlessConvertible<TSource>(in TSource? source, IFormatProvider? provider = null) where TSource : IBoxlessConvertible {
                return InvokeCaster(BoxlessConvertibleCast<TSource, TResult>.caster, source, provider).ValueOrThrow();
            }
        }
        
        /// Packed exception into <c>Option&lt;TResult, Exception&gt;</c> instead of throwing for failed conversion.
        public static class MaybeTo<TResult> {
            /// Return default when conversion is not supported.
            public static Option<TResult, Exception> From<TSource>(in TSource? source, IFormatProvider? provider = null) {
                return InvokeCaster(Cast<TSource, TResult>.caster, source, provider);
            }

            /// Return the source when converting between the same type; return default otherwise. Useful for generic types.
            public static Option<TResult, Exception> FromIdentity<TSource>(in TSource? source) {
                return InvokeIdentityCaster<TSource, TResult>(source);
            }
            
            /// Convert only using IConvertible interface
            public static Option<TResult, Exception> FromIConvertible<TSource>(in TSource? source, IFormatProvider? provider = null) where TSource : IConvertible {
                return InvokeCaster(ConvertibleCast<TSource, TResult>.caster, source, provider);
            }

            /// Convert only using IBoxlessConvertible interface
            public static Option<TResult, Exception> FromIBoxlessConvertible<TSource>(in TSource? source, IFormatProvider? provider = null) where TSource : IBoxlessConvertible {
                return InvokeCaster(BoxlessConvertibleCast<TSource, TResult>.caster, source, provider);
            }
        }

        /// Method with a signature similar to System.Convert.ChangeType and similarly throws InvalidCastException on invalid conversions.
        public static TResult ChangeType<TSource, TResult>(in TSource source, IFormatProvider? provider) where TSource : IConvertible {
            return InvokeCaster(Cast<TSource, TResult>.caster, source, provider).ValueOrThrow();
        }

        /// Convert to the same type, useful for generic types. Throws InvalidCastException if TSource is not TResult.
        public static TResult? Identity<TSource, TResult>(in TSource? source) {
            return InvokeIdentityCaster<TSource, TResult>(source).ValueOrThrow();
        }
    }
}
