// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Data {
    public partial struct Variant {
        private Variant(bool value) {
            Type = VariantType.Bool;
            f = new FractionSimple(value ? 1 : 0);
            d = f.Numerator;
            o = null;
        }
        private Variant(int value) {
            Type = VariantType.Int;
            f = new FractionSimple(value);
            d = f.Numerator;
            o = null;
        }
        private Variant(float value) {
            Type = VariantType.Float;
            f = FractionSimple.FromFloat(value);
            d = value;
            o = null;
        }
        private Variant(double value) {
            Type = VariantType.Double;
            f = FractionSimple.FromFloat(value);
            d = value;
            o = null;
        }
        private Variant(Fraction value) {
            Type = VariantType.Fraction;
            f = value;
            d = f.Double;
            o = null;
        }
        private Variant(FractionSimple value) {
            Type = VariantType.FractionSimple;
            f = value;
            d = f.Double;
            o = null;
        }
        private Variant(string value) {
            if (value == null) this = None;
            else {
                Type = VariantType.String;
                f = FractionSimple.invalid;
                d = Double.NaN;
                o = value;
            }
        }
        private Variant(object value) {
            if (value == null) this = None;
            else {
                // no type infer, object in object out
                Type = VariantType.Object;
                f = FractionSimple.invalid;
                d = Double.NaN;
                o = value;
            }
        }
        
        private static readonly Type typeFraction = typeof(Fraction);
        private static readonly Type typeFractionSimple = typeof(FractionSimple);
        
        private static readonly HashSet<Type> typesConvertible = new HashSet<Type>() {
            typeof(bool),
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeFraction,
            typeFractionSimple,
        };
    }
}