// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Data {
    public partial struct MetaEnum : IEquatable<MetaEnum>, IFormattable {
        // TODO: implement IBoxlessConvertible to DataEnum
        
        public override string ToString() => ToString("X", null);
        public string ToString(string format, IFormatProvider formatProvider) {
            return $"{Name}.{Value.ToString(format, formatProvider)}";
        }
        
        public override int GetHashCode() => IsEmpty ? 0 : HashCode.Combine(ID, Value);
        
        public bool Equals(MetaEnum other) => (IsEmpty && other.IsEmpty) || (ID == other.ID && Value == other.Value);
        public override bool Equals(object obj) => obj is MetaEnum other && Equals(other);
        public static bool operator==(MetaEnum left, MetaEnum right) => left.Equals(right);
        public static bool operator!=(MetaEnum left, MetaEnum right) => !left.Equals(right);
    }
}