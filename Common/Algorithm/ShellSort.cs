// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace MaTech.Common.Algorithm {
    public static class Sort {
        /// <summary>
        /// 根据IComparable.CompareTo的结果或者默认的比较规则，使用Hibbard序列进行希尔排序。
        /// </summary>
        public static void ShellSortGeneric<T>(IList<T> list) where T : IComparable<T> {
            Assert.IsNotNull(list);
            int n = list.Count, i, j;
            // Hibbard序列，gap=2^i-1
            // 查找序列里最大可能的间隔，然后倒序使用
            int gap = 2;
            while (gap < n + 1) gap <<= 1;
            gap = (gap >> 1) - 1;
            // 正式的希尔排序
            T t, s;
            for (; gap > 0; gap >>= 1) { // 间隔每次缩小一半，2的整数幂减一减半还是2的整数幂减一
                for (i = gap; i < n; i += 1) { // gap间隔的反向插入排序
                    t = list[i];
                    for (j = i; j >= gap && (s = list[j - gap]).CompareTo(t) > 0; j -= gap) {
                        list[j] = s;
                    }
                    list[j] = t;
                }
            }
        }

        /// <summary>
        /// 根据IComparable<T>.CompareTo的结果或者默认的比较规则，使用Hibbard序列进行希尔排序。
        /// </summary>
        public static void ShellSort<T>(IList<T> list) where T : IComparable<T> {
            Assert.IsNotNull(list);
            int n = list.Count, i, j;
            // Hibbard序列，gap=2^i-1
            // 查找序列里最大可能的间隔，然后倒序使用
            int gap = 2;
            while (gap < n + 1) gap <<= 1;
            gap = (gap >> 1) - 1;
            // 正式的希尔排序
            T t, s;
            for (; gap > 0; gap >>= 1) { // 间隔每次缩小一半，2的整数幂减一减半还是2的整数幂减一
                for (i = gap; i < n; i += 1) { // gap间隔的反向插入排序
                    t = list[i];
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
        public static void ShellSort<T>(IList<T> list, IComparer<T> comparer) {
            Assert.IsNotNull(list);
            Assert.IsNotNull(comparer);
            int n = list.Count, i, j;
            // Hibbard序列，gap=2^i-1
            // 查找序列里最大可能的间隔，然后倒序使用
            int gap = 2;
            while (gap < n + 1) gap <<= 1;
            gap = (gap >> 1) - 1;
            // 正式的希尔排序
            T t, s;
            for (; gap > 0; gap >>= 1) { // 间隔每次缩小一半，2的整数幂减一减半还是2的整数幂减一
                for (i = gap; i < n; i += 1) { // gap间隔的反向插入排序
                    t = list[i];
                    for (j = i; j >= gap && comparer.Compare(s = list[j - gap], t) > 0; j -= gap) {
                        list[j] = s;
                    }
                    list[j] = t;
                }
            }
        }
    }
}