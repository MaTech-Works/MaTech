// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using Standart.Hash.xxHash;
using UnityEngine;

namespace MaTech.Common.Data {
    /// <summary>
    /// 支持运行时增加条目、无boxing的整型转型与hashcode、缓存了条目名称的enum扩展数据类型。
    /// </summary>
    /// <example>
    /// 因为EnumEx与enum一样使用自增的整型值，推荐使用这种格式来扩展EnumEx，让序列化的结果保持一致：
    /// <code>
    /// public enum Foo {}
    /// public static class ExtraFoo {
    ///     public static EnumEx&lt;Foo&gt; Bar1 { get; private set; }
    ///     public static EnumEx&lt;Foo&gt; Bar2 { get; private set; }
    ///     [InitializeEnumExMethod]
    ///     private static void Init() {
    ///         Bar1 = new("Bar1");
    ///         Bar2 = new("Bar2");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TEnum"></typeparam>
    [Serializable]
    public partial struct EnumEx<TEnum> where TEnum : unmanaged, Enum, IConvertible {
        [field: SerializeField]
        public TEnum Value { get; private set; }

        public int UnderlyingValue => BoxlessConvert.To<int>.From(Value);
        
        public static implicit operator TEnum(EnumEx<TEnum> x) => x.Value;
        public static implicit operator EnumEx<TEnum>(TEnum x) => new EnumEx<TEnum>(x);
        public static implicit operator EnumEx<TEnum>(int x) => new EnumEx<TEnum>(x);

        public EnumEx(TEnum x) { Value = x; }
        public EnumEx(int x) { Value = BoxlessConvert.To<TEnum>.From(x); }
        public EnumEx(string name, int index) { Value = DefineEnumWithIndex(name, index); }
        public EnumEx(string name, bool ordered = true) { Value = ordered ? DefineOrderedEnum(name) : DefineUnorderedEnum(name); }
    }

    public static class EnumEx {
        public static int MaxUnorderedHashAttempts { get; set; } = 10;
    }
}