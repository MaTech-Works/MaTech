// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Algorithm {
    /// 数据驱动的Enum类，仅比较string的hashcode，需要避免大规模长字符串上的使用，以避免碰撞。
    /// 需要容忍hash碰撞的场合请使用StringKey类。
    public struct StringEnum : IEquatable<StringEnum> {
        public string Name { get; }
        public int Hash { get; }

        public StringEnum(string name) {
            Name = name;
            Hash = name.GetHashCode();
        }

        public static implicit operator StringEnum(string name) => new StringEnum(name);
        public static explicit operator string(StringEnum se) => se.Name;
        public static explicit operator int(StringEnum se) => se.Hash;

        public bool Equals(StringEnum other) => Hash == other.Hash;
        public override bool Equals(object obj) => obj is StringEnum other && Equals(other);

        public override int GetHashCode() => Hash;

        public static bool operator==(StringEnum left, StringEnum right) => left.Equals(right);
        public static bool operator!=(StringEnum left, StringEnum right) => !left.Equals(right);
    }
}