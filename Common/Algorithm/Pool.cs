// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public readonly struct Recycler<T> : IDisposable where T : class {
        public interface IRecycle { public bool Recycle(T item); }
        public readonly IRecycle target;
        public readonly T? value;
        public Recycler(IRecycle target, T? value) { this.target = target; this.value = value; }
        public static implicit operator bool(in Recycler<T> item) => item.value is not null;
        public static implicit operator T?(in Recycler<T> item) => item.value;
        public void Dispose() { if (value is not null) target.Recycle(value); }
    }
    
    public class Pool<T> : Recycler<T>.IRecycle where T : class {
        private readonly Bin<T> bin = new();
        private readonly Func<T> factory;
        private readonly Action<T>? init;
        private readonly Action<T>? reset;

        public Pool(Func<T> factory, Action<T>? init = null, Action<T>? reset = null, int spare = 0) {
            this.factory = factory;
            this.init = init;
            this.reset = reset;
            SpareCount = spare;
        }

        public int SpareCount {
            get => bin.Count;
            set {
                int delta = Math.Max(value, 0) - bin.Count;
                for (; delta > 0; --delta) bin.Recycle(factory.Invoke());
                for (; delta < 0; ++delta) bin.Trim();
            }
        }

        public Recycler<T> Get() {
            var item = bin.Get().value ?? factory.Invoke();
            if (item is not null) init?.Invoke(item);
            return new(this, item);
        }

        public bool Recycle(T item) {
            if (bin.Recycle(item)) {
                reset?.Invoke(item);
                return true;
            }
            return false;
        }
    }
    
    public class Pool<T, TData> : Recycler<T>.IRecycle where T : class {
        private readonly Bin<T> bin = new();
        private readonly Func<T> factory;
        private readonly InitFunc? init;
        private readonly Action<T>? reset;

        public delegate void InitFunc(T item, in TData data);
        private static InitFunc? With(Action<T, TData>? init) => init is null ? null : (T item, in TData data) => init(item, data);

        public Pool(Func<T> factory, Action<T, TData>? init = null, Action<T>? reset = null) {
            this.factory = factory;
            this.init = With(init);
            this.reset = reset;
        }
        public Pool(Func<T> factory, InitFunc? init = null, Action<T>? reset = null) {
            this.factory = factory;
            this.init = init;
            this.reset = reset;
        }

        public int SpareCount {
            get => bin.Count;
            set {
                int delta = Math.Max(value, 0) - bin.Count;
                for (; delta > 0; --delta) bin.Recycle(factory.Invoke());
                for (; delta < 0; ++delta) bin.Trim();
            }
        }

        public Recycler<T> Get(in TData data) {
            var item = bin.Get().value ?? factory.Invoke();
            if (item is not null) init?.Invoke(item, data);
            return new(this, item);
        }

        public bool Recycle(T item) {
            if (bin.Recycle(item)) {
                reset?.Invoke(item);
                return true;
            }
            return false;
        }
    }
    
    public class Bin<T> : Recycler<T>.IRecycle where T : class {
        private readonly HashSet<T> spare = new();
        private readonly Stack<T> recent = new();
        
        public virtual int Count => spare.Count;

        public Recycler<T> Get() {
            if (recent.TryPop(out var item)) return new(this, item);
            return new(this, null);
        } 
        
        public bool Recycle(T item) {
            if (spare.Contains(item)) return false;
            recent.Push(item);
            spare.Add(item);
            return true;
        }

        public bool Trim() {
            if (!recent.TryPop(out var item)) return false;
            if (item is IDisposable disposable)
                disposable.Dispose();
            spare.Remove(item);
            return true;
        }
    }
}