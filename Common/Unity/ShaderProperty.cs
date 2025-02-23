// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MaTech.Common.Unity {
    [Serializable, InlineProperty]
    public struct ShaderProperty {
        public string name;
        public int ID => Empty || Unknown ? unusedID : Unchanged ? id : id = Shader.PropertyToID(lastName = name);
            
        public static ShaderProperty FromName(string name) => new() {
            id = string.IsNullOrWhiteSpace(name) ? unusedID : Shader.PropertyToID(name),
            lastName = name,
            name = name,
        };
        
        public static implicit operator ShaderProperty(string name) => FromName(name);
        public static implicit operator int(ShaderProperty property) => property.ID;
            
        private string lastName;
        private int id;
            
        private bool Empty => string.IsNullOrWhiteSpace(name);
        private bool Unknown => lastName == null;
        private bool Unchanged => lastName == name;
            
        private static readonly int unusedID = Shader.PropertyToID("_Some_Absolutely_Unused_Property");
            
        public override string ToString() {
            if (Empty) return "(empty)";
            if (Unchanged) return $"{name} (id: {id})";
            if (Unknown) return $"{name} (id unknown)";
            return $"{name} (id dirty)";
        }
    }
}