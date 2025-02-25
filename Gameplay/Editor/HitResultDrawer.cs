// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Scoring;
using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Editor {
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HitResult))]
    public class HitResultDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginChangeCheck();
            var maskValue = EditorGUI.MaskField(position, label, property.intValue, HitResultInfo.Array);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = maskValue;
            }
        }
    }
    #endif
}
