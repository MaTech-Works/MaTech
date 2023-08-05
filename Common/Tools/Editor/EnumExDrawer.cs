// Copyright (c) 2023, LuiCat (as MaTech)
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
    [CustomPropertyDrawer(typeof(EnumEx<>), true)]
    public class EnumExDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var boxedEnum = property.GetBoxedValue();
            if (boxedEnum == null) {
                Debug.LogError($"[EnumEx] Editor not supported for this specific EnumEx case; this is probably due to our mimicked SerializedProperty.boxedValue before Unity version 2022.2.");
                return;
            }
            
            var typeEnumEx = boxedEnum.GetType();
            var methodGetRegisteredNames = typeEnumEx.GetMethod("GetRegisteredNames");
            if (methodGetRegisteredNames == null) {
                Debug.LogError($"[EnumEx] Reflection on EnumEx.GetRegisteredNames failed for editor. Did someone touch the code in a wrong manner?");
                return;
            }

            var names = methodGetRegisteredNames.Invoke(null, null) as string[];
            if (names == null) {
                Debug.LogError($"[EnumEx] Failed to get the name list for editor. Did someone touch the code in a wrong manner?");
                return;
            }
            
            EditorGUI.BeginChangeCheck();

            int selectedIndex = Array.IndexOf(names, boxedEnum.ToString());
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, names);
            
            if (EditorGUI.EndChangeCheck()) {
                var selectedName = names[selectedIndex];
                boxedEnum = Activator.CreateInstance(typeEnumEx, selectedName);
                property.SetBoxedValue(boxedEnum);
            }
        }
    }
    
#if ODIN_INSPECTOR
    public sealed class EnumExOdinDrawer<T> : OdinValueDrawer<EnumEx<T>> where T : unmanaged, Enum, IConvertible {
        protected override void DrawPropertyLayout(GUIContent label) {
            var names = EnumEx<T>.GetRegisteredNames();
            
            EditorGUI.BeginChangeCheck();

            int selectedIndex = Array.IndexOf(names, ValueEntry.SmartValue.ToString());
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names);
            
            if (EditorGUI.EndChangeCheck()) {
                var selectedName = names[selectedIndex];
                ValueEntry.SmartValue = new EnumEx<T>(selectedName);
            }
        }
    }
#endif
}