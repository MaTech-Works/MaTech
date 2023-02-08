// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个可以从开头顺序取出元素的特殊List，取出元素后不会从容器中移除元素，并且可以重设下一次取出元素的下标。
    /// </summary>
    public class QueueList<T> : List<T> {
        private int indexNext;

        public QueueList() : base() { }
        public QueueList(IEnumerable<T> collection) : base(collection) { }
        public QueueList(int capacity) : base(capacity) { }

        public int IndexNext {
            get { return indexNext; }
            set { indexNext = value; }
        }

        public int RemainCount => Count - indexNext;
        public bool HasNext => indexNext < Count;

        public T Next() {
            return base[indexNext++];
        }

        public T Peek() {
            return base[indexNext];
        }

        public void Skip() {
            ++indexNext;
        }

        public void Restart() {
            indexNext = 0;
        }

        public void ClearAndRestart() {
            indexNext = 0;
            Clear();
        }

        public T GetNextOrDefault(T defaultValue = default) {
            if (HasNext) return Next();
            return defaultValue;
        }

        public T PeekOrDefault(T defaultValue = default) {
            if (HasNext) return Peek();
            return defaultValue;
        }
    }

    public static class QueueListExtensions {
        public static QueueList<T> ToQueueList<T>(this IEnumerable<T> e) => new QueueList<T>(e);
    }
}