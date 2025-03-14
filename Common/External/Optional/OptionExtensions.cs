﻿using System;

namespace Optional
{
    public static class OptionExtensions
    {
        /// <summary>
        /// Wraps an existing value in an Option&lt;T&gt; instance.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T> Some<T>(this T value) => Option.Some(value);

        /// <summary>
        /// Wraps an existing value in an Option&lt;T, TException&gt; instance.
        /// </summary>
        /// <param name="value">The value to be wrapped.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> Some<T, TException>(this T value) =>
            Option.Some<T, TException>(value);

        /// <summary>
        /// Creates an empty Option&lt;T&gt; instance from a specified value.
        /// </summary>
        /// <param name="value">A value determining the type of the optional.</param>
        /// <returns>An empty optional.</returns>
        public static Option<T> None<T>(this T value) => Option.None<T>();

        /// <summary>
        /// Creates an empty Option&lt;T, TException&gt; instance, 
        /// with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="exception">The exceptional value.</param>
        /// <returns>An empty optional.</returns>
        public static Option<T, TException> None<T, TException>(this T value, TException exception) =>
            Option.None<T, TException>(exception);

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value does not satisfy the given predicate, 
        /// an empty optional is returned.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T> SomeWhen<T>(this T value, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return predicate(value) ? Option.Some(value) : Option.None<T>();
        }

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value does not satisfy the given predicate, 
        /// an empty optional is returned, with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="exception">The exceptional value.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> SomeWhen<T, TException>(this T value, Func<T, bool> predicate, TException exception)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return predicate(value) ? Option.Some<T, TException>(value) : Option.None<T, TException>(exception);
        }

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value does not satisfy the given predicate, 
        /// an empty optional is returned, with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> SomeWhen<T, TException>(this T value, Func<T, bool> predicate, Func<TException> exceptionFactory)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return predicate(value) ? Option.Some<T, TException>(value) : Option.None<T, TException>(exceptionFactory());
        }

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value satisfies the given predicate, 
        /// an empty optional is returned.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T> NoneWhen<T>(this T value, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return value.SomeWhen(val => !predicate(val));
        }

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value satisfies the given predicate, 
        /// an empty optional is returned, with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="exception">The exceptional value.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> NoneWhen<T, TException>(this T value, Func<T, bool> predicate, TException exception)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return value.SomeWhen(val => !predicate(val), exception);
        }

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value does satisfy the given predicate, 
        /// an empty optional is returned, with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> NoneWhen<T, TException>(this T value, Func<T, bool> predicate, Func<TException> exceptionFactory)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return value.SomeWhen(val => !predicate(val), exceptionFactory);
        }

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value is null, an empty optional is returned.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T> SomeNotNull<T>(this T value) => value.SomeWhen(val => val != null);

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value is null, an empty optional is returned, 
        /// with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="exception">The exceptional value.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> SomeNotNull<T, TException>(this T value, TException exception) =>
            value.SomeWhen(val => val != null, exception);

        /// <summary>
        /// Creates an Option&lt;T&gt; instance from a specified value. 
        /// If the value is null, an empty optional is returned, 
        /// with a specified exceptional value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value.</param>
        /// <returns>An optional containing the specified value.</returns>
        public static Option<T, TException> SomeNotNull<T, TException>(this T value, Func<TException> exceptionFactory)
        {
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return value.SomeWhen(val => val != null, exceptionFactory);
        }

        /// <summary>
        /// Converts a Nullable&lt;T&gt; to an Option&lt;T&gt; instance.
        /// </summary>
        /// <param name="value">The Nullable&lt;T&gt; instance.</param>
        /// <returns>The Option&lt;T&gt; instance.</returns>
        public static Option<T> ToOption<T>(this T? value) where T : struct =>
            value.HasValue ? Option.Some(value.Value) : Option.None<T>();

        /// <summary>
        /// Converts a Nullable&lt;T&gt; to an Option&lt;T, TException&gt; instance, 
        /// with a specified exceptional value.
        /// </summary>
        /// <param name="value">The Nullable&lt;T&gt; instance.</param>
        /// <param name="exception">The exceptional value.</param>
        /// <returns>The Option&lt;T, TException&gt; instance.</returns>
        public static Option<T, TException> ToOption<T, TException>(this T? value, TException exception) where T : struct =>
            value.HasValue ? Option.Some<T, TException>(value.Value) : Option.None<T, TException>(exception);

        /// <summary>
        /// Converts a Nullable&lt;T&gt; to an Option&lt;T, TException&gt; instance, 
        /// with a specified exceptional value.
        /// </summary>
        /// <param name="value">The Nullable&lt;T&gt; instance.</param>
        /// <param name="exceptionFactory">A factory function to create an exceptional value.</param>
        /// <returns>The Option&lt;T, TException&gt; instance.</returns>
        public static Option<T, TException> ToOption<T, TException>(this T? value, Func<TException> exceptionFactory) where T : struct
        {
            if (exceptionFactory == null) throw new ArgumentNullException(nameof(exceptionFactory));
            return value.HasValue ? Option.Some<T, TException>(value.Value) : Option.None<T, TException>(exceptionFactory());
        }

        /// <summary>
        /// Returns the existing value if present, or the attached 
        /// exceptional value.
        /// </summary>
        /// <param name="option">The specified optional.</param>
        /// <returns>The existing or exceptional value.</returns>
        public static T ValueOrException<T>(this Option<T, T> option) => option.HasValue ? option.Value : option.Exception;

        /// <summary>
        /// Flattens two nested optionals into one. The resulting optional
        /// will be empty if either the inner or outer optional is empty.
        /// </summary>
        /// <param name="option">The nested optional.</param>
        /// <returns>A flattened optional.</returns>
        public static Option<T> Flatten<T>(this Option<Option<T>> option) =>
            option.FlatMap(innerOption => innerOption);

        /// <summary>
        /// Flattens two nested optionals into one. The resulting optional
        /// will be empty if either the inner or outer optional is empty.
        /// </summary>
        /// <param name="option">The nested optional.</param>
        /// <returns>A flattened optional.</returns>
        public static Option<T, TException> Flatten<T, TException>(this Option<Option<T, TException>, TException> option) =>
            option.FlatMap(innerOption => innerOption);

        // ============= MaTech additions =============
        
        public static T ValueOrNull<T>(in this Option<T> option) where T : class => option.HasValue ? option.Value : null;
        public static T? ToNullable<T>(in this Option<T> option) where T : struct => option.HasValue ? option.Value : null;

        public static bool TryGet<T>(in this Option<T> option, out T result) {
            if (option.HasValue) {
                result = option.Value;
                return true;
            }
            result = default;
            return false;
        }
        public static bool TryGet<T, E>(in this Option<T, E> option, out T result) {
            if (option.HasValue) {
                result = option.Value;
                return true;
            }
            result = default;
            return false;
        }

        public static bool TryAssign<T>(in this Option<T> option, ref T target) {
            if (option.HasValue) {
                target = option.Value;
                return true;
            }
            return false;
        }
        public static bool TryAssign<T, E>(in this Option<T, E> option, ref T target) {
            if (option.HasValue) {
                target = option.Value;
                return true;
            }
            return false;
        }

        public static Option<(T1, T2)> TupleSome<T1, T2>(in this Option<T1> option, in T2 value)
            => option.HasValue ? (option.Value, value).Some() : Option.None<(T1, T2)>();
        
        public static Option<TValue> InvokeSome<TKey, TValue>(in this Option<Func<TKey, TValue>> option, in TKey key)
            => option.HasValue ? option.Value.Invoke(key).Some() : Option.None<TValue>();
        public static Option<TValue> InvokeSome<TKey, TValue>(in this Option<FuncIn<TKey, TValue>> option, in TKey key)
            => option.HasValue ? option.Value.Invoke(key).Some() : Option.None<TValue>();
        
        public delegate TValue FuncIn<TKey, out TValue>(in TKey key);
    }
}
