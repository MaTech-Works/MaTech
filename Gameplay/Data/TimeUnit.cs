// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Data;
using MaTech.Common.Utils;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Data {
    public interface ITimeUnit<T> where T : struct, ITimeUnit<T> {
        public double Value { get; }
        public int CompareTo(in T other, bool aligned = false);

        public T Negate();
        public T ScaleBy(double scale);
        public T OffsetBy(in T offset);
        public T DeltaSince(in T smaller);
        public double RatioTo(in T divisor);
    }
    
    // todo: rename to BeatValue
    // todo: implement Rational struct and replace with it here
    // todo: after Rational, make this serializable or implement IMeta
    // ReSharper disable once StructCanBeMadeReadOnly
    public struct BeatUnit : ITimeUnit<BeatUnit>, IComparable<BeatUnit>, IEquatable<BeatUnit> {
        public readonly FractionMixed fraction;
        public readonly float decimals; // todo: not caching the decimals when fraction is used, so no precision loss is accumulated throughout calculation
        
        public readonly FractionMixed Fraction => IsMax ? FractionMixed.maxValue : IsMin ? FractionMixed.minValue : fraction;
        public readonly double Value => IsMax ? double.PositiveInfinity : IsMin ? double.NegativeInfinity : fraction.Integer + decimals;

        public readonly bool IsMax => fraction >= maxFraction;
        public readonly bool IsMin => fraction <= minFraction;
        
        private BeatUnit(int value) {
            fraction = MathUtil.Clamp(value, -MaxInteger, MaxInteger);
            decimals = 0;
        }
        private BeatUnit(double value, int? denominator = null) {
            value = MathUtil.Clamp(value, -MaxInteger, MaxInteger);
            fraction = denominator is null ? FractionMixed.FromFloat(value) : FractionMixed.FromFloatRounded(value, denominator.Value);
            decimals = (float)(value - fraction.Integer);
        }
        private BeatUnit(in FractionMixed value, double? knownValue = null) {
            fraction = MathUtil.Clamp(value, minFraction, maxFraction);
            decimals = (float?)(knownValue - fraction.Integer) ?? fraction.DecimalFloat;
        }
        private BeatUnit(in BeatUnit other, in BeatUnit offset) {
            fraction = other.fraction + offset.fraction; // todo: handle denominator overflow and reduce precision
            decimals = other.decimals + offset.decimals + (other.fraction.Integer + offset.fraction.Integer - fraction.Integer);
            this = this.ClampBetween(MinValue, MaxValue);
        }

        // todo: arithmetic operators
        public readonly BeatUnit Negate() => new(fraction.Negated, -Value);
        public readonly BeatUnit ScaleBy(double scale) => new(Value * scale);
        public readonly BeatUnit OffsetBy(in BeatUnit offset) => new(this, offset);
        public readonly BeatUnit DeltaSince(in BeatUnit smaller) => new(this, smaller.Negate());
        public readonly double RatioTo(in BeatUnit divisor) => Value / divisor.Value;
        
        public readonly BeatUnit ScaleByFraction(in FractionMixed scale) => new(Fraction * scale);

        public static implicit operator double(in BeatUnit obj) => obj.Value;
        public static implicit operator FractionMixed(in BeatUnit obj) => obj.fraction;
        public static implicit operator BeatUnit(int value) => new(value);
        public static implicit operator BeatUnit(double value) => new(value);
        public static implicit operator BeatUnit(FractionMixed value) => new(value);
        public static implicit operator BeatUnit(FractionSimple value) => new(value);
        public static implicit operator BeatUnit((int a, int b) t) => FromFraction(t.a, t.b);
        public static implicit operator BeatUnit((int n, int a, int b) t) => FromFraction(t.n, t.a, t.b);
        
        public static BeatUnit Zero => new(0);
        public static BeatUnit MaxValue => maxFraction;
        public static BeatUnit MinValue => minFraction;
        
        public static BeatUnit FromCount(int count) => new(count);
        public static BeatUnit FromFraction(int a, int b) => new(FractionSimple.Simple(a, b));
        public static BeatUnit FromFraction(int n, int a, int b) => new(FractionMixed.Simple(n, a, b));
        public static BeatUnit FromValue(double value) => new(value);
        public static BeatUnit FromValueRounded(double value, int denominator) => new(value, denominator);

        public static bool operator==(in BeatUnit a, in BeatUnit b) => a.fraction == b.fraction;
        public static bool operator!=(in BeatUnit a, in BeatUnit b) => a.fraction != b.fraction;

        public readonly int CompareTo(BeatUnit other) => CompareTo(other, false);
        public readonly int CompareTo(in BeatUnit other, bool alignToFraction) {
            if (CompareUtil.TryCompareTo(fraction, other.fraction, out var result)) return result;
            return alignToFraction ? 0 : decimals.CompareTo(other.decimals);
        }
        
        public bool Equals(BeatUnit other) => CompareTo(other, true) == 0;
        public override bool Equals(object? obj) => obj is BeatUnit other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(fraction, decimals);

        public override readonly string ToString() {
            if (IsMax) return "Max"; if (IsMin) return "Min";
            float delta = decimals - fraction.DecimalFloat;
            return delta.Near(0) ? $"{fraction}" : $"{fraction} ({(delta > 0 ? "+" : "-")}{delta:f3})";
        }
        
        private const int MaxInteger = 1000000000;
        private static readonly FractionMixed maxFraction = new(MaxInteger);
        private static readonly FractionMixed minFraction = new(-MaxInteger);
    }

    // todo: rename to TimeValue
    // todo: implement Rational struct and replace with it here
    // todo: after Rational, make this serializable or implement IMeta
    // ReSharper disable once StructCanBeMadeReadOnly
    public struct TimeUnit : ITimeUnit<TimeUnit>, IComparable<TimeUnit> {
        public readonly int integer;
        public readonly float decimals;

        public readonly double Value => IsMax ? double.PositiveInfinity : IsMin ? double.NegativeInfinity : Seconds;

        public readonly double Milliseconds => integer + decimals;
        public readonly double Seconds => Milliseconds * 0.001;

        public readonly int MillisecondsRounded => integer;
        public readonly int MillisecondsFloored => integer + (decimals < 0 ? -1 : 0);
        public readonly int MillisecondsCeiling => integer + (decimals > 0 ? 1 : 0);
        
        public readonly bool IsMax => integer >= MaxInteger;
        public readonly bool IsMin => integer <= -MaxInteger;
        
        private TimeUnit(double ms) {
            ms = MathUtil.Clamp(ms, -MaxInteger, MaxInteger);
            integer = MathUtil.RoundToInt(ms);
            decimals = (float)(ms - integer);
        }
        private TimeUnit(int ms, float d = 0) {
            integer = MathUtil.Clamp(ms, -MaxInteger, MaxInteger);
            decimals = d;
        }
        private TimeUnit(in TimeUnit other, in TimeUnit offset) {
            var decimalsWithOffset = other.decimals + offset.decimals;
            var decimalsWithOffsetInInteger = MathUtil.RoundToInt(decimalsWithOffset);
            integer = decimalsWithOffsetInInteger + other.integer + offset.integer;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
            this = this.ClampBetween(MinValue, MaxValue);
        }

        // todo: arithmetic operators (notice that TimeUnit/TimeUnit results in casting to int)
        public readonly TimeUnit Negate() => new(-integer, -decimals);
        public readonly TimeUnit ScaleBy(double rate) => FromMilliseconds(integer * rate).OffsetBy(FromMilliseconds(decimals * rate));
        public readonly TimeUnit OffsetBy(in TimeUnit offset) => new(this, offset);
        public readonly TimeUnit DeltaSince(in TimeUnit smaller) => new(this, smaller.Negate());
        public readonly double RatioTo(in TimeUnit divisor) => Milliseconds / divisor.Milliseconds;
        
        public readonly TimeUnit OffsetByMilliseconds(int ms) => new(integer + ms, decimals);
        
        public static implicit operator int(in TimeUnit obj) => obj.MillisecondsRounded;
        public static implicit operator double(in TimeUnit obj) => obj.Seconds;
        public static implicit operator TimeUnit(int value) => FromMilliseconds(value);
        public static implicit operator TimeUnit(double value) => FromSeconds(value);

        public static TimeUnit Zero => new(0);
        public static TimeUnit MaxValue => new(MaxInteger);
        public static TimeUnit MinValue => new(-MaxInteger);
        
        public static TimeUnit FromMilliseconds(double ms) => new(ms);
        public static TimeUnit FromSeconds(double seconds) => new(seconds * 1000);
        public static TimeUnit FromTimeSpan(TimeSpan timeSpan) => new(timeSpan.TotalMilliseconds);
        
        public static bool operator==(in TimeUnit a, in TimeUnit b) => a.integer == b.integer;
        public static bool operator!=(in TimeUnit a, in TimeUnit b) => a.integer != b.integer;

        public readonly int CompareTo(TimeUnit other) => CompareTo(other, false);
        public readonly int CompareTo(in TimeUnit other, bool alignToMilliseconds) {
            if (CompareUtil.TryCompareTo(integer, other.integer, out var result)) return result;
            return alignToMilliseconds ? 0 : decimals.CompareTo(other.decimals);
        }
        
        public bool Equals(TimeUnit other) => CompareTo(other, true) == 0;
        public override bool Equals(object? obj) => obj is TimeUnit other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(integer, decimals);
        
        public override readonly string ToString() {
            if (IsMax) return "Max";
            if (IsMin) return "Min";
            return $"{integer}ms";
        }

        private const int MaxInteger = 1000000000;
    }

    // todo: make this serializable or implement IMeta
    // ReSharper disable once StructCanBeMadeReadOnly
    public struct Range<T> where T : struct, ITimeUnit<T> {
        public readonly T start;
        public readonly T end;

        public Range(in T start, in T end) {
            this.start = start;
            this.end = end;
        }
        
        public void Deconstruct(out T start, out T end) => (start, end) = (this.start, this.end);
        
        public static implicit operator (T start, T end)(in Range<T> range) => (range.start, range.end);
        public static implicit operator Range<T>(in (T start, T end) tuple) => new(tuple.start, tuple.end);
    }

    public static class TimeUnitComparers<T> where T : struct, ITimeUnit<T> {
        private class Comparer : IComparer<T> { public bool aligned; public int Compare(T x, T y) => x.CompareTo(y, aligned); }
        public static readonly IComparer<T> precise = new Comparer() { aligned = true };
        public static readonly IComparer<T> aligned = new Comparer() { aligned = false };
    }
    
    public static class TimeUnitExtensions {
        // todo: Min and Max
        
        // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
        private static T Delta<T>(in T a, in T b) where T : struct, ITimeUnit<T> => b.DeltaSince(a);
        private static T Clamp<T>(in T a, in T b, T value) where T : struct, ITimeUnit<T> => (value.CompareTo(a), value.CompareTo(b)) switch { (<0, <0) => a, (>0, >0) => b, _ => value };
        private static T Lerp<T>(in T a, in T b, double k) where T : struct, ITimeUnit<T> => a.ScaleBy(1 - k).OffsetBy(b.ScaleBy(k));
        private static double Ratio<T>(in T a, in T b, in T value) where T : struct, ITimeUnit<T> => value.DeltaSince(a).RatioTo(b.DeltaSince(a));
        // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable
        
        private static bool InRange<T>(this T value, in T start, in T end, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeUnit<T> {
            int compareToStart = value.CompareTo(start, aligned);
            int compareToEnd = value.CompareTo(end, aligned);
            bool afterStart = excludeStart ? compareToStart > 0 : compareToStart >= 0;
            bool beforeEnd = excludeEnd ? compareToEnd < 0 : compareToEnd <= 0;
            return afterStart && beforeEnd;
        }
        
        public static T ClampBetween<T>(this T self, in T a, in T b) where T : struct, ITimeUnit<T> => Clamp(a, b, self);
        public static double RatioBetween<T>(this T self, in T a, in T b) where T : struct, ITimeUnit<T> => Ratio(a, b, Clamp(a, b, self));
        public static double RatioAcross<T>(this T self, in T a, in T b) where T : struct, ITimeUnit<T> => Ratio(a, b, self);
        public static bool InRange<T>(this T self, in Range<T> range, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeUnit<T>
            => self.InRange(range.start, range.end, excludeStart, excludeEnd, aligned);

        public static T Length<T>(in this Range<T> range) where T : struct, ITimeUnit<T> => Delta(range.start, range.end);
        public static T Lerp<T>(in this Range<T> range, double k) where T : struct, ITimeUnit<T> => Lerp(range.start, range.end, k);
        public static T Clamp<T>(in this Range<T> range, T value) where T : struct, ITimeUnit<T> => Clamp(range.start, range.end, value);
        public static Range<T> Clamp<T>(in this Range<T> range, Range<T> value) where T : struct, ITimeUnit<T>
            => new(Clamp(range.start, range.end, value.start), Clamp(range.start, range.end, value.end));
        public static double RatioOf<T>(in this Range<T> range, T value, bool clamped = true) where T : struct, ITimeUnit<T>
            => SaturateOrPassthrough(Ratio(range.start, range.end, value), clamped);
        public static bool Contains<T>(in this Range<T> range, T value, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeUnit<T>
            => value.InRange(range.start, range.end, excludeStart, excludeEnd, aligned);
        
        public static T Length<T>(in this (T start, T end) range) where T : struct, ITimeUnit<T> => Delta(range.start, range.end);
        public static T Lerp<T>(in this (T start, T end) range, double k) where T : struct, ITimeUnit<T> => Lerp(range.start, range.end, k);
        public static T Clamp<T>(in this (T start, T end) range, T value) where T : struct, ITimeUnit<T> => Clamp(range.start, range.end, value);
        public static Range<T> Clamp<T>(in this (T start, T end) range, Range<T> value) where T : struct, ITimeUnit<T>
            => new(Clamp(range.start, range.end, value.start), Clamp(range.start, range.end, value.end));
        public static double RatioOf<T>(in this (T start, T end) range, T value, bool clamped = true) where T : struct, ITimeUnit<T>
            => SaturateOrPassthrough(Ratio(range.start, range.end, value), clamped);
        public static bool Contains<T>(in this (T start, T end) range, T value, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeUnit<T>
            => value.InRange(range.start, range.end, excludeStart, excludeEnd, aligned);
        
        private static double SaturateOrPassthrough(double value, bool saturate) => saturate ? MathUtil.Saturate(value) : value;
    }
}