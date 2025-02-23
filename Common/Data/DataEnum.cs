// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using MaTech.Common.Algorithm;
using UnityEngine;

namespace MaTech.Common.Data {
    /// <summary>
    /// 支持运行时增加条目、无 boxing 的整型转型与 hashcode、缓存了条目名称的 enum 扩展数据类型。
    /// </summary>
    /// <example>
    /// DataEnum 与 enum 一样使用自增的整型值，依照 C# 对于 static 项按文字顺序初始化的特性，可以使用这种格式来扩展 DataEnum，让序列化的结果保持一致：
    /// <code>
    /// public enum Foo { First = 573 }
    /// [InitializeDataEnum]
    /// public static class ExtraFoo {
    ///     public static DataEnum&lt;Foo&gt; Hello { get; } = new("Hi"); // 574, actual name is "Hi"
    ///     public static DataEnum&lt;Foo&gt; Lui { get; } = new("Lui", 765);
    ///     public static DataEnum&lt;Foo&gt; Cat { get; } = new("Cat"); // 766
    /// }
    /// </code>
    /// 也可以使用原始 enum 类型：
    /// <code>
    /// public enum Foo { }
    /// [InitializeDataEnum]
    /// public static class ExtraFoo {
    ///     public static Foo Bar { get; } = DataEnum.Ordered&lt;Foo&gt;("Bar"); // 0
    /// }
    /// </code>
    /// 也可以集中处理初始化：
    /// <code>
    /// public enum Foo { }
    /// public static class ExtraFoo {
    ///     public static DataEnum&lt;Foo&gt; Bar1 { get; private set; }
    ///     public static DataEnum&lt;Foo&gt; Bar2 { get; private set; }
    ///     [InitializeDataEnumMethod]
    ///     private static void Init() {
    ///         Bar1 = new("Bar1");
    ///         Bar2 = new("Bar2");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TEnum"></typeparam>
    [Serializable]
    public partial struct DataEnum<TEnum> where TEnum : unmanaged, Enum, IConvertible {
        [field: SerializeField]
        public TEnum Value { get; private set; }

        public readonly int UnderlyingValue => BoxlessConvert.To<int>.From(Value);
        
        public static implicit operator TEnum(DataEnum<TEnum> x) => x.Value;
        public static implicit operator DataEnum<TEnum>(TEnum x) => new(x);
        public static implicit operator DataEnum<TEnum>(int x) => new(x);
        public static implicit operator DataEnum<TEnum>(string name) => DefineUnorderedEnum(name); // unordered; define ordered with new("name") or new("name", index)
        
        public DataEnum(TEnum x) { Value = x; }
        public DataEnum(int x) { Value = BoxlessConvert.To<TEnum>.From(x); }
        public DataEnum(string name, int index) { Value = DefineEnumWithIndex(name, index); }
        public DataEnum(string name, bool ordered = true) { Value = ordered ? DefineOrderedEnum(name) : DefineUnorderedEnum(name); }
    }

    public static class DataEnum {
        public static TEnum WithIndex<TEnum>(string name, int index) where TEnum : unmanaged, Enum, IConvertible => DataEnum<TEnum>.DefineEnumWithIndex(name, index);
        public static TEnum Unordered<TEnum>(string name) where TEnum : unmanaged, Enum, IConvertible => DataEnum<TEnum>.DefineUnorderedEnum(name);
        public static TEnum Ordered<TEnum>(string name) where TEnum : unmanaged, Enum, IConvertible => DataEnum<TEnum>.DefineOrderedEnum(name);
        
        public static int MaxUnorderedHashAttempts { get; set; } = 10;
    }
}