// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// note: 这里没有用#nullable enable，避免难以区分defaultable和Nullable<T>
#nullable enable

using System;
using System.Collections.Generic;

namespace MaTech.Common.Data {
    public partial class MetaTableGeneric<TEnum> where TEnum : unmanaged, Enum, IConvertible {
        public bool Has<T>(TEnum key) => ValueDictOf<T>(key).Has(key);
        public bool Remove<T>(TEnum key) => ValueDictOf<T>(key).Remove(key);
        public T? Get<T>(TEnum key) => ValueDictOf<T>(key).TryGet(key, out var result) ? result : default;
        public T? GetNullable<T>(TEnum key) where T : struct => ValueDictOf<T>(key).TryGet(key, out var result) ? result : null;
        public T GetOrSet<T>(TEnum key, in T value) => ValueDictOf<T>(key, value).GetOrSet(key, value);
        public T Set<T>(TEnum key, in T value) => ValueDictOf<T>(key, value).Set(key, value);

        public bool Has<T>(DataEnum<TEnum> key) => ValueDictOf<T>(key).Has(key);
        public bool Remove<T>(DataEnum<TEnum> key) => ValueDictOf<T>(key).Remove(key);
        public T? Get<T>(DataEnum<TEnum> key) => ValueDictOf<T>(key).TryGet(key, out var result) ? result : default;
        public T? GetNullable<T>(DataEnum<TEnum> key) where T : struct => ValueDictOf<T>(key).TryGet(key, out var result) ? result : null;
        public T GetOrSet<T>(DataEnum<TEnum> key, in T value) => ValueDictOf<T>(key, value).GetOrSet(key, value);
        public T Set<T>(DataEnum<TEnum> key, in T value) => ValueDictOf<T>(key, value).Set(key, value);
        
        public bool Has<T>(string keyName) => Has<T>(new DataEnum<TEnum>(keyName, ordered: false)); 
        public bool Remove<T>(string keyName) => Remove<T>(new DataEnum<TEnum>(keyName, ordered: false)); 
        public T? Get<T>(string keyName) => Get<T>(new DataEnum<TEnum>(keyName, ordered: false)); 
        public T? GetNullable<T>(string keyName) where T : struct => GetNullable<T>(new DataEnum<TEnum>(keyName, ordered: false)); 
        public T GetOrSet<T>(string keyName, in T value) => GetOrSet<T>(new DataEnum<TEnum>(keyName, ordered: false), value); 
        public T Set<T>(string keyName, in T value) => Set<T>(new DataEnum<TEnum>(keyName, ordered: false), value); 
        
        public int Collect<T>(ICollection<T> outList) => ValueDictOf<T>().Collect(outList);

        public Selector<Root, TEnum> Select(TEnum key) => CreateRootSelector(key);
        public Selector<Root, TEnum> Select(DataEnum<TEnum> key) => CreateRootSelector(key.Value);
        public Selector<Root, TEnum> Select(string keyName) => CreateRootSelector(new DataEnum<TEnum>(keyName, ordered: false).Value);
        
        public partial struct Selector<TContext, TEnum0> {
            public bool IsValid() => context.GetTable<TEnum0>() != null;
            
            public MetaTableGeneric<TEnum0>? GetContextTable() => context.GetTable<TEnum0>();
            
            #nullable enable
            public MetaTableGeneric<TEnum0> EnsureContextTable() => context.EnsureTable<TEnum0>();
            #nullable restore
            
            public bool Has<T>() => GetContextTable()?.Has<T>(key) ?? false;
            public bool Remove<T>() => GetContextTable()?.Remove<T>(key) ?? false;
            
            public T Get<T>() => GetContextTable() is { } table ? table.Get<T>(key) : default; // cannot do GetContextTable()?.Get<T>() without C# 9.0
            public T? GetNullable<T>() where T : struct => GetContextTable()?.GetNullable<T>(key);
            public T GetOrSet<T>(in T value) => EnsureContextTable().GetOrSet<T>(key, value);
            public T Set<T>(in T value) => EnsureContextTable().Set<T>(key, value);
            
            public int Collect<T>(ICollection<T> outList) => GetContextTable() is { } table ? table.Collect<T>(outList) : 0; // cannot do GetContextTable()?.Collect<T>(outList) without C# 9.0
            
            public Selector<Selector<TContext, TEnum0>, TEnum1> Select<TEnum1>(TEnum1 key1) where TEnum1 : unmanaged, Enum, IConvertible => CreateNestedSelector(this, key1);
            public Selector<Selector<TContext, TEnum0>, TEnum1> Select<TEnum1>(DataEnum<TEnum1> key1) where TEnum1 : unmanaged, Enum, IConvertible => CreateNestedSelector(this, key1.Value);
            public Selector<Selector<TContext, TEnum0>, TEnum1> Select<TEnum1>(string keyName1) where TEnum1 : unmanaged, Enum, IConvertible => CreateNestedSelector(this, new DataEnum<TEnum1>(keyName1, ordered: false).Value);
        }
    }
}