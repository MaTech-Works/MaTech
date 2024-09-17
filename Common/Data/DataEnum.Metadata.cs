// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using MaTech.Common.Algorithm;
using Standart.Hash.xxHash;
using UnityEngine;
using UnityEngine.Scripting;

// ReSharper disable StaticMemberInGenericType

namespace MaTech.Common.Data {
    public partial struct DataEnum<TEnum> {
        private static readonly ReaderWriterLockSlim lockMetadata = new ReaderWriterLockSlim();
        
        private static readonly Type typeEnum = typeof(TEnum);
        
        private static readonly Dictionary<string, TEnum> mapNameToEnum;
        private static readonly Dictionary<TEnum, string> mapEnumToName;
        
        private static readonly ReadOnlyDictionary<string, TEnum> predefinedNameToEnum;
        private static readonly ReadOnlyDictionary<TEnum, string> predefinedEnumToName;

        private static readonly int maxEnumIndex = int.MaxValue;
        private static int currentEnumIndex;

        public static int GetValues(ICollection<TEnum> outValues) {
            using var lockRAII = ReaderLockRAII.EnterRead(lockMetadata);
            foreach (var value in mapNameToEnum.Values)
                outValues.Add(value);
            return mapNameToEnum.Count;
        }
        
        public static int GetValues(ICollection<DataEnum<TEnum>> outValues) {
            using var lockRAII = ReaderLockRAII.EnterRead(lockMetadata);
            foreach (var value in mapNameToEnum.Values)
                outValues.Add(value);
            return mapNameToEnum.Count;
        }
        
        public static int GetNames(ICollection<string> outNames) {
            using var lockRAII = ReaderLockRAII.EnterRead(lockMetadata);
            foreach (var name in mapNameToEnum.Keys)
                outNames.Add(name!);
            return mapNameToEnum.Count;
        }
        
        public static TEnum DefineEnumWithIndex(string name, int index) {
            return DefineEnum_Private(name, index, (_, index) => {
                currentEnumIndex = Math.Max(index, currentEnumIndex);
                return BoxlessConvert.To<TEnum>.From(index);
            });
        }

        public static TEnum DefineOrderedEnum(string name) {
            return DefineEnum_Private(name, 0, (name, _) => {
                if (currentEnumIndex == maxEnumIndex) {
                    var maxEnum = BoxlessConvert.To<TEnum>.From(maxEnumIndex);
                    if (mapEnumToName.TryGetValue(maxEnum, out var maxEnumName)) {
                        Debug.LogError($"[DataEnum] Out of ordered indices for enum {typeof(TEnum)}; defining enum {name} to max underlying value {maxEnumIndex:X}, overlapping with existing {maxEnumName}.\n" +
                            "Since it's unlikely to use up so many indices with unique enum names, are there an enum defined with underlying value close to max? Please avoid such use cases.");
                    }
                } else {
                    currentEnumIndex += 1;
                }
                return BoxlessConvert.To<TEnum>.From(currentEnumIndex);
            });
        }

        public static TEnum DefineUnorderedEnum(string name) {
            return DefineEnum_Private(name, 0, (name, _) => {
                int maxAttempts = DataEnum.MaxUnorderedHashAttempts;
                
                #if UNITY_EDITOR || MATECH_TEST
                var listConflictNameValue = new List<(int index, string name)>(maxAttempts);
                string CollisionText() => "\nCollided with:\n" + string.Join("\n", listConflictNameValue.Select(t => $"{t.name} ({unchecked((uint)t.index):X08})"));
                #else
                string CollisionText() => "";
                #endif
                
                for (uint seed = 0; seed < maxAttempts; ++seed) {
                    int index = unchecked((int)(xxHash32.ComputeHash(name, seed) | 0x80000000u));
                    var value = BoxlessConvert.To<TEnum>.From(index);
                    
                    if (!mapEnumToName.ContainsKey(value)) {
                        if (seed != 0) {
                            Debug.Log($"[DataEnum] Hash collision detected. Unordered enum {name} did {listConflictNameValue.Count} attempts before finding a unique hash.{CollisionText()}");
                        }
                        return value;
                    }
                    
                    #if UNITY_EDITOR || MATECH_TEST
                    listConflictNameValue.Add((index, mapEnumToName[value]));
                    #endif
                }
                
                Debug.LogError($"[DataEnum] Too many hash collisions. Unordered enum {name} did {maxAttempts} attempts before finding a unique hash. -1 will be used for this enum name; gameplay features depending on it might break.{CollisionText()}");
                return BoxlessConvert.To<TEnum>.From(-1);
            });
        }
        
        private static TEnum DefineEnum_Private(string name, int index, Func<string, int, TEnum> funcGetValue) {
            using (var lockRAII = ReaderLockRAII.EnterRead(lockMetadata)) {
                if (mapNameToEnum.TryGetValue(name, out var value)) {
                    return value;
                }
            }
            using (var lockRAII = WriterLockRAII.EnterRead(lockMetadata)) {
                // check again for race condition
                if (!mapNameToEnum.TryGetValue(name, out var value)) {
                    lockRAII.EnterWriteFromRead();
                    value = funcGetValue(name, index);
                    mapNameToEnum.TryAdd(name, value);
                    mapEnumToName.TryAdd(value, name);
                }
                return value;
            }
        }
        
        static DataEnum() {
            var enumType = typeof(TEnum);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            
            if (underlyingType == typeof(bool)) {
                throw new NotSupportedException($"[DataEnum] Unsupported boolean-backed enum type [{enumType}]; cannot call `Enum.GetNames()` on it. It doesn't make sense to extend a boolean-backed enum type.");
            }
            if (underlyingType != typeof(int)) {
                if (!DataEnumMaxIndexLookUp.ofTypes.TryGetValue(underlyingType, out maxEnumIndex)) {
                    Debug.LogError($"[DataEnum] Cannot determine max index for underlying type [{underlyingType}] of enum type [{enumType}]. Value auto-increment will be done in int range and might break with integer overflow.");
                }
            }
            
            var values = (TEnum[])Enum.GetValues(typeEnum);
            var names = Enum.GetNames(typeEnum);
            var namesAndValues = names.Zip(values, (name, value) => (name, value)).ToArray();
            
            predefinedNameToEnum = new ReadOnlyDictionary<string, TEnum>(namesAndValues.ToDictionary(t => t.name, t => t.value));
            predefinedEnumToName = new ReadOnlyDictionary<TEnum, string>(namesAndValues.ToDictionary(t => t.value, t => t.name));
            
            mapNameToEnum = new Dictionary<string, TEnum>(predefinedNameToEnum);
            mapEnumToName = new Dictionary<TEnum, string>(predefinedEnumToName);

            if (namesAndValues.Length == 0) {
                currentEnumIndex = -1; // make next 0
            } else {
                currentEnumIndex = namesAndValues.Max(t => BoxlessConvert.To<int>.From(t.value));
            }
        }

        private static string[]? cachedRegisteredNames;
        
        // DO NOT MODIFY OR REMOVE THIS METHOD unless you know what you are doing.
        // Has reflection usage in DataEnumDrawer.
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
            BoxlessConvert.PreserveForEnum<TEnum>();
            GetRegisteredNames();
        }
    }

    internal static class DataEnumMaxIndexLookUp {
        public static readonly Dictionary<Type, int> ofTypes = new() {
            [typeof(sbyte)] = sbyte.MaxValue,
            [typeof(byte)] = byte.MaxValue,
            [typeof(short)] = short.MaxValue,
            [typeof(ushort)] = ushort.MaxValue,
            [typeof(int)] = int.MaxValue,
            [typeof(uint)] = int.MaxValue,
            [typeof(long)] = int.MaxValue,
            [typeof(ulong)] = int.MaxValue,
        };
    }
}