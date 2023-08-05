// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Common.Tools.Editor {
    public class ShowAsVector2Drawer : MaterialPropertyDrawer {
        public override void OnGUI(Rect position, MaterialProperty property, string label, MaterialEditor editor) {
            EditorGUI.LabelField(position, label);
            position.xMin += position.width * 0.4f;
            property.vectorValue = EditorGUI.Vector2Field(position, GUIContent.none, property.vectorValue);
        }
    }
}