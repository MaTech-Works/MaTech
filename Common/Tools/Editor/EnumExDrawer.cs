// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Data;
using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace MaTech.Common.Tools.Editor {
    [CustomPropertyDrawer(typeof(DataEnum<>), true)]
    public class DataEnumDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var boxedEnum = property.GetBoxedValue();
            if (boxedEnum == null) {
                Debug.LogError($"[DataEnum] Editor not supported for this specific DataEnum case; this is probably due to our mimicked SerializedProperty.boxedValue before Unity version 2022.2.");
                return;
            }
            
            var typeDataEnum = boxedEnum.GetType();
            var methodGetRegisteredNames = typeDataEnum.GetMethod("GetRegisteredNames");
            if (methodGetRegisteredNames == null) {
                Debug.LogError($"[DataEnum] Reflection on DataEnum.GetRegisteredNames failed for editor. Did someone touch the code in a wrong manner?");
                return;
            }

            var names = methodGetRegisteredNames.Invoke(null, null) as string[];
            if (names == null) {
                Debug.LogError($"[DataEnum] Failed to get the name list for editor. Did someone touch the code in a wrong manner?");
                return;
            }
            
            EditorGUI.BeginChangeCheck();

            int selectedIndex = Array.IndexOf(names, boxedEnum.ToString());
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, names);
            
            if (EditorGUI.EndChangeCheck()) {
                var selectedName = names[selectedIndex];
                boxedEnum = Activator.CreateInstance(typeDataEnum, selectedName);
                property.SetBoxedValue(boxedEnum);
            }
        }
    }
    
#if ODIN_INSPECTOR
    public sealed class DataEnumOdinDrawer<T> : OdinValueDrawer<DataEnum<T>> where T : unmanaged, Enum, IConvertible {
        protected override void DrawPropertyLayout(GUIContent label) {
            var names = DataEnum<T>.GetRegisteredNames();
            
            EditorGUI.BeginChangeCheck();

            int selectedIndex = Array.IndexOf(names, ValueEntry.SmartValue.ToString());
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names);
            
            if (EditorGUI.EndChangeCheck()) {
                var selectedName = names[selectedIndex];
                ValueEntry.SmartValue = new DataEnum<T>(selectedName);
            }
        }
    }
#endif
}