// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;
#nullable enable

namespace MaTech.Common.Data {
    public partial class MetaTable<TEnum> where TEnum : struct, Enum, IConvertible {
        public bool Has<T>(EnumEx<TEnum> key) => dictByType.GetOrEmpty<T>().ContainsKey(key);
        public bool Remove<T>(EnumEx<TEnum> key) => dictByType.GetOrEmpty<T>().Remove(key);
        
        public T? Get<T>(EnumEx<TEnum> key) => dictByType.GetOrEmpty<T>().GetNullable(key);
        public T Set<T>(EnumEx<TEnum> key, in T value) => dictByType.GetOrCreate<T>()[key] = value;
        public T GetOrSet<T>(EnumEx<TEnum> key, in T value) => dictByType.GetOrCreate<T>().GetOrAdd(key, value);

        public int Collect<T>(ICollection<T> outList) {
            var table = dictByType.GetOrEmpty<T>();
            if (table.Count == 0) return 0;
            foreach (var value in table.Values) {
                outList.Add(value);
            }
            return table.Count;
        }
        
        public Selector<Root, TEnum> Select(TEnum key) => CreateRootSelector(key);
        
        public partial struct Selector<TContext, TEnum0> {
            public bool IsValid() => context.GetTable<TEnum0>() != null;
            
            public MetaTable<TEnum0>? GetContextTable() => context.GetTable<TEnum0>();
            public MetaTable<TEnum0> EnsureContextTable() => context.EnsureTable<TEnum0>();
            
            public bool Has<T>() => GetContextTable()?.Has<T>(key) ?? false;
            public bool Remove<T>() => GetContextTable()?.Remove<T>(key) ?? false;
            
            public T? Get<T>() => GetContextTable() is MetaTable<TEnum0> table ? table.Get<T>(key) : default; // cannot write GetContextTable()?.Get<T>(key), not targeting C# 9.0
            public T Set<T>(in T value) => EnsureContextTable().Set<T>(key, value);
            public T GetOrSet<T>(in T value) => EnsureContextTable().GetOrSet<T>(key, value);
            
            public int Collect<T>(ICollection<T> outList) => GetContextTable() is MetaTable<TEnum0> table ? table.Collect<T>(outList) : 0; // cannot write GetContextTable()?.Collect<T>(outList), not targeting C# 9.0
            
            public Selector<Selector<TContext, TEnum0>, TEnum1> Select<TEnum1>(TEnum1 key1) where TEnum1 : struct, Enum, IConvertible
                => CreateNestedSelector(this, key1);
        }
    }
    
    // ======== Examples ========
    
    #if UNITY_EDITOR
    internal struct MetaTableExample {
        public enum Foo { Bar }
        public enum Lui { Cat }
        
        public void Example() {
            var table = new MetaTable<Foo>();
            var list = new List<int>();
        
            Foo bar0 = Foo.Bar;
            var bar1 = new EnumEx<Foo>(bar0); // prints as "Bar", underlying enum value is 0 (Foo.Bar)
            var bar2 = new EnumEx<Foo>("Bar"); // prints as "Bar", underlying enum value is 0 (Foo.Bar)
            var baz = new EnumEx<Foo>("Baz"); // prints as "Baz", underlying enum value is 1
            Foo qux = new EnumEx<Foo>("Qux"); // prints as 2; new EnumEx<Foo>(qux) prints as "Qux"

            table.Set(bar0, 765);
            table.Set(bar1, 5.73f);
            table.Select(bar2).Set("test");

            table.Has<int>(Foo.Bar); // --> true
            table.Has<float>(Foo.Bar); // --> true
            table.Has<string>(Foo.Bar); // --> true
            table.Has<double>(Foo.Bar); // --> false
            table.Select(Foo.Bar).Has<int>(); // --> true
            table.Select(Foo.Bar).Has<float>(); // --> true
            table.Select(Foo.Bar).Has<string>(); // --> true
            table.Select(Foo.Bar).Has<double>(); // --> false

            table.Get<int>(Foo.Bar); // --> 765
            table.Get<float>(Foo.Bar); // --> 5.73f
            table.Get<string>(Foo.Bar); // --> "test"
            table.Get<double>(Foo.Bar); // --> null (as Nullable<double>)

            table.GetOrSet(baz, 1); // --> 1
            table.GetOrSet(baz, 2); // --> 1
            table.Get<int>(baz); // --> 1
            table.Remove<int>(baz);
            table.GetOrSet(baz, 3); // --> 3
            table.Get<int>(baz); // --> 3
            table.Set(baz, 4); // --> 4
            table.Get<int>(baz); // --> 4

            bool useSelectMethod = true;
            bool saveSelector = true;
            if (useSelectMethod) {
                if (saveSelector) {
                    var selector1 = table.Select(qux).Select(Lui.Cat);
                    var selector2 = table.Select(qux).Select(Foo.Bar);
                    selector1.EnsureContextTable();
                    selector1.IsValid(); // --> true
                    selector2.Set(Lui.Cat);
                    selector2.Get<Lui>(); // --> Cat
                } else {
                    table.Select(qux).Select(Lui.Cat).EnsureContextTable();
                    table.Select(qux).Select(Lui.Cat).IsValid(); // --> true
                    table.Select(qux).Select(Foo.Bar).Set(Lui.Cat);
                    table.Select(qux).Select(Foo.Bar).Get<Lui>(); // --> Cat
                }
            } else {
                table.Set(qux, new MetaTable<Lui>());
                table.Has<MetaTable<Lui>>(qux); // --> true
                table.Set(qux, new MetaTable<Foo>()).Set(Foo.Bar, Lui.Cat);
                table.Get<MetaTable<Foo>>(qux)!.Get<Lui>(Foo.Bar); // --> Cat
            }
        }
    }
    #endif
}