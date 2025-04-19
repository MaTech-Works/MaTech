// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using MaTech.Common.Data;
using Optional;
using Optional.Unsafe;

namespace MaTech.Gameplay.Data {
    public interface IPropertyDefine {
        public ScalarUnit UnitForInt => ScalarUnit.one;
    }

    // todo: overrides with non-constant decorations
    public readonly struct Property<TDefine> where TDefine : IPropertyDefine, new() {
        private static readonly IPropertyDefine define = new TDefine();
        
        //public readonly DataEnum<TEnum> key; // todo: this is for PropertySheet
        public readonly MetaValue<ScalarUnit> scalar;

        private Property(in Variant value, in ScalarUnit? unit = null) {
            scalar = new MetaValue<ScalarUnit>(value, unit ?? ScalarUnit.one);
        }
        
        public Option<T> As<T>() => scalar.As<T>();
        public T To<T>() => scalar.As<T>().ValueOrDefault();

        public static implicit operator Property<TDefine>(int number) => new(number, define.UnitForInt);
        public static implicit operator Property<TDefine>(float value) => new(value);
        public static implicit operator Property<TDefine>(double value) => new(value);
        public static implicit operator Property<TDefine>(in Scalar value) => new(value);
        public static implicit operator Property<TDefine>((int a, int b) x) => new(FractionImproper.Valid(x.a, x.b));
        public static implicit operator Property<TDefine>((int n, int a, int b) x) => new(FractionMixed.Valid(x.n, x.a, x.b));

        public static implicit operator Variant(in Property<TDefine> t) => t.To<Variant>();
        
        public static implicit operator int(in Property<TDefine> t) => t.To<int>();
        public static implicit operator float(in Property<TDefine> t) => t.To<float>();
        public static implicit operator double(in Property<TDefine> t) => t.To<double>();
        
        public static implicit operator Scalar(in Property<TDefine> t) => t.To<Scalar>();
        public static implicit operator FractionMixed(in Property<TDefine> t) => t.To<FractionMixed>();
        public static implicit operator FractionImproper(in Property<TDefine> t) => t.To<FractionImproper>();

        public static implicit operator TimeValue(in Property<TDefine> t) => TimeValue.FromMilliseconds(t.To<double>()); // todo: implement with unit adopt
    }

    // todo: finish TEnum and original design
    public class PropertySheet<TEnum> where TEnum : unmanaged, Enum, IConvertible {
        public PropertySheet<TEnum>? original;
        public Property<TDefine> Define<TDefine>(string name, Variant value) where TDefine : struct, IPropertyDefine => default;
    }
}