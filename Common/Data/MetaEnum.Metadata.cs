// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using MaTech.Common.Algorithm;
using Standart.Hash.xxHash;
using UnityEngine;

namespace MaTech.Common.Data {
    public partial struct MetaEnum {
        private static readonly ReaderWriterLockSlim lockMetadata = new();
        
        private static readonly Dictionary<uint, string> knownNamesByID = new();
        private static readonly Dictionary<string, uint> knownIDsByName = new();
        private static readonly Dictionary<Type, List<uint>> knownTypeAliasIDs = new();
        
        #if UNITY_EDITOR || MATECH_TEST
        private static readonly Dictionary<string, string> knownInvariantNames = new(); // invariant name (lower case invariant culture) --> full name
        #endif

        private static uint GetEnumID(string name) => GetEnumID_Impl(name, null);
        private static uint GetEnumID<T>() where T : unmanaged, Enum, IConvertible => GetEnumID_Impl(null, typeof(T));

        private static uint GetEnumID_Impl(string? name, Type? type) {
            if (type == null && string.IsNullOrEmpty(name)) {
                Debug.LogError("[MetaEnum] Trying to register an enum without type with empty name. Please report a bug or check implementations.");
                return 0;
            }

            name ??= type!.Name; // name and type will not be both null here
            
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError($"[MetaEnum] Trying to register an enum of type [{type}] with empty name. Please report a bug or check implementations.");
                return 0;
            }

            using var lockRAII = WriterLockRAII.EnterRead(lockMetadata);

            if (knownIDsByName.ContainsKey(name))
                return 0;
            
            lockRAII.EnterWriteFromRead();

            #if UNITY_EDITOR || MATECH_TEST
            var invariantName = name.ToLowerInvariant();
            if (knownInvariantNames.TryGetValue(invariantName, out var similarName)) {
                Debug.LogWarning($"[MetaEnum] Registering an enum [{name}] with name of different casing with existing name [{similarName}]. " +
                    "MetaEnum treat enum types with exactly matching name to be inter-convertible/comparable by value; with different casing, this is not granted. " +
                    "Please check if any name is a typo.");
            } else {
                knownInvariantNames.Add(invariantName, name);
            }
            #endif

            if (!knownIDsByName.TryGetValue(name, out var id)) {
                id = xxHash32.ComputeHash(name);
                if (id == 0) {
                    Debug.LogError($"[MetaEnum] Hash collision of name [{name}] with hash value zero. " +
                        "Please report this case to developer, and as a workaround, please try a slightly different name to avoid this issue.");
                    return 0;
                }
                if (knownNamesByID.TryGetValue(id, out var hashCollisionName)) {
                    Debug.LogError($"[MetaEnum] Hash collision of name [{name}] with name [{hashCollisionName}], both having hash value of [{id:X016}]. " +
                        "Please report this case to developer, and as a workaround, please try a slightly different name to avoid this issue.");
                    return 0;
                }
                knownNamesByID.Add(id, name);
                knownIDsByName.Add(name, id);
            }

            if (type != null && name != type.Name) {
                var list = knownTypeAliasIDs.GetOrNew(type);
                list.OrderedInsert(id, CollectionsExtension.ResolveEqual.ReplaceFirstEqual);
            }
            
            return id;
        }

    }
}