// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

#nullable enable

namespace MaTech.Common.Data {
    public partial class MetaTable<TEnum> {
        public interface ITableContext {
            MetaTable<TEnum1>? GetTable<TEnum1>() where TEnum1 : struct, Enum, IConvertible;
            MetaTable<TEnum1> EnsureTable<TEnum1>() where TEnum1 : struct, Enum, IConvertible;
        }

        public readonly struct Root : ITableContext {
            private readonly MetaTable<TEnum> table;
            internal Root(MetaTable<TEnum> table) { this.table = table; }

            MetaTable<TEnum1>? ITableContext.GetTable<TEnum1>() => table as MetaTable<TEnum1>;
            MetaTable<TEnum1> ITableContext.EnsureTable<TEnum1>() => table as MetaTable<TEnum1>
                ?? throw new InvalidOperationException($"[MetaTable] Cannot ensure a MetaTable of different enum type ({typeof(TEnum1)}) for Root; Root already exists with a fixed-typed enum ({typeof(TEnum)}).");
        }
        
        public partial struct Selector<TContext, TEnum0> : ITableContext where TContext : struct, ITableContext where TEnum0 : struct, Enum, IConvertible {
            private TContext context; // we have to make this field mutable, otherwise C# creates a defensive copy on calling ITableContext. https://github.com/dotnet/runtime/issues/76355
            private readonly EnumEx<TEnum0> key;
            internal Selector(TContext context, EnumEx<TEnum0> key) { this.context = context; this.key = key; }

            MetaTable<TEnum1>? ITableContext.GetTable<TEnum1>() => context.GetTable<TEnum0>()?.Get<MetaTable<TEnum1>>(key);
            MetaTable<TEnum1> ITableContext.EnsureTable<TEnum1>() => context.EnsureTable<TEnum0>().GetOrSet(key, new MetaTable<TEnum1>());
        }
        
        private Selector<Root, TEnum> CreateRootSelector(TEnum key) => new Selector<Root, TEnum>(new Root(this), (EnumEx<TEnum>)key);
        private static Selector<TNested, TEnum0> CreateNestedSelector<TNested, TEnum0>(TNested nested, TEnum0 key) where TNested : struct, ITableContext where TEnum0 : struct, Enum, IConvertible
            => new Selector<TNested, TEnum0>(nested, key);
    }
}