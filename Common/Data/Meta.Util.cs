// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Data {
    public static partial class Meta {
        private struct ShallowCopyVisitor<TKey> : IMetaVisitableMethods<TKey>.IVisitor where TKey : unmanaged {
            public IMetaTableMethods<TKey> target;
            public Variant Visit(in TKey key, in Variant value) {
                target.Set(key, value);
                return value;
            }
        }
        
        private struct DeepCopyVisitor<TKey, TTable> : IMetaVisitableMethods<TKey>.IVisitor where TKey : unmanaged where TTable : IMetaTableMethods<TKey>, new() {
            public IMetaTableMethods<TKey> target;
            public int depth;
            public Variant Visit(in TKey key, in Variant value) {
                var nested = value.As<IMetaVisitableMethods<TKey>>();
                if (nested != null) {
                    if (depth <= 1) return value;
                    var visitor = new DeepCopyVisitor<TKey, TTable> { target = new TTable(), depth = depth - 1 };
                    nested.Visit(ref visitor);
                    target.Set(key, Variant.From(visitor.target));
                } else {
                    target.Set(key, value);
                }
                return value;
            }
        }
        public static void DeepCopy<TEnum>(IMetaVisitable<TEnum> from, IMetaTable<TEnum> to, int maxDepth = 100) where TEnum : unmanaged, Enum, IConvertible {
            var visitor = new DeepCopyVisitor<DataEnum<TEnum>, MetaTable<TEnum>> { target = to, depth = maxDepth };
            from.Visit(ref visitor);
        }
        
        public static void ShallowCopy(IMetaVisitable from, IMetaTable to) {
            var visitor = new ShallowCopyVisitor<MetaEnum> { target = to };
            from.Visit(ref visitor);
        }
        public static void ShallowCopy<TEnum>(IMetaVisitable<TEnum> from, IMetaTable<TEnum> to) where TEnum : unmanaged, Enum, IConvertible {
            var visitor = new ShallowCopyVisitor<DataEnum<TEnum>> { target = to };
            from.Visit(ref visitor);
        }
        
        public static void DeepCopy(IMetaVisitable from, IMetaTable to, int maxDepth = 100) {
            var visitor = new DeepCopyVisitor<MetaEnum, MetaTable> { target = to, depth = maxDepth };
            from.Visit(ref visitor);
        }
    }
}