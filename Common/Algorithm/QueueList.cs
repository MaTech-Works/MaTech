// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个可以从开头顺序取出元素的特殊List，取出元素后不会从容器中移除元素，并且可以重设下一次取出元素的下标。
    /// </summary>
    public class QueueList<T> : List<T> {
        public int indexNext;

        public QueueList() { }
        public QueueList(IEnumerable<T> collection) : base(collection) { }
        public QueueList(int capacity) : base(capacity) { }

        public T Next() => base[indexNext++];
        public T Peek() => base[indexNext];

        public T NextOr(T fallback = default) => HasNext ? Next() : fallback;
        public T PeekOr(T fallback = default) => HasNext ? Peek() : fallback;
        public T NextOrDefault() => HasNext ? Next() : default;
        public T PeekOrDefault() => HasNext ? Peek() : default;

        public T NextIf(Predicate<T> predicate, T fallback = default) => HasNext && predicate(Peek()) ? Next() : fallback;
        public T PeekIf(Predicate<T> predicate, T fallback = default) => HasNext && predicate(Peek()) ? Peek() : fallback;
        public T NextIf<TParam>(in TParam param, Func<T, TParam, bool> predicate, T fallback = default) => HasNext && predicate(Peek(), param) ? Next() : fallback;
        public T PeekIf<TParam>(in TParam param, Func<T, TParam, bool> predicate, T fallback = default) => HasNext && predicate(Peek(), param) ? Peek() : fallback;

        public bool HasNext => indexNext < Count;
        public int RemainCount => Count - indexNext;
        
        public void Skip() => ++indexNext;
        public void Rewind() => --indexNext;
        public void Restart() => indexNext = 0;
        public new void Clear() { Restart(); base.Clear(); }
        
        public void RemovePassed() { RemoveRange(0, Math.Min(indexNext, Count)); Restart(); }
    }

    public static class QueueListExtensions {
        public static QueueList<T> ToQueueList<T>(this IEnumerable<T> e) => new QueueList<T>(e);
    }
}