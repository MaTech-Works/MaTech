// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static MaTech.Gameplay.ChartPlayer;
using static MaTech.Gameplay.Logic.HitMatchMethod;
using static MaTech.Gameplay.Logic.JudgeLogicBase;
using static MaTech.Gameplay.Logic.JudgeLogicBase.NoteHitAction;
#if ODIN_INSPECTOR
using Sirenix.Utilities.Editor;
#endif

namespace MaTech.Gameplay.Logic {
    [Serializable]
    public enum HitMatchMethod {
        [Tooltip("要求判定结果和目标完全一样，没有缺少与多余，才会触发事件")] Exact,
        [Tooltip("判定结果和目标有任意的重合（有交集），就会触发事件")] HasAny,
        [Tooltip("判定结果完全包含目标时触发事件")] ContainsAll,
        [Tooltip("目标完全包含判定结果时触发事件")] InsideAll,
        [Tooltip("目标完全包含判定结果时触发事件")] HasNone,
    }
    
    [Serializable]
    public struct HitCondition {
        public NoteHitAction[] allowedActions;
        public HitMatchMethod matchMethod;
        public HitResult matchTarget;
        
        public bool Match(in HitEvent hit) {
            foreach (NoteHitAction allowedAction in allowedActions) {
                if (allowedAction != hit.action) continue;
                switch (matchMethod) {
                case Exact when hit.result == matchTarget: break;
                case HasAny when hit.result.HasAnyFlag(matchTarget): break;
                case ContainsAll when hit.result.HasAllFlag(matchTarget): break;
                case InsideAll when matchTarget.HasAllFlag(hit.result): break;
                case HasNone when hit.result.HasAnyFlagExcept(HitResult.None, matchTarget): break;
                default: continue;
                }
                return true;
            }
            return false;
        }
    }
    
    // todo: always use this struct for OnHitNote and various methods
    // todo: consider about class with pooling, could be done together with merging EmptyHit
    [Serializable]
    public readonly struct HitEvent {
        public readonly TimeUnit time;
        public readonly NoteHitAction action;
        public readonly HitResult result;
        public readonly IJudgeUnit unit;

        public HitEvent(IJudgeUnit unit, NoteHitAction action, TimeUnit time, HitResult result) {
            this.time = time;
            this.action = action;
            this.result = result;
            this.unit = unit;
        }
        
        public static HitEvent Empty => new();
    }

    [Serializable]
    public class UnityEventHit : UnityEvent<HitEvent> {}
    
    [Serializable]
    public class HitEventBinding : ISerializationCallbackReceiver {
        [SerializeField, HideInInspector, Obsolete] private NoteHitAction[] allowedActions;
        [SerializeField, HideInInspector, Obsolete] private HitMatchMethod matchMethod;
        [SerializeField, HideInInspector, Obsolete] private HitResult matchTarget;
        
        #if ODIN_INSPECTOR
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, ShowIndexLabels = false, CustomAddFunction = "AddConditionWithDefaults")]
        #endif
        public List<HitCondition> conditions;
        public UnityEventHit onHit;

        public bool Invoke(in HitEvent hit) {
            foreach (var condition in conditions) {
                if (!condition.Match(hit))
                    return false;
            }
            onHit.Invoke(hit);
            return true;
        }
        
        #pragma warning disable CS0612 // Type or member is obsolete
        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() {
            if (allowedActions?.Length > 0 || matchTarget is not HitResult.None) {
                conditions = new() { new() {
                    allowedActions = allowedActions,
                    matchMethod = matchMethod,
                    matchTarget = matchTarget,
                }};
                allowedActions = null;
                matchTarget = HitResult.None;
            }
        }
        #pragma warning restore CS0612 // Type or member is obsolete
        
        #if ODIN_INSPECTOR
        private static readonly NoteHitAction[] defaultActions = { Auto, Press, Hold, Release, Flick, Linked };
        private void AddConditionWithDefaults() {
            conditions ??= new();
            conditions.Add(new(){ allowedActions = defaultActions, matchMethod = HasAny });
        }
        #endif
    }

    public static class HitEventExtensions {
        public static void InvokeAll(this HitEventBinding[] bindings, in HitEvent hit) { foreach (var binding in bindings) { binding.Invoke(hit); } }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HitCondition))]
    public class HitEventDrawer : PropertyDrawer {
        private readonly List<DataEnum<NoteHitAction>> cachedEnums = new(10);
        private readonly Dictionary<NoteHitAction, bool> cachedEnumState = new(10);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, null)
                + EditorGUI.GetPropertyHeight(SerializedPropertyType.Enum, null) + 16;
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