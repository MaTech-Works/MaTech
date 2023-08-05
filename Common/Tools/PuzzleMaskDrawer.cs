// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Common.Tools {
    public class PuzzleMaskAttribute : PropertyAttribute {}
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PuzzleMaskAttribute))]
    public class PuzzleMaskDrawer : PropertyDrawer {
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, GUIContent.none) * 4;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            int value = property.intValue;

            EditorGUI.LabelField(position, label);
            position.xMin += EditorGUIUtility.labelWidth;
            
            float toggleSize = position.height / 4;
            Rect togglePosition = new Rect(Vector2.zero, Vector2.one * toggleSize);
            
            for (int i = 0; i < 4; ++i) {
                for (int j = 0; j < 4; ++j) {
                    int bitMask = (1 << (i * 4 + j));
                    togglePosition.position = position.position + new Vector2(toggleSize * j, toggleSize * i);
                    if (EditorGUI.Toggle(togglePosition, (value & bitMask) != 0)) {
                        value |= bitMask;
                    } else {
                        value &= ~bitMask;
                    }
                }
            }

            property.intValue = value;
        }
        
    }
    #endif
}
