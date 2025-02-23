// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using MaTech.Common.Data;
using MaTech.Common.Utils;

namespace MaTech.Gameplay.Time {
    public readonly struct BeatUnit : IComparable<BeatUnit> {
        public readonly Fraction fraction;
        public readonly float decimals;

        public double Value => fraction.Integer + decimals;

        private BeatUnit(int value) {
            fraction = new Fraction(value);
            decimals = 0;
        }

        private BeatUnit(in Fraction value) {
            fraction = value.Simplified;
            decimals = fraction.DecimalFloat;
        }

        private BeatUnit(double value) {
            fraction = Fraction.FromFloat(value);
            decimals = (float)(value - fraction.Integer);
        }

        private BeatUnit(double value, int denominator) {
            fraction = Fraction.FromFloatRounded(value, denominator);
            decimals = (float)(value - fraction.Integer);
        }
        
        private BeatUnit(in Fraction value, float knownDecimals) {
            fraction = value.Simplified;
            decimals = knownDecimals;
        }

        private BeatUnit(in BeatUnit other, in BeatUnit offset) {
            fraction = other.fraction + offset.fraction;
            decimals = other.decimals + offset.decimals + (other.fraction.Integer + offset.fraction.Integer - fraction.Integer);
        }
        
        public static BeatUnit FromCount(int count) => new(count);
        public static BeatUnit FromFraction(Fraction fraction) => new(fraction);
        public static BeatUnit FromFraction(int numerator, int denominator) => new(new FractionSimple(numerator, denominator));
        public static BeatUnit FromFraction(int integer, int numerator, int denominator) => new(new Fraction(integer, numerator, denominator));
        public static BeatUnit FromFractionReduced(int numerator, int denominator) => new(new FractionSimple(numerator, denominator).Reduced);
        public static BeatUnit FromFractionReduced(int integer, int numerator, int denominator) => new(new Fraction(integer, numerator, denominator).Reduced);
        public static BeatUnit FromValue(double value) => new(value);
        public static BeatUnit FromValueRounded(double value, int denominator) => new(value, denominator);

        public BeatUnit OffsetBy(in BeatUnit offset) => new(this, offset);
        public BeatUnit Negate() => new(Fraction.zero - fraction, 1 - decimals);

        public static bool InRange(in BeatUnit value, in BeatUnit start, in BeatUnit end, bool excludeStart = false, bool excludeEnd = false, bool compareWithDecimals = false) {
            int compareToStart = value.CompareTo(start, compareWithDecimals);
            int compareToEnd = value.CompareTo(end, compareWithDecimals);
            bool afterStart = excludeStart ? compareToStart > 0 : compareToStart >= 0;
            bool beforeEnd = excludeEnd ? compareToEnd < 0 : compareToEnd <= 0;
            return afterStart && beforeEnd;
        }

        public int CompareTo(BeatUnit other) => CompareTo(other, true);
        public int CompareTo(in BeatUnit other, bool compareWithDecimals) {
            if (CompareUtil.TryCompareTo(fraction, other.fraction, out var result)) return result;
            return compareWithDecimals ? decimals.CompareTo(other.decimals) : 0;
        }

        public static implicit operator int(in BeatUnit obj) => obj.fraction.Integer;
        public static implicit operator double(in BeatUnit obj) => obj.Value;
        public static implicit operator Fraction(in BeatUnit obj) => obj.fraction;
        public static implicit operator BeatUnit(int value) => new(value);
        public static implicit operator BeatUnit(double value) => new(value);
        public static implicit operator BeatUnit(Fraction value) => new(value);

        public override string ToString() {
            float delta = decimals - fraction.DecimalFloat;
            return delta.Near(0) ? $"{fraction}" : $"{fraction} ({(delta > 0 ? "+" : "-")}{delta:f3})";
        }
    }

    public readonly struct TimeUnit : IComparable<TimeUnit> {
        private readonly int integer;
        private readonly float decimals;

        public double Milliseconds => integer + decimals;
        public double Seconds => Milliseconds * 0.001;

        public int MillisecondsRounded => integer;
        public int MillisecondsFloored => integer + (decimals < 0 ? -1 : 0);
        public int MillisecondsCeiling => integer + (decimals > 0 ? 1 : 0);
        
        private TimeUnit(double ms) {
            integer = MathUtil.RoundToInt(ms);
            decimals = (float)(ms - integer);
        }

        private TimeUnit(int ms, float d = 0) {
            integer = ms;
            decimals = d;
        }

        private TimeUnit(in TimeUnit other, in TimeUnit offset) {
            var decimalsWithOffset = other.decimals + offset.decimals;
            var decimalsWithOffsetInInteger = MathUtil.RoundToInt(decimalsWithOffset);
            integer = decimalsWithOffsetInInteger + other.integer + offset.integer;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
        }

        public static TimeUnit Zero => new(0);
        public static TimeUnit MaxValue => new(int.MaxValue);
        public static TimeUnit MinValue => new(int.MinValue);
        
        public static TimeUnit FromMilliseconds(double ms) => new(ms);
        public static TimeUnit FromSeconds(double seconds) => new(seconds * 1000);
        public static TimeUnit FromTimeSpan(TimeSpan timeSpan) => new(timeSpan.TotalMilliseconds);

        public TimeUnit Negate() => new(-integer, -decimals);
        public TimeUnit ScaleBy(float rate) => FromMilliseconds((double)integer * rate).OffsetBy(FromMilliseconds((double)decimals * rate));
        public TimeUnit OffsetBy(in TimeUnit offset) => new(this, offset);
        
        public TimeUnit DeltaSince(in TimeUnit timeEarlier) => new(this, timeEarlier.Negate());
        
        public static bool InRange(in TimeUnit value, in TimeUnit start, in TimeUnit end, bool excludeStart = false, bool excludeEnd = false, bool roundToMS = false) {
            int compareToStart = value.CompareTo(start, roundToMS);
            int compareToEnd = value.CompareTo(end, roundToMS);
            bool afterStart = excludeStart ? compareToStart > 0 : compareToStart >= 0;
            bool beforeEnd = excludeEnd ? compareToEnd < 0 : compareToEnd <= 0;
            return afterStart && beforeEnd;
        }

        public int CompareTo(TimeUnit other) => CompareTo(other, false);
        public int CompareTo(in TimeUnit other, bool roundToMS) {
            if (CompareUtil.TryCompareTo(integer, other.integer, out var result)) return result;
            return roundToMS ? 0 : decimals.CompareTo(other.decimals);
        }
        
        public override string ToString() => $"{integer}ms";
    }
}