// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using UnityEngine;
using UnityEngine.Scripting;

namespace MaTech.Common.Data {
    public partial struct EnumEx<T> {
        private static readonly Type typeEnum = typeof(T);
        
        private static readonly HashSet<T> predefinedEnums;
        private static readonly Dictionary<string, T> mapNameToEnum;
        private static readonly Dictionary<T, string> mapEnumToName;

        private static int maxEnumIndex;

        public static int GetValues(ICollection<T> outValues) {
            using var lockRAII = ReaderLockRAII.EnterRead();
            foreach (var value in mapNameToEnum.Values)
                outValues.Add(value);
            return mapNameToEnum.Count;
        }
        
        public static int GetValues(ICollection<EnumEx<T>> outValues) {
            using var lockRAII = ReaderLockRAII.EnterRead();
            foreach (var value in mapNameToEnum.Values)
                outValues.Add(value);
            return mapNameToEnum.Count;
        }
        
        public static int GetNames(ICollection<string> outNames) {
            using var lockRAII = ReaderLockRAII.EnterRead();
            foreach (var name in mapNameToEnum.Keys)
                outNames.Add(name!);
            return mapNameToEnum.Count;
        }

        static EnumEx() {
            var enumType = typeof(T);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            if (underlyingType != typeof(int)) {
                Debug.LogWarning($"[EnumEx] Partial support for underlying type [{underlyingType}] of enum type [{enumType}], value auto-increment are done in Int32 range and might break on integer overflow.");
            }
            
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

                maxEnumIndex = Math.Max(maxEnumIndex, BoxlessConvert.To<int>.From(value));
            }
        }

        private static string[]? cachedRegisteredNames;
        
        // DO NOT MODIFY OR REMOVE THIS METHOD unless you know what you are doing.
        // Has reflection usage in EnumExDrawer.
        [Preserve]
        public static string[] GetRegisteredNames() {
            if (cachedRegisteredNames == null || cachedRegisteredNames.Length != mapNameToEnum.Count) {
                cachedRegisteredNames = mapNameToEnum.Keys.ToArray();
            }
            return cachedRegisteredNames;
        }

        // DO NOT MODIFY OR REMOVE THIS METHOD unless you know what you are doing.
        [Preserve]
        public static void PreserveStub_DoNotCall() {
            BoxlessConvert.PreserveForEnum<T>();
            GetRegisteredNames();
        }
    }
}