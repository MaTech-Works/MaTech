// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Data {
    public interface IValueUnit<TValue> {
        public Variant ApplyTo(in TValue value);
    }

    public readonly struct ScalarUnit : IValueUnit<Variant> {
        public readonly Variant scale;
        
        // todo: replace with the Rational struct and remove these math boilerplates
        public Variant ApplyTo(in Variant value) => value.Type switch {
            VariantType.Int => scale.IsFloatPoint ? scale.Double * value.Int : scale.Fraction * value.Int,
            VariantType.Float when value.IsFloat => scale.Float * value.Float,
            VariantType.Float => scale.Double * value.Float,
            VariantType.Double => scale.Double * value.Double,
            VariantType.Fraction => scale.Fraction * value.Fraction,
            VariantType.FractionSimple => scale.FractionSimple * value.FractionSimple,
            _ => Variant.None
        };
        
        public ScalarUnit(in Variant scale = default) { this.scale = scale.IsNumeral ? scale : 1; }
        public static implicit operator ScalarUnit(in Variant scale) => new(scale);

        public static ScalarUnit WithDivision(int division) => (Variant)FractionSimple.Normal(1, division);
        public static ScalarUnit WithRatio(int numerator, int denominator) => (Variant)FractionSimple.Normal(numerator, denominator);
        public static ScalarUnit WithScale(float scale) => new(scale);
        public static ScalarUnit WithScale(int scale) => new(scale);
        
        public static readonly ScalarUnit one = WithDivision(1);
        public static readonly ScalarUnit deci = WithDivision(10);
        public static readonly ScalarUnit centi = WithDivision(100);
        public static readonly ScalarUnit milli = WithDivision(1000);
        public static readonly ScalarUnit sec = WithDivision(60);
    }
    
    // todo: more units like TimeUnit or BeatUnit? (notice that we have been using these names to describe actual values; to be renamed first)
    
    public readonly struct ValueWithUnit<TValue, TUnit> where TUnit : IValueUnit<TValue>, new() {
        public TValue Value { get; }
        public TUnit Unit { get; }
        
        public Type UnitType => typeof(TUnit);
        public Variant Scaled => Unit.ApplyTo(Value);

        public ValueWithUnit(in TValue value, in TUnit unit = default) {
            Value = value;
            Unit = unit ?? defaultUnit;
        }
        
        public static implicit operator float(in ValueWithUnit<TValue, TUnit> v) => v.Scaled.Float;
        public static implicit operator double(in ValueWithUnit<TValue, TUnit> v) => v.Scaled.Double;
        public static implicit operator Fraction(in ValueWithUnit<TValue, TUnit> v) => v.Scaled.Fraction;
        public static implicit operator FractionSimple(in ValueWithUnit<TValue, TUnit> v) => v.Scaled.FractionSimple;
        public static implicit operator Variant(in ValueWithUnit<TValue, TUnit> v) => v.Scaled;

        private static readonly TUnit defaultUnit = new();
    }
}