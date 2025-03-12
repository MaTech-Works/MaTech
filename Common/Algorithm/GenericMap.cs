// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Optional;

namespace MaTech.Common.Algorithm {
    public class GenericDelegate {
        private readonly Dictionary<Type, Delegate> dict = new(2);
        
        public GenericDelegate(Action<GenericDelegate> init = null) { init?.Invoke(this); } // new(self => self.Set(...).Set(...)...)
        
        public GenericDelegate Define<T>(Delegate f) => Define(typeof(T), f);
        public GenericDelegate Define(Type type, Delegate f) { if (f is not null) dict[type] = f; return this; }
        
        public bool Has<T>() => Has(typeof(T));
        public bool Has(Type type) => dict.ContainsKey(type);
        
        public TFunc To<T, TFunc>() where TFunc : Delegate => To<TFunc>(typeof(T));
        public TFunc To<TFunc>(Type type) where TFunc : Delegate => To(type) as TFunc;
        public Delegate To<T>() => To(typeof(T));
        public Delegate To(Type type) => dict.GetValueOrDefault(type);
    }
    
    public class GenericDelegate<TFunc> where TFunc : Delegate {
        private readonly Dictionary<Type, TFunc> dict = new(2);
        
        public GenericDelegate(Action<GenericDelegate<TFunc>> init = null) { init?.Invoke(this); } // new(self => self.Set(...).Set(...)...)
        
        public GenericDelegate<TFunc> Define<T>(TFunc f) => Define(typeof(T), f);
        public GenericDelegate<TFunc> Define(Type type, TFunc f) { if (f is not null) dict[type] = f; return this; }
        
        public bool Has<T>() => Has(typeof(T));
        public bool Has(Type type) => dict.ContainsKey(type);
        
        public TFunc To<T>() => To(typeof(T));
        public TFunc To(Type type) => dict.GetValueOrDefault(type);
    }
    
    public class GenericProperty<TSelf> {
        private readonly GenericDelegate getters = new();
        private readonly GenericDelegate setters = new();
        
        public delegate void Setter<TValue>(TSelf self, in TValue key);
        
        public GenericProperty(Action<GenericProperty<TSelf>> init = null) { init?.Invoke(this); } // new(self => self.Define(...).Define(...)...)
        public GenericProperty<TSelf> Define<T>(Func<TSelf, T> getter) { getters.Define<T>(getter); return this; }
        public GenericProperty<TSelf> Define<T>(Action<TSelf, T> setter) { setters.Define<T>(setter); return this; }
        public GenericProperty<TSelf> Define<T>(Setter<T> setter) { setters.Define<T>(setter); return this; }
        public GenericProperty<TSelf> Define<T>(Func<TSelf, T> getter, Action<TSelf, T> setter) { getters.Define<T>(getter); setters.Define<T>(setter); return this; }
        public GenericProperty<TSelf> Define<T>(Func<TSelf, T> getter, Setter<T> setter) { getters.Define<T>(getter); setters.Define<T>(setter); return this; }
        
        public bool Has<T>() => getters.Has<T>() || setters.Has<T>();
        public bool Has<T>(bool getter) => getter ? getters.Has<T>() : setters.Has<T>();
        public Option<T> Get<T>(in TSelf self) => getters.To<T, Func<TSelf, T>>() is {} f ? f(self).Some() : Option.None<T>();

        public bool Set<T>(in TSelf self, in T value) {
            var d = setters.To<T>();
            if (d is Action<TSelf, T> action) action(self, value);
            else if (d is Setter<T> setter) setter(self, value);
            return d is not null;
        }
    }
    
    public class GenericMapping<TSelf> {
        private readonly GenericDelegate delegates = new();
        
        public delegate TValue Mapping<TKey, out TValue>(TSelf self, in TKey key);

        public GenericMapping(Action<GenericMapping<TSelf>> init = null) { init?.Invoke(this); } // new(self => self.Define(...).Define(...)...)
        public GenericMapping<TSelf> Define<TKey, TValue>(Func<TSelf, TKey, TValue> mappingKeyValue) { delegates.Define<TKey>(mappingKeyValue); return this; }
        public GenericMapping<TSelf> Define<TKey, TValue>(Mapping<TKey, TValue> mappingKeyValue) { delegates.Define<TKey>(mappingKeyValue); return this; }
        
        public bool Has<TKey, TValue>() => delegates.Has<Func<TSelf, TKey, TValue>>() || delegates.Has<Mapping<TKey, TValue>>();

        public Option<TValue> Map<TKey, TValue>(in TSelf self, in TKey key) {
            var d = delegates.To<TKey>();
            if (d is Func<TSelf, TKey, TValue> func) return func(self, key).Some();
            if (d is Mapping<TKey, TValue> mapping) return mapping(self, key).Some();
            return Option.None<TValue>();
        }
    }

    public class GenericMapping<TSelf, TValue> {
        private readonly GenericDelegate delegates = new();
        
        public delegate TValue Mapping<TKey>(TSelf self, in TKey key);
        
        public GenericMapping(Action<GenericMapping<TSelf, TValue>> init = null) { init?.Invoke(this); } // new(self => self.Define(...).Define(...)...)
        public GenericMapping<TSelf, TValue> Define<TKey>(Func<TSelf, TKey, TValue> mappingKeyValue) { delegates.Define<TKey>(mappingKeyValue); return this; }
        public GenericMapping<TSelf, TValue> Define<TKey>(Mapping<TKey> mappingKeyValue) { delegates.Define<TKey>(mappingKeyValue); return this; }
        
        public bool Has<TKey>() => delegates.Has<Func<TSelf, TKey, TValue>>() || delegates.Has<Mapping<TKey>>();
        
        public Option<TValue> Map<TKey>(in TSelf self, in TKey key) {
            var d = delegates.To<TKey>();
            if (d is Func<TSelf, TKey, TValue> func) return func(self, key).Some();
            if (d is Mapping<TKey> mapping) return mapping(self, key).Some();
            return Option.None<TValue>();
        }
    }
    
    // there is nothing here like std::bind. do it with value tuples please.
}