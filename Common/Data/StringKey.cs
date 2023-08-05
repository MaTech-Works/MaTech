// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Data {
    /// <summary> 缓存了hashcode的string包装，会优先使用hashcode判断相等。 </summary>
    public struct StringKey : IEquatable<StringKey> {
        public string Name { get; }
        public int Hash { get; }

        public StringKey(string name) {
            Name = name;
            Hash = name.GetHashCode();
        }

        public static implicit operator StringKey(string name) => new StringKey(name);
        public static explicit operator string(StringKey se) => se.Name;
        public static explicit operator int(StringKey se) => se.Hash;

        public bool Equals(StringKey other) => Hash == other.Hash && Name == other.Name;
        public override bool Equals(object obj) => obj is StringKey other && Equals(other);

        public override int GetHashCode() => Hash;

        public static bool operator==(StringKey left, StringKey right) => left.Equals(right);
        public static bool operator!=(StringKey left, StringKey right) => !left.Equals(right);
    }
}