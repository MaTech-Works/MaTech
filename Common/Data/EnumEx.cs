// Copyright (c) 2023, LuiCat (as MaTech)
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
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public partial struct EnumEx<T> where T : unmanaged, Enum, IConvertible {
        [field: SerializeField]
        public T Value { get; private set; }
        
        public static implicit operator T(EnumEx<T> x) => x.Value;
        public static implicit operator EnumEx<T>(T x) => new EnumEx<T>(x);
        public static implicit operator EnumEx<T>(int x) => new EnumEx<T>(BoxlessConvert.To<T>.From(x));

        public EnumEx(T x) { Value = x; }
        public EnumEx(string name, int? enumIndex = null) {
            using (var lockRAII = ReaderLockRAII.EnterRead()) {
                if (mapNameToEnum.TryGetValue(name, out var x)) {
                    Value = x;
                    return;
                }
            }
            using (var lockRAII = WriterLockRAII.EnterRead()) {
                if (mapNameToEnum.TryGetValue(name, out var x)) { // check again for race condition
                    Value = x;
                } else {
                    lockRAII.EnterWrite();
                    maxEnumIndex = enumIndex ?? maxEnumIndex + 1;
                    Value = BoxlessConvert.To<T>.From(maxEnumIndex);
                    mapNameToEnum.Add(name, Value);
                    mapEnumToName.Add(Value, name);
                }
            }
        }
    }
}