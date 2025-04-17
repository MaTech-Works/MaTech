// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Optional;

namespace MaTech.Common.Data {
    // todo: RhythmicUnit (adopts into all ITimeValue)
    // todo: finish design on morphing the unit and transforming a set of MetaValue with that morph

    public readonly struct MetaValue<TUnit> where TUnit : IValueUnit, new() {
        public Variant Base { get; }
        public TUnit Unit { get; }

        //public ValueWithUnit<T> To<T>(Morph<TUnit, T> morph) where T : IValueUnit, new() => default;
        public Option<T> As<T>() => Unit.Adopt<T>(Base);

        public MetaValue(in Variant baseValue, in TUnit unit = default) {
            Base = baseValue;
            Unit = unit ?? defaultUnit;
        }

        private static readonly TUnit defaultUnit = new();
    }
}