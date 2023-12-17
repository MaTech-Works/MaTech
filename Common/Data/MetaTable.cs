// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

namespace MaTech.Common.Data {
    // TODO: 把MetaTableGeneric中实现的Selector机制移植到Meta扩展类里
    // TODO: 使用visit机制实现浅复制与深复制
    // TODO: 针对ISerializable接口封装MetaVisitor wrapper和反向wrapper
    // TODO: 实现用标签与反射驱动、有成员信息缓存的序列化与Meta wrapper

    public class MetaTable : IMetaTable, IMetaVisitable {
        private readonly Dictionary<MetaEnum, Variant> dict = new();
        private readonly List<(MetaEnum key, Variant value)> pendingChanges = new();

        public bool Has(in MetaEnum key) => dict.ContainsKey(key);
        public Variant Get(in MetaEnum key) => dict.TryGetValue(key, out var value) ? value : Variant.None;
        public bool Remove(in MetaEnum key) => dict.Remove(key);
        public bool TrySet(in MetaEnum key, in Variant value, bool overwrite = true) => dict.TrySetMeta(key, value, overwrite);
        
        public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IMetaVisitable.IVisitor {
            foreach (var kv in dict) {
                var newValue = visitor.Visit(kv.Key, kv.Value);
                if (newValue != kv.Value) {
                    pendingChanges.Add((kv.Key, newValue));
                }
            }
            foreach ((MetaEnum key, Variant value) in pendingChanges) {
                dict.TrySetMeta(key, value);
            }
        }
    }
    
    public class MetaTable<TEnum> : IMetaTable<TEnum>, IMetaVisitable<TEnum> where TEnum : unmanaged, Enum, IConvertible {
        private readonly Dictionary<DataEnum<TEnum>, Variant> dict = new();
        private readonly List<(DataEnum<TEnum> key, Variant value)> pendingChanges = new();

        public bool Has(in DataEnum<TEnum> key) => dict.ContainsKey(key);
        public Variant Get(in DataEnum<TEnum> key) => dict.TryGetValue(key, out var value) ? value : Variant.None;
        public bool Remove(in DataEnum<TEnum> key) => dict.Remove(key);
        public bool TrySet(in DataEnum<TEnum> key, in Variant value, bool overwrite = true) => dict.TrySetMeta(key, value, overwrite);
        
        public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IMetaVisitable<TEnum>.IVisitor {
            foreach (var kv in dict) {
                var newValue = visitor.Visit(kv.Key, kv.Value);
                if (newValue != kv.Value) {
                    pendingChanges.Add((kv.Key, newValue));
                }
            }
            foreach ((DataEnum<TEnum> key, Variant value) in pendingChanges) {
                dict.TrySetMeta(key, value);
            }
        }
    }

    internal static class MetaTableOperation {
        public static bool TrySetMeta<TKey>(this Dictionary<TKey, Variant> dict, in TKey key, in Variant value, bool overwrite = true) where TKey : unmanaged {
            if (overwrite) {
                if (value.IsNone) {
                    return dict.Remove(key);
                } else {
                    dict[key] = value;
                    return true;
                }
            } else {
                if (value.IsNone) {
                    return false;
                } else {
                    return dict.TryAdd(key, value);
                }
            }
        }
    }
}