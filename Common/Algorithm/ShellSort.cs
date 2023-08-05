// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public static class ShellSort {
        /// <summary>
        /// 根据CompareTo的结果，使用Hibbard序列进行希尔排序。
        /// </summary>
        public static void Hibbard<T>(IList<T> list) where T : IComparable<T> {
            int n = list.Count;
            
            // Hibbard序列：gap=2^i-1，从最大可能的间隔倒序递推
            int gap = 2;
            while (gap < n + 1) gap <<= 1;
            gap = (gap >> 1) - 1;
            
            for (; gap > 0; gap >>= 1) { // 间隔每次缩小一半，2的整数幂减一减半还是2的整数幂减一
                for (int i = gap, j; i < n; i += 1) { // gap间隔的反向插入排序
                    T t = list[i], s;
                    for (j = i; j >= gap && (s = list[j - gap]).CompareTo(t) > 0; j -= gap) {
                        list[j] = s;
                    }
                    list[j] = t;
                }
            }
        }

        /// <summary>
        /// 根据比较器的结果，使用Hibbard序列进行希尔排序。
        /// </summary>
        public static void Hibbard<T>(IList<T> list, IComparer<T> comparer) {
            int n = list.Count;
            
            // Hibbard序列：gap=2^i-1，从最大可能的间隔倒序递推
            int gap = 2;
            while (gap < n + 1) gap <<= 1;
            gap = (gap >> 1) - 1;
            
            for (; gap > 0; gap >>= 1) { // 间隔每次缩小一半，2的整数幂减一减半还是2的整数幂减一
                for (int i = gap, j; i < n; i += 1) { // gap间隔的反向插入排序
                    T t = list[i], s;
                    for (j = i; j >= gap && comparer.Compare(s = list[j - gap], t) > 0; j -= gap) {
                        list[j] = s;
                    }
                    list[j] = t;
                }
            }
        }
    }
}