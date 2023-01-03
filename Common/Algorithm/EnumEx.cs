// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;

namespace MaTech.Common.Algorithm {
    public readonly partial struct EnumEx<T> where T : struct, Enum, IConvertible {
        public T Value { get; }
        public EnumEx(T x) { Value = x; }

        public override string ToString() => mapEnumToName.GetValueOrDefault(Value, Value.ToString());
        
        public static implicit operator T(EnumEx<T> x) => x.Value;
        public static implicit operator EnumEx<T>(T x) => new(x);

        static EnumEx() {
            foreach (var value in (T[])Enum.GetValues(typeEnum)) {
                var name = Enum.GetName(typeEnum, value);
                if (string.IsNullOrEmpty(name))
                    continue;
                mapNameToEnum.Add(name, value);
                mapEnumToName.Add(value, name);
                maxEnumIndex = Math.Max(maxEnumIndex, value.ToUInt64(null)); // todo: boxless conversion?
            }
        }
        
        public EnumEx(string name) {
            using (var lockRAII = LockRAII.UpgradeableReadLock()) {
                if (mapNameToEnum.TryGetValue(name, out var x)) {
                    Value = x;
                } else {
                    lockRAII.UpgradeToWriteLock();
                    Value = BoxlessConvert.To<T>.From(++maxEnumIndex);
                    mapNameToEnum.Add(name, Value);
                    mapEnumToName.Add(Value, name);
                }
            }
        }
    }
}