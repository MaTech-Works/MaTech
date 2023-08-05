// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Common.Tools {
    public class ButtonInInspectorAttribute : PropertyAttribute {}

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ButtonInInspectorAttribute))]
    public class ButtonInInspectorDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.Generic, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (GUI.Button(position, label)) {
                property.boolValue = true;
            }
        }
    }
#endif
}