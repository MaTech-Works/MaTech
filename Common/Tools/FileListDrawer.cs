// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Linq;
using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;

namespace MaTech.Common.Tools {
    public class FileListAttribute : PropertyAttribute {
        public string RootPath { get; set; } = "Assets/";
        public bool UseRelativePath { get; set; } = true;
        public string[] ExtensionNames { get; set; }
        public int MaxResults { get; set; } = 100;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FileListAttribute))]
    public class FileListDrawer : PropertyDrawer {
        private static readonly GUIContent[] dropListNone = new []{ GUIContent.none };
        private GUIContent[] dropList = dropListNone;
        private int lastIndex;

        private bool needRescan = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var positionButton = position;
            positionButton.xMin = positionButton.xMax - 60;
            position.xMax -= 64;
            
            if (GUI.Button(positionButton, "Rescan") || needRescan) RescanFiles(property.stringValue);

            lastIndex = EditorGUI.Popup(position, label, lastIndex, dropList);
            property.stringValue = dropList[lastIndex].text;
        }

        private void RescanFiles(string originalPath) {
            needRescan = false;
            
            try {
                lastIndex = 0;
                
                var attributeFileList = (FileListAttribute)attribute;
                var extensionNames = attributeFileList.ExtensionNames;
                if (extensionNames == null || extensionNames.Length == 0) {
                    dropList = dropListNone;
                    return;
                }

                var rootPath = attributeFileList.RootPath;
                if (!Path.IsPathRooted(rootPath)) {
                    rootPath = Path.Combine(Directory.GetCurrentDirectory(), rootPath).ToLinuxPath();
                }

                dropList = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                    .Where(path => extensionNames.Contains(Path.GetExtension(path)?.ToLower(), StringComparer.OrdinalIgnoreCase))
                    .Take(attributeFileList.MaxResults)
                    .Select(path => new GUIContent(path.ToLinuxPath().Replace(rootPath, "").Trim('/')))
                    .ToArray();

                for (int i = 0; i < dropList.Length; ++i) {
                    if (dropList[i].text == originalPath) {
                        lastIndex = i;
                    }
                }
            } catch (Exception ex) {
                Debug.LogError("[FileListDrawer] Error happened when scanning the folder. Check parameter in [FileList] attribute. See the exception for more info.");
                Debug.LogException(ex);
            }
        }
    }

    public static class FileListDrawerExtension {
        public static string ToLinuxPath(this string path) {
            return path.Replace('\\', '/');
        }
    }
    #endif
}