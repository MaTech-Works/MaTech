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
    [Serializable]
    public partial struct MetaEnum {
        [field: SerializeField]
        public uint ID { get; private set; }
        [field: SerializeField]
        public int Value { get; private set; }

        public readonly bool IsEmpty => ID == 0;

        public readonly bool Is<T>() where T : unmanaged, Enum, IConvertible => GetEnumID<T>() == ID;
        public readonly DataEnum<T>? As<T>() where T : unmanaged, Enum, IConvertible => Is<T>() ? new DataEnum<T>(Value) : null;
        public readonly DataEnum<T> UncheckedCastTo<T>() where T : unmanaged, Enum, IConvertible => new DataEnum<T>(Value);

        public static MetaEnum FromEnum<T>(T x) where T : unmanaged, Enum, IConvertible => new(GetEnumID<T>(), BoxlessConvert.To<int>.From(x));
        public static MetaEnum FromEnum<T>(DataEnum<T> x) where T : unmanaged, Enum, IConvertible => FromEnum(x.Value);
        public static MetaEnum FromValue(string name, int value) => new(GetEnumID(name), value);

        public static MetaEnum Empty => new MetaEnum();

        private MetaEnum(uint id, int value) {
            ID = id;
            Value = value;
        }
    }
}