// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Optional;

namespace MaTech.Common.Data {
    public interface IValueUnit {
        public Variant ApplyTo(in Variant value);
        public Option<T> To<T>();
    }

    public readonly struct ScalarUnit : IValueUnit {
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
    
    // todo: more units like TimeUnit or BeatUnit (notice that we have been using these names to describe actual values; to be renamed first)
    // todo: rename .Value and ApplyTo to something hinting morphing the unit into nothing and get a pure value, then rename ValueWithUnit to Value
    
    public readonly struct ValueWithUnit<TUnit> where TUnit : IValueUnit, new() {
        public Variant Base { get; }
        public TUnit Unit { get; }

        //public ValueWithUnit<T> To<T>(Morph<TUnit, T> morph) where T : IValueUnit, new() => default;
        //public Option<T> To<T>() => default;
        public Variant ToVariant => Unit.ApplyTo(Base);

        public ValueWithUnit(in Variant baseValue, in TUnit unit = default) {
            Base = baseValue;
            Unit = unit ?? defaultUnit;
        }
        
        public static implicit operator float(in ValueWithUnit<TUnit> v) => v.ToVariant.Float;
        public static implicit operator double(in ValueWithUnit<TUnit> v) => v.ToVariant.Double;
        public static implicit operator Fraction(in ValueWithUnit<TUnit> v) => v.ToVariant.Fraction;
        public static implicit operator FractionSimple(in ValueWithUnit<TUnit> v) => v.ToVariant.FractionSimple;
        public static implicit operator Variant(in ValueWithUnit<TUnit> v) => v.ToVariant;

        private static readonly TUnit defaultUnit = new();
    }
}