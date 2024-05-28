// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;

namespace MaTech.Common.Data {
    public partial class MetaTableGeneric<TEnum> {
        public interface ITableContext {
            MetaTableGeneric<TEnum1>? GetTable<TEnum1>() where TEnum1 : unmanaged, Enum, IConvertible;
            MetaTableGeneric<TEnum1> EnsureTable<TEnum1>() where TEnum1 : unmanaged, Enum, IConvertible;
        }

        public readonly struct Root : ITableContext {
            private readonly MetaTableGeneric<TEnum> table;
            internal Root(MetaTableGeneric<TEnum> table) { this.table = table; }

            MetaTableGeneric<TEnum1>? ITableContext.GetTable<TEnum1>() => table as MetaTableGeneric<TEnum1>;
            MetaTableGeneric<TEnum1> ITableContext.EnsureTable<TEnum1>() => table as MetaTableGeneric<TEnum1>
                ?? throw new InvalidOperationException($"[MetaTableGeneric] Cannot ensure a MetaTableGeneric of different enum type ({typeof(TEnum1)}) for Root; Root already exists with a fixed-typed enum ({typeof(TEnum)}).");
        }
        
        public partial struct Selector<TContext, TEnum0> : ITableContext where TContext : struct, ITableContext where TEnum0 : unmanaged, Enum, IConvertible {
            private TContext context; // we have to make this field mutable, otherwise C# creates a defensive copy on calling ITableContext. https://github.com/dotnet/runtime/issues/76355
            private readonly DataEnum<TEnum0> key;
            internal Selector(TContext context, DataEnum<TEnum0> key) { this.context = context; this.key = key; }

            MetaTableGeneric<TEnum1>? ITableContext.GetTable<TEnum1>() => context.GetTable<TEnum0>()?.Get<MetaTableGeneric<TEnum1>>(key);
            MetaTableGeneric<TEnum1> ITableContext.EnsureTable<TEnum1>() => context.EnsureTable<TEnum0>().GetOrSet(key, new MetaTableGeneric<TEnum1>());
        }
        
        private Selector<Root, TEnum> CreateRootSelector(TEnum key) => new Selector<Root, TEnum>(new Root(this), (DataEnum<TEnum>)key);
        private static Selector<TNested, TEnum0> CreateNestedSelector<TNested, TEnum0>(TNested nested, TEnum0 key) where TNested : struct, ITableContext where TEnum0 : unmanaged, Enum, IConvertible
            => new Selector<TNested, TEnum0>(nested, key);
    }
}