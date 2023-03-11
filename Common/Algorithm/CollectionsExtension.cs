// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace MaTech.Common.Algorithm {
    public static class CollectionsExtension {
        public static void OrderedInsert<T>(this List<T> list, T item, bool afterEqual = false) where T : IComparable<T> {
            int index = list.BinarySearch(item);
            if (index < 0) index = ~index;
            else if (afterEqual) {
                for (int n = list.Count; index < n && list[index].CompareTo(item) == 0; ++index) { }
            }
            list.Insert(index, item);
        }

        public static void Resize<T>(this List<T?> list, int size) {
            // note: by language specification, T? means defaultable rather than nullable, since the generic T is unconstrained
            if (size < list.Count) list.RemoveRange(size, list.Count - size);
            else while (size > list.Count) {
                list.Add(default);
            }
        }

        public static void RemoveAndFillWithLast<T>(this List<T> list, int index) {
            int indexLast = list.Count - 1;
            list[index] = list[indexLast];
            list.RemoveAt(indexLast);
        }

        public static void RemoveAllAndFillWithLast<T>(this List<T> list, Func<T, bool> conditionRemove) {
            for (int i = 0, n = list.Count; i < n;) {
                if (conditionRemove(list[i])) {
                    list.RemoveAndFillWithLast(i);
                    --n;
                } else {
                    ++i;
                }
            }
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

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) {
            return dict.TryAdd(key, value) ? value : dict[key];
        }

        public static TValue? GetNullable<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) {
            return dict.TryGetValue(key, out var value) ? value : default;
        }
    }
}