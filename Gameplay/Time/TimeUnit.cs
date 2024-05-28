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

        // TODO: 简化private构造函数的定义种类，把变种计算移动到static的工厂方法里
        private BeatUnit(int value) {
            fraction = new Fraction(value);
            decimals = 0;
        }

        private BeatUnit(Fraction value) {
            fraction = value;
            decimals = value.DecimalFloat;
        }

        private BeatUnit(double value) {
            fraction = Fraction.FromFloat(value);
            decimals = (float)(value - fraction.Integer);
        }

        private BeatUnit(double value, int denominator) {
            fraction = Fraction.FromFloatRounded(value, denominator);
            decimals = (float)(value - fraction.Integer);
        }
        
        private BeatUnit(Fraction value, float knownDecimals) {
            fraction = value;
            decimals = knownDecimals;
        }

        private BeatUnit(in BeatUnit other, in BeatUnit offset) {
            fraction = other.fraction + offset.fraction;
            decimals = other.decimals + offset.decimals + (other.fraction.Integer + offset.fraction.Integer - fraction.Integer);
        }
        
        public static BeatUnit FromCount(int count) => new BeatUnit(count);
        public static BeatUnit FromFraction(Fraction fraction) => new BeatUnit(fraction);
        public static BeatUnit FromFraction(int numerator, int denominator) => new BeatUnit(new FractionSimple(numerator, denominator));
        public static BeatUnit FromFraction(int integer, int numerator, int denominator) => new BeatUnit(new Fraction(integer, numerator, denominator));
        public static BeatUnit FromValue(double value) => new BeatUnit(value);
        public static BeatUnit FromValueRounded(double value, int denominator) => new BeatUnit(value, denominator);

        // TODO: 改用operator实现运算
        public BeatUnit OffsetBy(in BeatUnit offset) => new BeatUnit(this, offset);
        public BeatUnit Negate() => new BeatUnit(Fraction.zero - fraction, 1 - decimals);

        public static bool IsInRangeByFraction(in BeatUnit beat, in BeatUnit rangeStart, in BeatUnit rangeEnd, bool includeStart = true, bool includeEnd = false) {
            bool afterStart = includeStart ? beat.fraction >= rangeStart.fraction : beat.fraction > rangeStart.fraction;
            bool beforeEnd = includeEnd ? beat.fraction <= rangeEnd.fraction : beat.fraction < rangeEnd.fraction;
            return afterStart && beforeEnd;
        }

        public static bool IsInRangeByValue(in BeatUnit beat, in BeatUnit rangeStart, in BeatUnit rangeEnd, bool includeStart = true, bool includeEnd = false) {
            double value = beat.Value, valueStart = rangeStart.Value, valueEnd = rangeEnd.Value;
            bool afterStart = includeStart ? value >= valueStart : value > valueStart;
            bool beforeEnd = includeEnd ? value <= valueEnd : value < valueEnd;
            return afterStart && beforeEnd;
        }

        public int CompareTo(BeatUnit other) { // cannot use in parameter for IComparable
            if (CompareUtil.TryCompareTo(fraction.Integer, other.fraction.Integer, out var result)) return result;
            return decimals.CompareTo(other.decimals);
        }

        public static implicit operator int(in BeatUnit obj) => obj.fraction.Integer;
        public static implicit operator double(in BeatUnit obj) => obj.Value;
        public static implicit operator Fraction(in BeatUnit obj) => obj.fraction;
        public static implicit operator BeatUnit(int value) => new BeatUnit(value);
        public static implicit operator BeatUnit(double value) => new BeatUnit(value);
        public static implicit operator BeatUnit(Fraction value) => new BeatUnit(value);

        public override string ToString() => $"{fraction} ({Value:f3})";
    }

    public readonly struct TimeUnit : IComparable<TimeUnit> {
        public readonly int milliseconds;
        public readonly float decimals;

        public double Milliseconds => milliseconds + decimals;
        public double Seconds => Milliseconds * 0.001;

        // TODO: 简化private构造函数的定义种类，把变种计算移动到static的工厂方法里
        private TimeUnit(int ms) {
            milliseconds = ms;
            decimals = 0;
        }

        private TimeUnit(double ms) {
            milliseconds = MathUtil.RoundToInt(ms);
            decimals = (float)(ms - milliseconds);
        }

        private TimeUnit(in TimeUnit other, int offsetMS) {
            milliseconds = other.milliseconds + offsetMS;
            decimals = other.decimals;
        }

        private TimeUnit(in TimeUnit other, float offsetMS) {
            var decimalsWithOffset = other.decimals + offsetMS;
            var decimalsWithOffsetInInteger = MathUtil.RoundToInt(decimalsWithOffset);
            milliseconds = decimalsWithOffsetInInteger + other.milliseconds;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
        }

        private TimeUnit(in TimeUnit other, in TimeUnit offset) {
            var decimalsWithOffset = other.decimals + offset.decimals;
            var decimalsWithOffsetInInteger = MathUtil.RoundToInt(decimalsWithOffset);
            milliseconds = decimalsWithOffsetInInteger + other.milliseconds + offset.milliseconds;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
        }

        public static TimeUnit Zero => new TimeUnit(0);
        public static TimeUnit MaxValue => new TimeUnit(int.MaxValue);
        public static TimeUnit MinValue => new TimeUnit(int.MinValue);
        
        public static TimeUnit FromMilliseconds(double ms) => new TimeUnit(ms);
        public static TimeUnit FromSeconds(double seconds) => new TimeUnit(seconds * 1000);
        public static TimeUnit FromMinutes(double minutes) => new TimeUnit(minutes * 60 * 1000);
        public static TimeUnit FromTimeSpan(TimeSpan timeSpan) => new TimeUnit(timeSpan.TotalMilliseconds);

        public TimeUnit OffsetBy(in TimeUnit offset) => new TimeUnit(this, offset);

        public TimeUnit Negate() => new TimeUnit(-milliseconds).OffsetBy(new TimeUnit(-decimals));

        public static bool IsInRangeByMilliseconds(in TimeUnit time, in TimeUnit rangeStart, in TimeUnit rangeEnd, bool includeStart = true, bool includeEnd = false) {
            bool afterStart = includeStart ? time.milliseconds >= rangeStart.milliseconds : time.milliseconds > rangeStart.milliseconds;
            bool beforeEnd = includeEnd ? time.milliseconds <= rangeEnd.milliseconds : time.milliseconds < rangeEnd.milliseconds;
            return afterStart && beforeEnd;
        }

        public static bool IsInRangeByValue(in TimeUnit time, in TimeUnit rangeStart, in TimeUnit rangeEnd, bool includeStart = true, bool includeEnd = false) {
            double value = time.Milliseconds, valueStart = rangeStart.Milliseconds, valueEnd = rangeEnd.Milliseconds;
            bool afterStart = includeStart ? value >= valueStart : value > valueStart;
            bool beforeEnd = includeEnd ? value <= valueEnd : value < valueEnd;
            return afterStart && beforeEnd;
        }

        public int CompareTo(TimeUnit other) { // cannot use in parameter for IComparable
            if (CompareUtil.TryCompareTo(milliseconds, other.milliseconds, out var result)) return result;
            return decimals.CompareTo(other.decimals);
        }

        public override string ToString() => $"{Milliseconds}ms";
    }
}