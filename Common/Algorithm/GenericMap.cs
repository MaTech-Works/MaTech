// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Optional;
using Optional.Unsafe;
using static Optional.OptionExtensions;

namespace MaTech.Common.Algorithm {
    public class GenericMap<TSelf> {
        private readonly Dictionary<Type, Delegate> dict;

        public GenericMap(Action<GenericMap<TSelf>> init = null, int capacity = 2) { dict = new(capacity); init?.Invoke(this); }

        public GenericMap<TSelf> Add<T>(Func<TSelf, T> mappingToValue) { if (mappingToValue is not null) dict.Add(typeof(T), mappingToValue); return this; }
        public GenericMap<TSelf> Add<TKey, TValue>(Func<TSelf, Func<TKey, TValue>> mappingToFuncKeyValue) => Add<Func<TKey, TValue>>(mappingToFuncKeyValue);
        public GenericMap<TSelf> Add<TKey, TValue>(Func<TSelf, FuncIn<TKey, TValue>> mappingToFuncKeyValue) => Add<FuncIn<TKey, TValue>>(mappingToFuncKeyValue);
        
        public bool Has<T>() => dict.ContainsKey(typeof(T));
        public bool Has<TKey, TValue>() => dict.ContainsKey(typeof(Func<TKey, TValue>)) || dict.ContainsKey(typeof(FuncIn<TKey, TValue>));

        public Option<T> Get<T>(in TSelf self) => dict.TryGetValue(typeof(T), out Delegate f) ? ((Func<TSelf, T>)f)(self).Some() : Option.None<T>();
        public Option<TValue> Map<TKey, TValue>(in TSelf self, in TKey key) {
            if (Has<FuncIn<TKey, TValue>>()) Get<FuncIn<TKey, TValue>>(self).InvokeSome(key);
            return Get<Func<TKey, TValue>>(self).InvokeSome(key);
        }
    }

    public class GenericMap<TSelf, TValue> {
        private readonly Dictionary<Type, Delegate> dict;

        public GenericMap(Action<GenericMap<TSelf, TValue>> init = null, int capacity = 2) { dict = new(capacity); init?.Invoke(this); }

        public GenericMap<TSelf, TValue> Add<TKey>(Func<TSelf, Func<TKey, TValue>> mappingToFuncKeyValue) { dict.Add(typeof(TKey), mappingToFuncKeyValue); return this; }
        public GenericMap<TSelf, TValue> Add<TKey>(Func<TSelf, FuncIn<TKey, TValue>> mappingToFuncKeyValue) { dict.Add(typeof(TKey), mappingToFuncKeyValue); return this; }
        
        public bool Has<TKey>() => dict.ContainsKey(typeof(TKey));
        
        private Delegate Get<TKey>(in TSelf self) => dict.TryGetValue(typeof(TKey), out Delegate f) ? ((Func<TSelf, Delegate>)f)(self) : null;
        public Option<TValue> Map<TKey>(in TSelf self, in TKey key) {
            var map = dict.TryGetValue(typeof(TKey), out Delegate f) ? ((Func<TSelf, Delegate>)f)(self) : null;
            if (map is FuncIn<TKey, TValue> funcIn) return funcIn.Invoke(key).Some();
            if (map is Func<TKey, TValue> func) return func.Invoke(key).Some();
            return Option.None<TValue>();
        }
    }
}