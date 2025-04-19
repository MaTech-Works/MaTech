// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Data {
    public partial struct Variant {
        private Variant(Scalar value, Type type) {
            s = value;
            o = type;
        }
        private Variant(object value) {
            s = Scalar.Invalid;
            o = value;
        }

        internal static readonly Type typeNone = null;
        internal static readonly Type typeBool = typeof(bool);
        internal static readonly Type typeInt = typeof(int);
        internal static readonly Type typeFloat = typeof(float);
        internal static readonly Type typeDouble = typeof(double);
        internal static readonly Type typeScalar = typeof(Scalar);
        internal static readonly Type typeEnum = typeof(MetaEnum);
        internal static readonly Type typeMixed = typeof(FractionMixed);
        internal static readonly Type typeImproper = typeof(FractionImproper);
        
        private static readonly HashSet<Type> typesConvertible = new HashSet<Type>() {
            typeof(bool),
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeMixed,
            typeImproper,
        };
    }
}