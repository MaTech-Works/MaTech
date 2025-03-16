// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public class CollectionPool<T, TItem> : Pool<T> where T : class, ICollection<TItem> {
        protected CollectionPool(int spare, Func<T> factory) : base(factory, null, collection => collection.Clear()) { SpareCount = spare; }
    }
    
    public class ListPool<T> : CollectionPool<List<T>, T> {
        public ListPool(int spare = 1, int capacityOfCollection = 0) : base(spare, () => new List<T>(capacityOfCollection)) { }
    }
    public class HashSetPool<T> : CollectionPool<HashSet<T>, T> {
        public HashSetPool(int spare = 1, int capacityOfCollection = 0) : base(spare, () => new HashSet<T>(capacityOfCollection)) { }
    }
    public class DictPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> {
        public DictPool(int spare = 1, int capacityOfCollection = 0) : base(spare, () => new Dictionary<TKey, TValue>(capacityOfCollection)) { }
    }
}