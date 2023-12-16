// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;

namespace MaTech.Common.Data {
    public static partial class Meta {
        public static WrappedMetaToEnum<TEnum> Wrap<TEnum>(IMeta meta) where TEnum : unmanaged, Enum, IConvertible => new(meta);
        public static WrappedMetaFromEnum<TEnum> Wrap<TEnum>(IMeta<TEnum> meta) where TEnum : unmanaged, Enum, IConvertible => new(meta);
        public static WrappedMetaTableToEnum<TEnum> Wrap<TEnum>(IMetaTable meta) where TEnum : unmanaged, Enum, IConvertible => new(meta);
        public static WrappedMetaTableFromEnum<TEnum> Wrap<TEnum>(IMetaTable<TEnum> meta) where TEnum : unmanaged, Enum, IConvertible => new(meta);
        public static WrappedMetaVisitableToEnum<TEnum> Wrap<TEnum>(IMetaVisitable meta) where TEnum : unmanaged, Enum, IConvertible => new(meta);
        public static WrappedMetaVisitableFromEnum<TEnum> Wrap<TEnum>(IMetaVisitable<TEnum> meta) where TEnum : unmanaged, Enum, IConvertible => new(meta);

        public readonly struct WrappedMetaToEnum<TEnum> : IMeta<TEnum> where TEnum : unmanaged, Enum, IConvertible {
            public bool Has(in EnumEx<TEnum> key) => target.Has(MetaEnum.FromEnum(key));
            public Variant Get(in EnumEx<TEnum> key) => target.Get(MetaEnum.FromEnum(key));
            
            public WrappedMetaToEnum(IMeta target) {
                this.target = target;
            }

            private readonly IMeta target;
        }
        
        public readonly struct WrappedMetaFromEnum<TEnum> : IMeta where TEnum : unmanaged, Enum, IConvertible {
            public bool Has(in MetaEnum key) => key.Is<TEnum>() && target.Has(key.UncheckedCastTo<TEnum>());
            public Variant Get(in MetaEnum key) => key.Is<TEnum>() ? target.Get(key.UncheckedCastTo<TEnum>()) : Variant.None;
            
            public WrappedMetaFromEnum(IMeta<TEnum> target) {
                this.target = target;
            }

            private readonly IMeta<TEnum> target;
        }

        public readonly struct WrappedMetaTableToEnum<TEnum> : IMetaTable<TEnum> where TEnum : unmanaged, Enum, IConvertible {
            public bool Has(in EnumEx<TEnum> key) => target.Has(MetaEnum.FromEnum(key));
            public Variant Get(in EnumEx<TEnum> key) => target.Get(MetaEnum.FromEnum(key));
            public bool Remove(in EnumEx<TEnum> key) => target.Remove(MetaEnum.FromEnum(key));
            public bool TrySet(in EnumEx<TEnum> key, in Variant value, bool overwrite) => target.TrySet(MetaEnum.FromEnum(key), value, overwrite);
            
            public WrappedMetaTableToEnum(IMetaTable target) {
                this.target = target;
            }

            private readonly IMetaTable target;
        }
        
        public readonly struct WrappedMetaTableFromEnum<TEnum> : IMetaTable where TEnum : unmanaged, Enum, IConvertible {
            public bool Has(in MetaEnum key) => key.Is<TEnum>() && target.Has(key.UncheckedCastTo<TEnum>());
            public Variant Get(in MetaEnum key) => key.Is<TEnum>() ? target.Get(key.UncheckedCastTo<TEnum>()) : Variant.None;
            public bool Remove(in MetaEnum key) => key.Is<TEnum>() && target.Remove(key.UncheckedCastTo<TEnum>());
            public bool TrySet(in MetaEnum key, in Variant value, bool overwrite) => key.Is<TEnum>() && target.TrySet(key.UncheckedCastTo<TEnum>(), value, overwrite);
            
            public WrappedMetaTableFromEnum(IMetaTable<TEnum> target) {
                this.target = target;
            }

            private readonly IMetaTable<TEnum> target;
        }

        
        public readonly struct WrappedMetaVisitableToEnum<TEnum> : IMetaVisitable<TEnum> where TEnum : unmanaged, Enum, IConvertible {
            public WrappedMetaVisitableToEnum(IMetaVisitable target) => this.target = target;
            private readonly IMetaVisitable target;
            
            public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IMetaVisitable<TEnum>.IVisitor {
                var wrapper = new Wrapper<TVisitor>(ref visitor);
                target.Visit(ref wrapper);
                visitor = wrapper.visitor; // in case of struct
            }
        
            private readonly struct Wrapper<TVisitor> : IMetaVisitable.IVisitor where TVisitor : IMetaVisitable<TEnum>.IVisitor {
                public readonly TVisitor visitor;
                public Wrapper(ref TVisitor visitor) => this.visitor = visitor;
                public Variant Visit(in MetaEnum key, in Variant value) => key.Is<TEnum>() ? visitor.Visit(key.UncheckedCastTo<TEnum>(), in value) : Variant.None;
            }
        }

        public readonly struct WrappedMetaVisitableFromEnum<TEnum> : IMetaVisitable where TEnum : unmanaged, Enum, IConvertible {
            public WrappedMetaVisitableFromEnum(IMetaVisitable<TEnum> target) => this.target = target;
            private readonly IMetaVisitable<TEnum> target;
            
            public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IMetaVisitable.IVisitor {
                var wrapper = new Wrapper<TVisitor>(ref visitor);
                target.Visit(ref wrapper);
                visitor = wrapper.visitor; // in case of struct
            }
        
            private readonly struct Wrapper<TVisitor> : IMetaVisitable<TEnum>.IVisitor where TVisitor : IMetaVisitable.IVisitor {
                public readonly TVisitor visitor;
                public Wrapper(ref TVisitor visitor) => this.visitor = visitor;
                public Variant Visit(in EnumEx<TEnum> key, in Variant value) => visitor.Visit(MetaEnum.FromEnum(key), in value);
            }
        }
    }
}