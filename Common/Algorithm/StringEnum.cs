// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Algorithm {
    public struct StringEnum : IEquatable<StringEnum> {
        public string Name { get; }
        public int Hash { get; }

        public StringEnum(string name) {
            Name = name;
            Hash = name.GetHashCode();
        }

        public StringEnum(int value) {
            Name = null;
            Hash = value;
        }

        public static implicit operator StringEnum(string name) => new StringEnum(name);
        public static implicit operator StringEnum(int hash) => new StringEnum(hash);
        public static implicit operator string(StringEnum se) => se.Name;
        public static implicit operator int(StringEnum se) => se.Hash;

        public bool Equals(StringEnum other) => Hash == other.Hash;
        public override bool Equals(object obj) => obj is StringEnum other && Equals(other);

        public override int GetHashCode() => Hash;

        public static bool operator==(StringEnum left, StringEnum right) => left.Equals(right);
        public static bool operator!=(StringEnum left, StringEnum right) => !left.Equals(right);
    }
}