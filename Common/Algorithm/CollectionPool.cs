// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public class CollectionPool<T, TItem> : Pool<T> where T : class, ICollection<TItem> {
        protected CollectionPool(int capacityPool, Func<T> factory)
            : base(factory, null, collection => collection.Clear(), capacityPool) { }
    }
    
    public class ListPool<T> : CollectionPool<List<T>, T> {
        public ListPool(int capacityPool = 1, int capacityCollection = 0)
            : base(capacityPool, () => new List<T>(capacityCollection)) { }
    }
    
    public class HashSetPool<T> : CollectionPool<HashSet<T>, T> {
        public HashSetPool(int capacityPool = 1, int capacityCollection = 0)
            : base(capacityPool, () => new HashSet<T>(capacityCollection)) { }
    }
    
    public class DictPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> {
        public DictPool(int capacityPool = 1, int capacityCollection = 0)
            : base(capacityPool, () => new Dictionary<TKey, TValue>(capacityCollection)) { }
    }
}