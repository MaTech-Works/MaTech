// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

#nullable enable

namespace MaTech.Common.Data {
    public partial class MetaTable<TEnum> {
        private class ValueDict<T> : Dictionary<EnumEx<TEnum>, T> {
            public static Func<ValueDict<T>> Create { get; } = () => new ValueDict<T>();
            public static ValueDict<T> Empty { get; } = Create();
        }

        private class ValueDictByType : Dictionary<Type, IDictionary> {
            public ValueDict<T> GetOrEmpty<T>() => (ValueDict<T>)this.GetValueOrDefault(typeof(T), ValueDict<T>.Empty);
            public ValueDict<T> GetOrCreate<T>() => (ValueDict<T>)this.GetOrCreate(typeof(T), ValueDict<T>.Create);
        }

        private readonly ValueDictByType dictByType = new ValueDictByType();
    }
}