// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEditor;
using UnityEngine;

namespace MaTech.Common.Tools {
    public enum HelpBoxMessageType {
        None, Info, Warning, Error
    }
    
    public class HelpBoxAttribute : PropertyAttribute {
        public string Text { get; private set; }
        public HelpBoxMessageType MessageType { get; private set; }

        public HelpBoxAttribute(string text, HelpBoxMessageType messageType) {
            Text = text;
            MessageType = messageType;
        }
    }

    #if UNITY_EDITOR
    // Modified from https://forum.unity.com/threads/helpattribute-allows-you-to-use-helpbox-in-the-unity-inspector-window.462768/
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : DecoratorDrawer {
        public override float GetHeight() {
            var helpBoxStyle = GUI.skin?.GetStyle("helpbox");
            if (helpBoxStyle == null) return base.GetHeight();
            var helpBoxAttribute = (HelpBoxAttribute)attribute;
            return Mathf.Max(40f, helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.Text), EditorGUIUtility.currentViewWidth) + 4);
        }
 
        public override void OnGUI(Rect position) {
            var helpBoxAttribute = (HelpBoxAttribute)attribute;
            EditorGUI.HelpBox(position, helpBoxAttribute.Text, (MessageType)helpBoxAttribute.MessageType);
        }
    }
    #endif
}