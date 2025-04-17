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
    public interface ITimeValue<T> : IComparable<T>, IEquatable<T> where T : struct, ITimeValue<T> {
        public double Value { get; }
        public int CompareTo(in T other, bool aligned = false);

        public T Negate();
        public T ScaleBy(double scale);
        public T OffsetBy(in T offset);
        public T DeltaSince(in T smaller);
        public double RatioTo(in T divisor);
    }
    
    // todo: implement Rational struct and replace with it here
    // todo: after Rational, make this serializable or implement IMeta
    // ReSharper disable once StructCanBeMadeReadOnly
    public struct BeatValue : ITimeValue<BeatValue> {
        private readonly FractionMixed fraction;
        private readonly float decimals; // todo: not caching the decimals when fraction is used, so no precision loss is accumulated throughout calculation
        
        public readonly FractionMixed Fraction => IsMax ? FractionMixed.maxValue : IsMin ? FractionMixed.minValue : fraction;
        public readonly double Value => IsMax ? double.PositiveInfinity : IsMin ? double.NegativeInfinity : fraction.Integer + decimals;

        public readonly bool IsMax => fraction >= maxFraction;
        public readonly bool IsMin => fraction <= minFraction;
        
        private BeatValue(int value) {
            fraction = MathUtil.Clamp(value, -MaxInteger, MaxInteger);
            decimals = 0;
        }
        private BeatValue(double value, int? denominator = null) {
            value = MathUtil.Clamp(value, -MaxInteger, MaxInteger);
            fraction = denominator is null ? FractionMixed.FromFloat(value) : FractionMixed.FromFloatRounded(value, denominator.Value);
            decimals = (float)(value - fraction.Integer);
        }
        private BeatValue(in FractionMixed value, double? knownValue = null) {
            fraction = MathUtil.Clamp(value, minFraction, maxFraction);
            decimals = (float?)(knownValue - fraction.Integer) ?? fraction.DecimalFloat;
        }
        private BeatValue(in BeatValue other, in BeatValue offset) {
            fraction = other.fraction + offset.fraction; // todo: handle denominator overflow and reduce precision
            decimals = other.decimals + offset.decimals + (other.fraction.Integer + offset.fraction.Integer - fraction.Integer);
            this = this.ClampBetween(MinValue, MaxValue);
        }

        public readonly BeatValue Negate() => new(fraction.Negated, -Value);
        public readonly BeatValue ScaleBy(double scale) => new(Value * scale);
        public readonly BeatValue OffsetBy(in BeatValue offset) => new(this, offset);
        public readonly BeatValue DeltaSince(in BeatValue smaller) => new(this, smaller.Negate());
        public readonly double RatioTo(in BeatValue divisor) => Value / divisor.Value;
        
        public readonly BeatValue ScaleByFraction(in FractionMixed scale) => new(Fraction * scale);

        public static implicit operator double(in BeatValue obj) => obj.Value;
        public static implicit operator FractionMixed(in BeatValue obj) => obj.fraction;
        public static implicit operator BeatValue(int value) => new(value);
        public static implicit operator BeatValue(double value) => new(value);
        public static implicit operator BeatValue(FractionMixed value) => new(value);
        public static implicit operator BeatValue(FractionSimple value) => new(value);
        public static implicit operator BeatValue((int a, int b) t) => FromFraction(t.a, t.b);
        public static implicit operator BeatValue((int n, int a, int b) t) => FromFraction(t.n, t.a, t.b);
        
        public static BeatValue Zero => new(0);
        public static BeatValue MaxValue => maxFraction;
        public static BeatValue MinValue => minFraction;
        
        public static BeatValue FromCount(int count) => new(count);
        public static BeatValue FromFraction(int a, int b) => new(FractionSimple.Simple(a, b));
        public static BeatValue FromFraction(int n, int a, int b) => new(FractionMixed.Simple(n, a, b));
        public static BeatValue FromValue(double value) => new(value);
        public static BeatValue FromValueRounded(double value, int denominator) => new(value, denominator);

        public static bool operator==(in BeatValue a, in BeatValue b) => a.fraction == b.fraction;
        public static bool operator!=(in BeatValue a, in BeatValue b) => a.fraction != b.fraction;
        
        public static BeatValue operator+(in BeatValue a, in BeatValue b) => a.OffsetBy(b);
        public static BeatValue operator-(in BeatValue a, in BeatValue b) => a.DeltaSince(b);
        public static BeatValue operator*(in BeatValue a, double b) => a.ScaleBy(b);
        public static BeatValue operator*(double b, in BeatValue a) => a.ScaleBy(b);
        public static BeatValue operator/(in BeatValue a, in BeatValue b) => a.RatioTo(b);

        public readonly int CompareTo(BeatValue other) => CompareTo(other, false);
        public readonly int CompareTo(in BeatValue other, bool alignToFraction) {
            if (CompareUtil.TryCompareTo(fraction, other.fraction, out var result)) return result;
            return alignToFraction ? 0 : decimals.CompareTo(other.decimals);
        }
        
        public bool Equals(BeatValue other) => CompareTo(other, true) == 0;
        public override bool Equals(object? obj) => obj is BeatValue other && Equals(other);
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

    // todo: implement Rational struct and replace with it here
    // todo: after Rational, make this serializable or implement IMeta
    // ReSharper disable once StructCanBeMadeReadOnly
    public struct TimeValue : ITimeValue<TimeValue> {
        private readonly int integer;
        private readonly float decimals;

        public readonly double Value => IsMax ? double.PositiveInfinity : IsMin ? double.NegativeInfinity : Seconds;

        public readonly double Milliseconds => integer + decimals;
        public readonly double Seconds => Milliseconds * 0.001;

        public readonly int MillisecondsRounded => integer;
        public readonly int MillisecondsFloored => integer + (decimals < 0 ? -1 : 0);
        public readonly int MillisecondsCeiling => integer + (decimals > 0 ? 1 : 0);
        
        public readonly bool IsMax => integer >= MaxInteger;
        public readonly bool IsMin => integer <= -MaxInteger;
        
        private TimeValue(double ms) {
            ms = MathUtil.Clamp(ms, -MaxInteger, MaxInteger);
            integer = MathUtil.RoundToInt(ms);
            decimals = (float)(ms - integer);
        }
        private TimeValue(int ms, float d = 0) {
            integer = MathUtil.Clamp(ms, -MaxInteger, MaxInteger);
            decimals = d;
        }
        private TimeValue(in TimeValue other, in TimeValue offset) {
            var decimalsWithOffset = other.decimals + offset.decimals;
            var decimalsWithOffsetInInteger = MathUtil.RoundToInt(decimalsWithOffset);
            integer = decimalsWithOffsetInInteger + other.integer + offset.integer;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
            this = this.ClampBetween(MinValue, MaxValue);
        }

        public readonly TimeValue Negate() => new(-integer, -decimals);
        public readonly TimeValue ScaleBy(double rate) => FromMilliseconds(integer * rate).OffsetBy(FromMilliseconds(decimals * rate));
        public readonly TimeValue OffsetBy(in TimeValue offset) => new(this, offset);
        public readonly TimeValue DeltaSince(in TimeValue smaller) => new(this, smaller.Negate());
        public readonly double RatioTo(in TimeValue divisor) => Milliseconds / divisor.Milliseconds;
        
        public readonly TimeValue OffsetByMilliseconds(int ms) => new(integer + ms, decimals);
        
        public static implicit operator int(in TimeValue obj) => obj.MillisecondsRounded;
        public static implicit operator double(in TimeValue obj) => obj.Seconds;
        public static implicit operator TimeValue(int value) => FromMilliseconds(value);
        public static implicit operator TimeValue(double value) => FromSeconds(value);

        public static TimeValue Zero => new(0);
        public static TimeValue MaxValue => new(MaxInteger);
        public static TimeValue MinValue => new(-MaxInteger);
        
        public static TimeValue FromMilliseconds(double ms) => new(ms);
        public static TimeValue FromSeconds(double seconds) => new(seconds * 1000);
        public static TimeValue FromTimeSpan(TimeSpan timeSpan) => new(timeSpan.TotalMilliseconds);
        
        public static bool operator==(in TimeValue a, in TimeValue b) => a.integer == b.integer;
        public static bool operator!=(in TimeValue a, in TimeValue b) => a.integer != b.integer;
        
        public static TimeValue operator+(in TimeValue a, in TimeValue b) => a.OffsetBy(b);
        public static TimeValue operator-(in TimeValue a, in TimeValue b) => a.DeltaSince(b);
        public static TimeValue operator*(in TimeValue a, double b) => a.ScaleBy(b);
        public static TimeValue operator*(double b, in TimeValue a) => a.ScaleBy(b);
        public static TimeValue operator/(in TimeValue a, in TimeValue b) => a.RatioTo(b);

        public readonly int CompareTo(TimeValue other) => CompareTo(other, false);
        public readonly int CompareTo(in TimeValue other, bool alignToMilliseconds) {
            if (CompareUtil.TryCompareTo(integer, other.integer, out var result)) return result;
            return alignToMilliseconds ? 0 : decimals.CompareTo(other.decimals);
        }
        
        public bool Equals(TimeValue other) => CompareTo(other, true) == 0;
        public override bool Equals(object? obj) => obj is TimeValue other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(integer, decimals);
        
        public override readonly string ToString() {
            if (IsMax) return "Max";
            if (IsMin) return "Min";
            return $"{integer}ms";
        }

        private const int MaxInteger = 1000000000;
    }
    
    // todo: after Rational, make this serializable or implement IMeta
    // ReSharper disable once StructCanBeMadeReadOnly
    public struct RollValue : ITimeValue<RollValue> {
        private readonly int integer;
        private readonly float decimals;

        public readonly double Value => IsMax ? double.PositiveInfinity : IsMin ? double.NegativeInfinity : integer + decimals;
        
        public readonly bool IsMax => integer >= MaxInteger;
        public readonly bool IsMin => integer <= -MaxInteger;
        
        private RollValue(double value) {
            value = MathUtil.Clamp(value, -MaxInteger, MaxInteger);
            integer = MathUtil.RoundToInt(value);
            decimals = (float)(value - integer);
        }
        private RollValue(int i, float d = 0) {
            integer = MathUtil.Clamp(i, -MaxInteger, MaxInteger);
            decimals = d;
        }
        private RollValue(in RollValue other, in RollValue offset) {
            var decimalsWithOffset = other.decimals + offset.decimals;
            var decimalsWithOffsetInInteger = MathUtil.RoundToInt(decimalsWithOffset);
            integer = decimalsWithOffsetInInteger + other.integer + offset.integer;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
            this = this.ClampBetween(MinValue, MaxValue);
        }

        public readonly RollValue Negate() => new(-integer, -decimals);
        public readonly RollValue ScaleBy(double rate) => FromValue(integer * rate).OffsetBy(FromValue(decimals * rate));
        public readonly RollValue OffsetBy(in RollValue offset) => new(this, offset);
        public readonly RollValue DeltaSince(in RollValue smaller) => new(this, smaller.Negate());
        public readonly double RatioTo(in RollValue divisor) => Value / divisor.Value;
        
        public static implicit operator double(in RollValue obj) => obj.Value;
        public static implicit operator RollValue(double value) => FromValue(value);

        public static RollValue Zero => new(0);
        public static RollValue MaxValue => new(MaxInteger);
        public static RollValue MinValue => new(-MaxInteger);
        
        public static RollValue FromValue(double value) => new(value);
        
        public static bool operator==(in RollValue a, in RollValue b) => a.integer == b.integer;
        public static bool operator!=(in RollValue a, in RollValue b) => a.integer != b.integer;
        
        public static RollValue operator+(in RollValue a, in RollValue b) => a.OffsetBy(b);
        public static RollValue operator-(in RollValue a, in RollValue b) => a.DeltaSince(b);
        public static RollValue operator*(in RollValue a, double b) => a.ScaleBy(b);
        public static RollValue operator*(double b, in RollValue a) => a.ScaleBy(b);
        public static RollValue operator/(in RollValue a, in RollValue b) => a.RatioTo(b);

        public readonly int CompareTo(RollValue other) => CompareTo(other, false);
        public readonly int CompareTo(in RollValue other, bool alignToMilliseconds) {
            if (CompareUtil.TryCompareTo(integer, other.integer, out var result)) return result;
            return alignToMilliseconds ? 0 : decimals.CompareTo(other.decimals);
        }
        
        public bool Equals(RollValue other) => CompareTo(other, true) == 0;
        public override bool Equals(object? obj) => obj is RollValue other && Equals(other);
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
    public struct Range<T> where T : struct, ITimeValue<T> {
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

    public static class TimeValueComparers<T> where T : struct, ITimeValue<T> {
        private class Comparer : IComparer<T> { public bool aligned; public int Compare(T x, T y) => x.CompareTo(y, aligned); }
        public static readonly IComparer<T> precise = new Comparer() { aligned = true };
        public static readonly IComparer<T> aligned = new Comparer() { aligned = false };
    }
    
    public static class TimeValueExtensions {
        // todo: Min and Max
        
        // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
        private static T Delta<T>(in T a, in T b) where T : struct, ITimeValue<T> => b.DeltaSince(a);
        private static T Clamp<T>(in T a, in T b, T value) where T : struct, ITimeValue<T> => (value.CompareTo(a), value.CompareTo(b)) switch { (<0, <0) => a, (>0, >0) => b, _ => value };
        private static T Lerp<T>(in T a, in T b, double k) where T : struct, ITimeValue<T> => a.ScaleBy(1 - k).OffsetBy(b.ScaleBy(k));
        private static double Ratio<T>(in T a, in T b, in T value) where T : struct, ITimeValue<T> => value.DeltaSince(a).RatioTo(b.DeltaSince(a));
        // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable
        
        private static bool InRange<T>(this T value, in T start, in T end, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeValue<T> {
            int compareToStart = value.CompareTo(start, aligned);
            int compareToEnd = value.CompareTo(end, aligned);
            bool afterStart = excludeStart ? compareToStart > 0 : compareToStart >= 0;
            bool beforeEnd = excludeEnd ? compareToEnd < 0 : compareToEnd <= 0;
            return afterStart && beforeEnd;
        }
        
        public static T ClampBetween<T>(this T self, in T a, in T b) where T : struct, ITimeValue<T> => Clamp(a, b, self);
        public static double RatioBetween<T>(this T self, in T a, in T b) where T : struct, ITimeValue<T> => Ratio(a, b, Clamp(a, b, self));
        public static double RatioAcross<T>(this T self, in T a, in T b) where T : struct, ITimeValue<T> => Ratio(a, b, self);
        public static bool InRange<T>(this T self, in Range<T> range, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeValue<T>
            => self.InRange(range.start, range.end, excludeStart, excludeEnd, aligned);

        public static T Length<T>(in this Range<T> range) where T : struct, ITimeValue<T> => Delta(range.start, range.end);
        public static T Lerp<T>(in this Range<T> range, double k) where T : struct, ITimeValue<T> => Lerp(range.start, range.end, k);
        public static T Clamp<T>(in this Range<T> range, T value) where T : struct, ITimeValue<T> => Clamp(range.start, range.end, value);
        public static Range<T> Clamp<T>(in this Range<T> range, Range<T> value) where T : struct, ITimeValue<T>
            => new(Clamp(range.start, range.end, value.start), Clamp(range.start, range.end, value.end));
        public static double RatioOf<T>(in this Range<T> range, T value, bool clamped = true) where T : struct, ITimeValue<T>
            => SaturateOrPassthrough(Ratio(range.start, range.end, value), clamped);
        public static bool Contains<T>(in this Range<T> range, T value, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeValue<T>
            => value.InRange(range.start, range.end, excludeStart, excludeEnd, aligned);
        
        public static T Length<T>(in this (T start, T end) range) where T : struct, ITimeValue<T> => Delta(range.start, range.end);
        public static T Lerp<T>(in this (T start, T end) range, double k) where T : struct, ITimeValue<T> => Lerp(range.start, range.end, k);
        public static T Clamp<T>(in this (T start, T end) range, T value) where T : struct, ITimeValue<T> => Clamp(range.start, range.end, value);
        public static Range<T> Clamp<T>(in this (T start, T end) range, Range<T> value) where T : struct, ITimeValue<T>
            => new(Clamp(range.start, range.end, value.start), Clamp(range.start, range.end, value.end));
        public static double RatioOf<T>(in this (T start, T end) range, T value, bool clamped = true) where T : struct, ITimeValue<T>
            => SaturateOrPassthrough(Ratio(range.start, range.end, value), clamped);
        public static bool Contains<T>(in this (T start, T end) range, T value, bool excludeStart = false, bool excludeEnd = false, bool aligned = true) where T : struct, ITimeValue<T>
            => value.InRange(range.start, range.end, excludeStart, excludeEnd, aligned);
        
        private static double SaturateOrPassthrough(double value, bool saturate) => saturate ? MathUtil.Saturate(value) : value;
    }
}