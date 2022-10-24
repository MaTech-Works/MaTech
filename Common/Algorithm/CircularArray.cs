// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Algorithm {
    public class CircularArray<T> {
        private T[] baseArray;
        private int total;
        private int cursor;

        public CircularArray(int count) {
            total = count;
            baseArray = new T[count];
        }

        public void Add(T item) {
            baseArray[cursor++] = item;
            if (cursor == total) {
                cursor = 0;
            }
        }

        public T Find(Predicate<T> match) {
            for (var i = 0; i < total; i++) {
                if (match(baseArray[i])) {
                    return baseArray[i];
                }
            }

            return default;
        }
    }
}