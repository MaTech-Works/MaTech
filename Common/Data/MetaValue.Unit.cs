// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Optional;

namespace MaTech.Common.Data {
    public interface IValueUnit {
        public Option<T> Adopt<T>(in Variant value);
    }

    public class AdopterUnit<TUnit> : IValueUnit where TUnit : AdopterUnit<TUnit>, new() {
        private static class Adopters<T> {
            public static Adopter<T> func;
            public static Option<T> Invoke(in TUnit unit, in Variant value) => func is null ? Option.None<T>() : Option.Some(func(unit, value));
        }
        
        protected static ref Adopter<T> AdopterTo<T>() => ref Adopters<T>.func;

        public delegate T Adopter<out T>(in TUnit unit, in Variant value);
        public Option<T> Adopt<T>(in Variant value) => Adopters<T>.Invoke((TUnit)this, value);
    }
}