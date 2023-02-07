// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

#nullable enable

namespace MaTech.Common.Data {
    public readonly partial struct EnumEx<T> {
        private static readonly Type typeEnum = typeof(T);
        
        private static readonly HashSet<T> predefinedEnums;
        private static readonly Dictionary<string, T> mapNameToEnum;
        private static readonly Dictionary<T, string> mapEnumToName;

        private static ulong maxEnumIndex;

        static EnumEx() {
            var values = (T[])Enum.GetValues(typeEnum);
            var length = values.Length;
            
            predefinedEnums = new HashSet<T>(length);
            mapNameToEnum = new Dictionary<string, T>(length);
            mapEnumToName = new Dictionary<T, string>(length);

            foreach (var value in values) {
                var name = Enum.GetName(typeEnum, value);
                if (string.IsNullOrEmpty(name))
                    continue;

                predefinedEnums.Add(value);
                mapNameToEnum.Add(name, value);
                mapEnumToName.Add(value, name);

                maxEnumIndex = Math.Max(maxEnumIndex, BoxlessConvert.To<ulong>.From(value));
            }
        }
    }
}