// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Common.Tools {
    public class FlagMaskAttribute : PropertyAttribute {
        public string[] CustomNames { get; set; }
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FlagMaskAttribute))]
    public class EnumFlagDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginChangeCheck();

            var enumName = ((FlagMaskAttribute) attribute).CustomNames ?? property.enumNames;
            var maskValue = EditorGUI.MaskField(position, label, property.intValue, enumName);
            
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = maskValue;
            }
        }
    }
    #endif
}
