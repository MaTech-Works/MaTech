// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

namespace MaTech.Common.Data {
    public partial struct MetaEnum : IEquatable<MetaEnum>, IFormattable {
        // TODO: implement IBoxlessConvertible to EnumEx
        
        public override string ToString() => ToString("X", null);
        public string ToString(string format, IFormatProvider formatProvider) {
            using var lockRAII = ReaderLockRAII.EnterRead(lockMetadata);
            var name = knownNamesByID.GetValueOrDefault(ID, "Unknown");
            return $"{name}.{Value.ToString(format, formatProvider)}";
        }
        
        public override int GetHashCode() => IsEmpty ? 0 : HashCode.Combine(ID, Value);
        
        public bool Equals(MetaEnum other) => (IsEmpty && other.IsEmpty) || (ID == other.ID && Value == other.Value);
        public override bool Equals(object obj) => obj is MetaEnum other && Equals(other);
        public static bool operator==(MetaEnum left, MetaEnum right) => left.Equals(right);
        public static bool operator!=(MetaEnum left, MetaEnum right) => !left.Equals(right);
    }
}