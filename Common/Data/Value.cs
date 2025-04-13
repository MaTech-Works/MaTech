// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Optional;
using Optional.Unsafe;

namespace MaTech.Common.Data {
    public interface IValueUnit {
        public Option<T> Adopt<T>(in Variant value);
    }

    public static class Adopters<TUnit> where TUnit : IValueUnit, new() {
        public delegate T Adopter<out T>(in TUnit unit, in Variant value);
        private static class AdopterOf<T> {
            public static Adopter<T> func;
            public static Option<T> Invoke(in TUnit unit, in Variant value) => func is null ? Option.None<T>() : Option.Some(func(unit, value));
        }
        public static ref Adopter<T> To<T>() => ref AdopterOf<T>.func;
        public static Option<T> Adopt<T>(in TUnit unit, in Variant value) => AdopterOf<T>.Invoke(unit, value);
    }
    
    public class ScalarUnit : IValueUnit {
        public readonly Variant scale;

        public Option<T> Adopt<T>(in Variant value) => Adopters<ScalarUnit>.Adopt<T>(this, value);

        static ScalarUnit() {
            // todo: replace with the Rational struct and remove these math boilerplates
            Adopters<ScalarUnit>.To<int>() = (in ScalarUnit unit, in Variant value) => unit.scale.Int * value.Int;
            Adopters<ScalarUnit>.To<float>() = (in ScalarUnit unit, in Variant value) => unit.scale.Float * value.Float;
            Adopters<ScalarUnit>.To<double>() = (in ScalarUnit unit, in Variant value) => unit.scale.Double * value.Double;
            Adopters<ScalarUnit>.To<FractionMixed>() = (in ScalarUnit unit, in Variant value) => unit.scale.Fraction * value.Fraction;
            Adopters<ScalarUnit>.To<FractionSimple>() = (in ScalarUnit unit, in Variant value) => unit.scale.FractionSimple * value.FractionSimple;
            Adopters<ScalarUnit>.To<Variant>() = (in ScalarUnit unit, in Variant value) => value.Type switch {
                VariantType.Int => unit.scale.IsFloatPoint ? unit.scale.Double * value.Int : unit.scale.Fraction * value.Int,
                VariantType.Float when value.IsFloat => unit.scale.Float * value.Float,
                VariantType.Float => unit.scale.Double * value.Float,
                VariantType.Double => unit.scale.Double * value.Double,
                VariantType.Fraction => unit.scale.Fraction * value.Fraction,
                VariantType.FractionSimple => unit.scale.FractionSimple * value.FractionSimple,
                _ => Variant.None
            };
        }
        
        public ScalarUnit() => scale = 1;
        public ScalarUnit(in Variant scale = default) => this.scale = scale.IsNumeral ? scale : 1;
        public static implicit operator ScalarUnit(in Variant scale) => new(scale);

        public static ScalarUnit WithDivision(int division) => new(FractionSimple.Normal(1, division));
        public static ScalarUnit WithRatio(int numerator, int denominator) => new(FractionSimple.Normal(numerator, denominator));
        public static ScalarUnit WithScale(float scale) => new(scale);
        public static ScalarUnit WithScale(int scale) => new(scale);
        
        public static readonly ScalarUnit one = WithDivision(1);
        public static readonly ScalarUnit deci = WithDivision(10);
        public static readonly ScalarUnit centi = WithDivision(100);
        public static readonly ScalarUnit milli = WithDivision(1000);
        public static readonly ScalarUnit sec = WithDivision(60);
    }
    
    // todo: more units like TimeUnit or BeatUnit (notice that we have been using these names to describe actual values; to be renamed first)
    // todo: finish design on morphing the unit and transforming the value with that morph
    
    public readonly struct Value<TUnit> where TUnit : IValueUnit, new() {
        public Variant Base { get; }
        public TUnit Unit { get; }

        //public ValueWithUnit<T> To<T>(Morph<TUnit, T> morph) where T : IValueUnit, new() => default;
        public Option<T> As<T>() => Unit.Adopt<T>(Base);

        public Value(in Variant baseValue, in TUnit unit = default) {
            Base = baseValue;
            Unit = unit ?? defaultUnit;
        }

        private static readonly TUnit defaultUnit = new();
    }
}