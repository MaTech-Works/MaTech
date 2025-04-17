// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Data {
    public class ScalarUnit : AdopterUnit<ScalarUnit> {
        public readonly Variant scale;

        static ScalarUnit() {
            // todo: replace with the Scalar struct and remove these math boilerplates
            AdopterTo<int>() = (in ScalarUnit unit, in Variant value) => unit.scale.Int * value.Int;
            AdopterTo<float>() = (in ScalarUnit unit, in Variant value) => unit.scale.Float * value.Float;
            AdopterTo<double>() = (in ScalarUnit unit, in Variant value) => unit.scale.Double * value.Double;
            AdopterTo<FractionMixed>() = (in ScalarUnit unit, in Variant value) => unit.scale.Mixed * value.Mixed;
            AdopterTo<FractionImproper>() = (in ScalarUnit unit, in Variant value) => unit.scale.Improper * value.Improper;
            AdopterTo<Variant>() = (in ScalarUnit unit, in Variant value) => value.Type switch {
                VariantType.Int => unit.scale.IsFloatPoint ? unit.scale.Double * value.Int : unit.scale.Mixed * value.Int,
                VariantType.Float when value.IsFloat => unit.scale.Float * value.Float,
                VariantType.Float => unit.scale.Double * value.Float,
                VariantType.Double => unit.scale.Double * value.Double,
                VariantType.Fraction => unit.scale.Mixed * value.Mixed,
                VariantType.FractionSimple => unit.scale.Improper * value.Improper,
                _ => Variant.None
            };
        }
        
        public ScalarUnit() => scale = 1;
        public ScalarUnit(in Variant scale = default) => this.scale = scale.IsNumeral ? scale : 1;
        public static implicit operator ScalarUnit(in Variant scale) => new(scale);

        public static ScalarUnit WithDivision(int division) => new(FractionImproper.Normal(1, division));
        public static ScalarUnit WithRatio(int numerator, int denominator) => new(FractionImproper.Normal(numerator, denominator));
        public static ScalarUnit WithScale(float scale) => new(scale);
        public static ScalarUnit WithScale(int scale) => new(scale);
        
        public static readonly ScalarUnit one = WithDivision(1);
        public static readonly ScalarUnit deci = WithDivision(10);
        public static readonly ScalarUnit centi = WithDivision(100);
        public static readonly ScalarUnit milli = WithDivision(1000);
        public static readonly ScalarUnit sec = WithDivision(60);
    }
}