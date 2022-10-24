// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// Object pool to avoid GC in gameplay. Needs manual recycles.
    /// If a constructor function is given, Get() will automatically make new objects with the constructor function.
    /// </summary>
    public class ObjectPool<T> where T : class {
        private Stack<T> spare;
        private Func<T> factory;
        private Action<T> init;
        private Action<T> reset;

        public ObjectPool(Func<T> factory = null, int initialCapacity = 0) {
            this.spare = new Stack<T>(initialCapacity);
            this.factory = factory;
        }

        public ObjectPool(Func<T> factory, Action<T> init, Action<T> reset, int initialCapacity = 0) {
            this.spare = new Stack<T>(initialCapacity);
            this.factory = factory;
            this.init = init;
            this.reset = reset;
        }

        public T Get() {
            var obj = spare.Count > 0 ? spare.Pop() : factory?.Invoke();
            init?.Invoke(obj);
            return obj;
        }

        public void Recycle(T obj) {
            reset?.Invoke(obj);
            spare.Push(obj);
        }
    }
}