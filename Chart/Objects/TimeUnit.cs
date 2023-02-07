using System;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;

#nullable enable

namespace MaTech.Chart.Objects {
    public readonly struct BeatUnit : IComparable<BeatUnit> {
        public readonly Fraction fraction;
        public readonly float decimals;

        public double Value => fraction.Int + decimals;

        public BeatUnit(int value) {
            fraction = new Fraction(value);
            decimals = 0;
        }

        public BeatUnit(Fraction value) {
            fraction = value;
            decimals = value.DecimalFloat;
        }

        public BeatUnit(double value) {
            fraction = Fraction.FromFloat(value);
            decimals = (float)(value - fraction.Int);
        }

        public BeatUnit(double value, int denom) {
            fraction = Fraction.FromFloatRounded(value, denom);
            decimals = (float)(value - fraction.Int);
        }

        public BeatUnit(in BeatUnit other, int offset) {
            fraction = other.fraction + offset;
            decimals = other.decimals;
        }

        public BeatUnit(in BeatUnit other, Fraction offset) {
            fraction = other.fraction + offset;
            decimals = other.decimals + offset.DecimalFloat + (other.fraction.Int + offset.Int - fraction.Int);
        }

        public BeatUnit(in BeatUnit other, float offset) {
            var decimalsWithOffset = other.decimals + offset;
            var decimalsWithOffsetInFraction = Fraction.FromFloat(decimalsWithOffset);
            fraction = decimalsWithOffsetInFraction + other.fraction.Int;
            decimals = decimalsWithOffset - decimalsWithOffsetInFraction.Int;
        }

        public BeatUnit(in BeatUnit other, float offset, int denom) {
            var decimalsWithOffset = other.decimals + offset;
            var decimalsWithOffsetInFraction = Fraction.FromFloatRounded(decimalsWithOffset, denom);
            fraction = decimalsWithOffsetInFraction + other.fraction.Int;
            decimals = decimalsWithOffset - decimalsWithOffsetInFraction.Int;
        }

        public BeatUnit OffsetBy(int offset) => new BeatUnit(this, offset);
        public BeatUnit OffsetBy(Fraction offset) => new BeatUnit(this, offset);
        public BeatUnit OffsetBy(float offset) => new BeatUnit(this, offset);
        public BeatUnit OffsetBy(float offset, int denom) => new BeatUnit(this, offset, denom);

        public int CompareTo(BeatUnit other) {
            if (CompareUtil.TryCompareTo(fraction.Int, other.fraction.Int, out var result)) return result;
            return decimals.CompareTo(other.decimals);
        }

        public static implicit operator int(in BeatUnit obj) => obj.fraction.Int;
        public static implicit operator double(in BeatUnit obj) => obj.Value;
        public static implicit operator Fraction(in BeatUnit obj) => obj.fraction;
        public static implicit operator BeatUnit(int value) => new BeatUnit(value);
        public static implicit operator BeatUnit(double value) => new BeatUnit(value);
        public static implicit operator BeatUnit(Fraction value) => new BeatUnit(value);
    }

    public readonly struct TimeUnit : IComparable<TimeUnit>{
        public readonly int integer;
        public readonly float decimals;

        public double Value => integer + decimals;

        public TimeUnit(int value) {
            integer = value;
            decimals = 0;
        }

        public TimeUnit(double value) {
            integer = (int)Math.Floor(value);
            decimals = (float)(value - integer);
        }

        public TimeUnit(in TimeUnit other, int offset) {
            integer = other.integer + offset;
            decimals = other.decimals;
        }

        public TimeUnit(in TimeUnit other, float offset) {
            var decimalsWithOffset = other.decimals + offset;
            var decimalsWithOffsetInInteger = (int)Math.Floor(decimalsWithOffset);
            integer = decimalsWithOffsetInInteger + other.integer;
            decimals = decimalsWithOffset - decimalsWithOffsetInInteger;
        }

        public TimeUnit OffsetBy(int offset) => new TimeUnit(this, offset);
        public TimeUnit OffsetBy(float offset) => new TimeUnit(this, offset);

        public int CompareTo(TimeUnit other) {
            if (CompareUtil.TryCompareTo(integer, other.integer, out var result)) return result;
            return decimals.CompareTo(other.decimals);
        }

        public static implicit operator int(in TimeUnit obj) => obj.integer;
        public static implicit operator double(in TimeUnit obj) => obj.Value;
        public static implicit operator TimeUnit(int value) => new TimeUnit(value);
        public static implicit operator TimeUnit(double value) => new TimeUnit(value);
    }
}