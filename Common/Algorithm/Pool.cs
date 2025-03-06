// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public readonly struct Recycler<T> : IDisposable where T : class {
        public interface IRecycle { public void Recycle(T item); }
        public readonly IRecycle target;
        public readonly T value;
        public Recycler(IRecycle target, T value) { this.target = target; this.value = value; }
        public static implicit operator T(Recycler<T> item) => item.value;
        public void Dispose() => target.Recycle(value);
    }
    
    public class Pool<T> : Recycler<T>.IRecycle where T : class {
        private readonly Stack<T> spare;
        private readonly Func<T> factory;
        private readonly Action<T> init;
        private readonly Action<T> reset;

        public Pool(Func<T> factory = null, int initialCapacity = 0) {
            this.spare = new Stack<T>(initialCapacity);
            this.factory = factory;
        }

        public Pool(Func<T> factory, Action<T> init, Action<T> reset, int initialCapacity = 0) {
            this.spare = new Stack<T>(initialCapacity);
            this.factory = factory;
            this.init = init;
            this.reset = reset;
        }

        public Recycler<T> Get() {
            var item = spare.Count > 0 ? spare.Pop() : factory?.Invoke();
            init?.Invoke(item);
            return new(this, item);
        }

        public void Recycle(T item) {
            reset?.Invoke(item);
            spare.Push(item);
        }
    }
    
    public class Pool<T, TData> : Recycler<T>.IRecycle where T : class {
        private readonly Stack<T> spare;
        private readonly Func<T> factory;
        private readonly Action<T, TData> init;
        private readonly Action<T> reset;

        public Pool(Func<T> factory = null, int initialCapacity = 0) {
            this.spare = new Stack<T>(initialCapacity);
            this.factory = factory;
        }

        public Pool(Func<T> factory, Action<T, TData> init, Action<T> reset, int initialCapacity = 0) {
            this.spare = new Stack<T>(initialCapacity);
            this.factory = factory;
            this.init = init;
            this.reset = reset;
        }

        public Recycler<T> Get(in TData data) {
            var item = spare.Count > 0 ? spare.Pop() : factory?.Invoke();
            init?.Invoke(item, data);
            return new(this, item);
        }

        public void Recycle(T item) {
            reset?.Invoke(item);
            spare.Push(item);
        }
    }
}