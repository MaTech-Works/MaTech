// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Data;
using MaTech.Gameplay.Scoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static MaTech.Gameplay.Scoring.JudgeLogicBase;

namespace MaTech.Gameplay.Utils {
    [Serializable]
    public struct HitEventCondition {
        public NoteHitAction[] allowedActions;
        public HitResult matchTarget;
        public MatchMethod matchMethod;

        [Serializable]
        public enum MatchMethod {
            [Tooltip("要求判定结果和目标完全一样，没有缺少与多余，才会触发事件")] Exact,
            [Tooltip("判定结果和目标有任意的重合（有交集），就会触发事件")] HasAny,
            [Tooltip("判定结果完全包含目标时触发事件")] ContainsAll,
            [Tooltip("目标完全包含判定结果时触发事件")] InsideAll,
        }
        
        public bool Match(NoteHitAction action, HitResult result) {
            foreach (NoteHitAction allowedAction in allowedActions) {
                if (allowedAction != action) continue;
                switch (matchMethod) {
                case MatchMethod.Exact when result == matchTarget: break;
                case MatchMethod.ContainsAll when result.HasAllFlag(matchTarget): break;
                case MatchMethod.HasAny when result.HasAnyFlag(matchTarget): break;
                case MatchMethod.InsideAll when matchTarget.HasAllFlag(result): break;
                default: continue;
                }
                return true;
            }
            return false;
        }
    }
    
    [Serializable]
    public class HitEvent {
        public NoteHitAction[] allowedActions;
        public HitResult matchTarget;
        public MatchMethod matchMethod;

        public UnityEvent onHit;
        
        [Serializable]
        public enum MatchMethod {
            [Tooltip("要求判定结果和目标完全一样，没有缺少与多余，才会触发事件")] Exact,
            [Tooltip("判定结果和目标有任意的重合（有交集），就会触发事件")] HasAny,
            [Tooltip("判定结果完全包含目标时触发事件")] ContainsAll,
            [Tooltip("目标完全包含判定结果时触发事件")] InsideAll,
        }
        
        public bool Match(NoteHitAction action, HitResult result) {
            foreach (NoteHitAction allowedAction in allowedActions) {
                if (allowedAction != action) continue;
                switch (matchMethod) {
                case MatchMethod.Exact when result == matchTarget: break;
                case MatchMethod.ContainsAll when result.HasAllFlag(matchTarget): break;
                case MatchMethod.HasAny when result.HasAnyFlag(matchTarget): break;
                case MatchMethod.InsideAll when matchTarget.HasAllFlag(result): break;
                default: continue;
                }
                return true;
            }
            return false;
        }

        public bool InvokeIfMatch(NoteHitAction action, HitResult result) {
            if (Match(action, result)) {
                onHit.Invoke();
                return true;
            }
            return false;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HitEvent))]
    [CustomPropertyDrawer(typeof(HitEventCondition))]
    public class HitEventDrawer : PropertyDrawer {
        private readonly List<DataEnum<NoteHitAction>> cachedEnums = new List<DataEnum<NoteHitAction>>(10);
        private readonly Dictionary<NoteHitAction, bool> cachedEnumState = new Dictionary<NoteHitAction, bool>(10);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.FindPropertyRelative("onHit") is { } onHit) {
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, null)
                    + EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, null)
                    + EditorGUI.GetPropertyHeight(onHit)
                    + 16;
            } else {
                return EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, null)
                    + EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, null)
                    + 16;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none);
            position.yMax -= 4;
            position.yMin += 4;
            position.xMax -= 8;

            SerializedProperty hitResultsProperty = property.FindPropertyRelative("allowedActions");
            float hitResultsHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, null);
            Rect hitResultsRect = position;
            hitResultsRect.height = hitResultsHeight;
            hitResultsRect.xMin += 12;
            position.yMin += hitResultsHeight + 4;

            DrawHitResultCheckBoxes(hitResultsRect, hitResultsProperty);

            SerializedProperty hitTimingProperty = property.FindPropertyRelative("matchTarget");
            float hitTimingHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, null);
            Rect hitTimingRect = position;
            hitTimingRect.height = hitTimingHeight;
            hitTimingRect.width *= 0.6f;
            hitTimingRect.xMin += 12;
            position.yMin += hitTimingHeight + 4;

            EditorGUIUtility.labelWidth = 80;
            EditorGUI.PropertyField(hitTimingRect, hitTimingProperty);

            SerializedProperty matchProperty = property.FindPropertyRelative("matchMethod");
            Rect matchRect = hitTimingRect;
            matchRect.xMin = matchRect.xMax + 4;
            matchRect.xMax = position.xMax;

            EditorGUIUtility.labelWidth = 86;
            EditorGUI.PropertyField(matchRect, matchProperty);

            if (property.FindPropertyRelative("onHit") is { } onHit) {
                Rect hitEventRect = position;
                hitEventRect.xMin += 12;
                EditorGUI.PropertyField(hitEventRect, onHit);
            }

            EditorGUI.EndProperty();
        }

        private void DrawHitResultCheckBoxes(Rect position, SerializedProperty property) {
            cachedEnums.Clear();
            DataEnum<NoteHitAction>.GetValues(cachedEnums);
            
            foreach (var action in cachedEnums) {
                cachedEnumState[action] = false;
            }

            for (int i = 0, n = property.arraySize; i < n; ++i) {
                cachedEnumState[(NoteHitAction)property.GetArrayElementAtIndex(i).intValue] = true;
            }

            EditorGUI.BeginChangeCheck();
            foreach (NoteHitAction action in cachedEnums) {
                string label = action.ToString();
                position.width = (label.Length * 7) - (label.Count(c => c == 'i' || c == 'l') * 4) + 24; // estimated
                cachedEnumState[action] = EditorGUI.ToggleLeft(position, label, cachedEnumState[action]);
                position.xMin = position.xMax;
            }

            if (EditorGUI.EndChangeCheck()) {
                property.ClearArray();
                property.arraySize = cachedEnumState.Count(pair => pair.Value);
                foreach ((NoteHitAction action, int i) in cachedEnumState.Where(pair => pair.Value).Select((pair, i) => (pair.Key, i))) {
                    property.GetArrayElementAtIndex(i).intValue = (int)action;
                }
            }
        }
    }
#endif
}