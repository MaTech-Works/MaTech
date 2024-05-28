// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace MaTech.Common.Algorithm {
    public static class CollectionsExtension {
        public enum ResolveEqual {
            BeforeEqual, AfterEqual, ReplaceFirstEqual
        }
        
        /// <summary>
        /// 将元素插入一个有序的列表，在无序列表上不保证结果。
        /// </summary>
        /// <returns> 返回新插入元素的下标 </returns>
        /// <exception cref="ArgumentOutOfRangeException"> 当指定了未定义的resolve枚举值时丢出异常 </exception>
        public static int OrderedInsert<T>(this List<T> list, T item, ResolveEqual resolve = ResolveEqual.AfterEqual) where T : IComparable<T> {
            int index = list.BinarySearch(item);
            if (index < 0) {
                index = ~index;
                list.Insert(index, item);
            } else switch (resolve) {
            case ResolveEqual.BeforeEqual:
                list.Insert(index, item);
                break;
            case ResolveEqual.AfterEqual:
                for (int n = list.Count; index < n && list[index].CompareTo(item) == 0; ++index) { }
                list.Insert(index, item);
                break;
            case ResolveEqual.ReplaceFirstEqual:
                list[index] = item;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resolve), resolve, "Unsupported ResolveEqual value for OrderedInsert");
            }
            return index;
        }

        /// <summary>
        /// 将元素插入一个有序的列表，在无序列表上不保证结果。
        /// </summary>
        /// <returns> 返回新插入元素的下标 </returns>
        /// <exception cref="ArgumentOutOfRangeException"> 当指定了未定义的resolve枚举值时丢出异常 </exception>
        public static int OrderedInsert<T>(this List<T> list, T item, IComparer<T> comparer, ResolveEqual resolve = ResolveEqual.AfterEqual) {
            int index = list.BinarySearch(item, comparer);
            if (index < 0) {
                index = ~index;
                list.Insert(index, item);
            } else switch (resolve) {
            case ResolveEqual.BeforeEqual:
                list.Insert(index, item);
                break;
            case ResolveEqual.AfterEqual:
                for (int n = list.Count; index < n && comparer.Compare(list[index], item) == 0; ++index) { }
                list.Insert(index, item);
                break;
            case ResolveEqual.ReplaceFirstEqual:
                list[index] = item;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resolve), resolve, "Unsupported ResolveEqual value for OrderedInsert");
            }
            return index;
        }

        /// <summary> 使用非列表元素的值 <b>顺序搜索</b> 列表。 </summary>
        /// <returns> 第一个匹配元素的下标，若无则返回-1 </returns>
        public static int IndexOfFirstMatchedValue<T, TValue>(this List<T> list, TValue value, Func<T, TValue, bool> match) {
            for (int i = 0, n = list.Count; i < n; ++i) {
                var item = list[i];
                if (match(item, value))
                    return i;
            }
            return -1;
        }
        
        /// <summary> 使用非列表元素的值 <b>顺序搜索</b> 列表。 </summary>
        /// <returns> 最后一个匹配元素的下标，若无则返回-1 </returns>
        public static int IndexOfLastMatchedValue<T, TValue>(this List<T> list, TValue value, Func<T, TValue, bool> match) {
            for (int i = list.Count - 1; i >= 0; --i) {
                var item = list[i];
                if (match(item, value))
                    return i;
            }
            return -1;
        }
        
        /// <summary> 使用非列表元素的值 <b>二分搜索</b> 有序列表，在无序列表上不保证返回结果。 </summary>
        /// <param name="matchOrAfterValue"> 对搜索目标以及所有排列在后面的元素返回true </param>
        /// <returns> 第一个匹配元素的下标，若无则返回-1 </returns>
        public static int IndexOfFirstMatchedValueBinarySearch<T, TValue>(this List<T> list, TValue value, Func<T, TValue, bool> matchOrAfterValue) {
            if (list.Count == 0 || !matchOrAfterValue(list.Last(), value))
                return -1;

            uint lower = 0, upper = (uint)list.Count - 1;
            while (lower < upper) {
                uint middle = (lower + upper) >> 1; // all indices are originally non-negative signed integer (31 bits), so this is safe
                if (matchOrAfterValue(list[(int)middle], value)) {
                    upper = middle;
                } else {
                    lower = middle + 1;
                }
            }
                
            return (int)upper;
        }
        
        /// <summary> 使用非列表元素的值 <b>二分搜索</b> 有序列表，在无序列表上不保证返回结果。 </summary>
        /// <param name="matchOrBeforeValue"> 对搜索目标以及所有排列在前面的元素返回true </param>
        /// <returns> 最后一个匹配元素的下标，若无则返回-1 </returns>
        public static int IndexOfLastMatchedValueBinarySearch<T, TValue>(this List<T> list, TValue value, Func<T, TValue, bool> matchOrBeforeValue) {
            if (list.Count == 0 || !matchOrBeforeValue(list.First(), value))
                return -1;

            uint lower = 0, upper = (uint)list.Count - 1;
            while (lower < upper) {
                uint middle = (lower + upper + 1) >> 1; // all indices are originally non-negative signed integer (31 bits), so this is safe
                if (matchOrBeforeValue(list[(int)middle], value)) {
                    lower = middle;
                } else {
                    upper = middle - 1;
                }
            }
                
            return (int)lower;
        }

        public static void Resize<T>(this List<T?> list, int size) {
            // note: T? means defaultable rather than nullable
            if (size < list.Count) list.RemoveRange(size, list.Count - size);
            else while (size > list.Count) {
                list.Add(default);
            }
        }

        public static bool RemoveCyclic<T>(this List<T> list, in T value) {
            int index = list.IndexOf(value);
            if (index == -1) return false;
            list.RemoveCyclicAt(index);
            return true;
        }

        public static void RemoveCyclicAt<T>(this List<T> list, int index) {
            int indexLast = list.Count - 1;
            list[index] = list[indexLast];
            list.RemoveAt(indexLast);
        }

        public static void RemoveCyclicWhere<T>(this List<T> list, Func<T, bool> conditionRemove) {
            for (int i = 0, n = list.Count; i < n;) {
                if (conditionRemove(list[i])) {
                    list.RemoveCyclicAt(i);
                    --n;
                } else {
                    ++i;
                }
            }
        }

        public static T? PeekNullable<T>(this Queue<T> queue) {
            return queue.TryPeek(out T result) ? result : default;
        }

        public static void DisposeAllValuesAndClear<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TValue : IDisposable {
            if (dict.Count == 0) return;
            foreach (var value in dict.Values) {
                value.Dispose();
            }
            dict.Clear();
        }

        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> create) {
            if (!dict.TryGetValue(key, out var value))
                dict.Add(key, value = create(key));
            return value;
        }
        
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> create) {
            if (!dict.TryGetValue(key, out var value))
                dict.Add(key, value = create());
            return value;
        }
        
        public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new() {
            if (!dict.TryGetValue(key, out var value))
                dict.Add(key, value = new TValue());
            return value;
        }

        public static TValue GetOrNewDerived<TKey, TValue, TDerived>(this Dictionary<TKey, TValue> dict, TKey key) where TDerived : TValue, new() {
            if (!dict.TryGetValue(key, out var value))
                dict.Add(key, value = new TDerived());
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) {
            return dict.TryAdd(key, value) ? value : dict[key];
        }

        public static TValue? GetOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : class {
            return dict.TryGetValue(key, out var value) ? value : null;
        }
        
        public static TValue? GetNullable<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : struct {
            return dict.TryGetValue(key, out var value) ? value : default;
        }
    }
}