// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;

#nullable enable

namespace MaTech.Common.Data {
    public readonly partial struct EnumEx<T> where T : struct, Enum, IConvertible {
        public T Value { get; }
        
        public static implicit operator T(EnumEx<T> x) => x.Value;
        public static implicit operator EnumEx<T>(T x) => new EnumEx<T>(x);
        
        public EnumEx(T x) { Value = x; }
        public EnumEx(string name) {
            using (var lockRAII = ReaderLockRAII.Read()) {
                if (mapNameToEnum.TryGetValue(name, out var x)) {
                    Value = x;
                    return;
                }
            }
            using (var lockRAII = WriterLockRAII.Read()) {
                if (mapNameToEnum.TryGetValue(name, out var x)) { // check again for race condition
                    Value = x;
                } else {
                    lockRAII.Write();
                    Value = BoxlessConvert.To<T>.From(++maxEnumIndex);
                    mapNameToEnum.Add(name, Value);
                    mapEnumToName.Add(Value, name);
                }
            }
        }
    }
}