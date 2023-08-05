// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个固定使用List实现的Stack，将RemoveAt(Count-1)封装并提供Empty属性供检查。
    /// </summary>
    public class StackList<T> : List<T> {
        public StackList() : base() { }
        public StackList(IEnumerable<T> collection) : base(collection) { }
        public StackList(int capacity) : base(capacity) { }

        public bool Empty {
            get {
                return Count == 0;
            }
        }

        public T Pop() {
            int indexLast = Count - 1;
            T result = base[indexLast];
            RemoveAt(indexLast);
            return result;
        }

        public bool TryPop(out T result) {
            if (Empty) {
                result = default;
                return false;
            }
            int indexLast = Count - 1;
            result = base[indexLast];
            RemoveAt(indexLast);
            return true;
        }
    }
}