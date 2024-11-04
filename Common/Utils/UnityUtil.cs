// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MaTech.Common.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace MaTech.Common.Utils {
    public static class UnityUtil {
        [Serializable] public class UnityEventInt : UnityEvent<int> { }
        [Serializable] public class UnityEventBool : UnityEvent<bool> { }
        [Serializable] public class UnityEventFloat : UnityEvent<float> { }
        [Serializable] public class UnityEventDouble : UnityEvent<double> { }
        [Serializable] public class UnityEventString : UnityEvent<string> { }
        [Serializable] public class UnityEventVector2 : UnityEvent<Vector2> { }
        [Serializable] public class UnityEventVector3 : UnityEvent<Vector3> { }
        [Serializable] public class UnityEventVector4 : UnityEvent<Vector4> { }
        [Serializable] public class UnityEventException : UnityEvent<Exception> { }
        [Serializable] public class UnityEventGameObject : UnityEvent<GameObject> { }

        public static void DestroyAllChildren(this Transform transform) => transform.DestroyChildren();
        public static void DestroyChildren(this Transform transform, int start = 0, int count = -1) {
            if (transform.childCount == 0) return;
            if (count == -1 || count > transform.childCount) {
                count = transform.childCount;
            }

            for (int i = start + count - 1; i >= start; i--) {
                var obj = transform.GetChild(i);
                obj.SetParent(null);
                Object.Destroy(obj.gameObject);
            }
        }

        public static void DeactiveAllChildren(this Transform transform) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                var obj = transform.GetChild(i);
                obj.gameObject.SetActive(false);
            }
        }

        public static void SetLocalPositionXY(this Transform transform, float x, float y) {
            transform.localPosition = new Vector3(x, y, transform.localPosition.z);
        }

        public static void SetLocalPositionXY(this Transform transform, Vector2 v) {
            transform.localPosition = new Vector3(v.x, v.y, transform.localPosition.z);
        }

        public static void ResetLocally(this RectTransform rectTransform, float minAnchorX = 0, float maxAnchorX = 1, float minAnchorY = 0, float maxAnchorY = 1) {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchorMin = new Vector2(minAnchorX, minAnchorY);
            rectTransform.anchorMax = new Vector2(maxAnchorX, maxAnchorY);
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public static void CopyTo(this RectTransform source, RectTransform target) {
            target.sizeDelta = source.sizeDelta;
            target.localPosition = source.localPosition;
            target.anchorMax = source.anchorMax;
            target.anchorMin = source.anchorMin;
            target.pivot = source.pivot;
            target.anchoredPosition3D = source.anchoredPosition3D;
            target.offsetMax = source.offsetMax;
            target.offsetMin = source.offsetMin;
        }

        public static void SetAnchor(this RectTransform rectTransform, float x, float y) {
            rectTransform.anchorMin = new Vector2(x, y);
            rectTransform.anchorMax = new Vector2(x, y);
        }
        
        public static void SetAnchor(this RectTransform rectTransform, float minX, float maxX, float minY, float maxY) {
            rectTransform.anchorMin = new Vector2(minX, minY);
            rectTransform.anchorMax = new Vector2(maxX, maxY);
        }

        public static void SetAnchorX(this RectTransform rectTransform, float value) {
            rectTransform.anchorMin = new Vector2(value, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(value, rectTransform.anchorMax.y);
        }

        public static void SetAnchorY(this RectTransform rectTransform, float value) {
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, value);
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, value);
        }

        public static void SetAnchorX(this RectTransform rectTransform, float min, float max) {
            rectTransform.anchorMin = new Vector2(min, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(max, rectTransform.anchorMax.y);
        }

        public static void SetAnchorY(this RectTransform rectTransform, float min, float max) {
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, min);
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, max);
        }
        
        public static void SetOffset(this RectTransform rectTransform, float x, float y) {
            rectTransform.offsetMin = new Vector2(x, y);
            rectTransform.offsetMax = new Vector2(x, y);
        }
        
        public static void SetOffset(this RectTransform rectTransform, float minX, float maxX, float minY, float maxY) {
            rectTransform.offsetMin = new Vector2(minX, minY);
            rectTransform.offsetMax = new Vector2(maxX, maxY);
        }

        public static void SetOffsetX(this RectTransform rectTransform, float value) {
            rectTransform.offsetMin = new Vector2(value, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(value, rectTransform.offsetMax.y);
        }

        public static void SetOffsetY(this RectTransform rectTransform, float value) {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, value);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, value);
        }

        public static void SetOffsetX(this RectTransform rectTransform, float min, float max) {
            rectTransform.offsetMin = new Vector2(min, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(max, rectTransform.offsetMax.y);
        }

        public static void SetOffsetY(this RectTransform rectTransform, float min, float max) {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, min);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, max);
        }
        
        public static Vector3 GetAnchoredLocalPosition(this RectTransform rectTransform, Vector2 ratio) {
            return rectTransform.rect.NormalizedToPointUnclamped(ratio);
        }
        
        public static Vector3 GetAnchoredWorldPosition(this RectTransform rectTransform, Vector2 ratio) {
            return rectTransform.localToWorldMatrix.MultiplyPoint(rectTransform.GetAnchoredLocalPosition(ratio));
        }

        public static Vector4 ToVector(this Rect rect) {
            return new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
        }

        public static bool Contains(this LayerMask mask, int layer) {
            if (layer < 0 || layer >= 32) return false;
            return (mask & (1 << layer)) != 0;
        }

        /// <summary> Checks if the object is literally null. Missing references are treated as assigned. </summary>
        public static bool IsUnassigned([NotNullWhen(false)] Object obj) => ReferenceEquals(obj, null) || obj.GetHashCode() == 0;
        /// <summary> Checks if the object is literally not null. Missing references are treated as assigned. </summary>
        public static bool IsAssigned([NotNullWhen(true)] Object obj) => !ReferenceEquals(obj, null) && obj.GetHashCode() != 0;

        public static T NullifyInvalid<T>(T obj) where T : Object => obj == null ? null : obj;
        public static T NullifyUnassigned<T>(T obj) where T : Object => obj == null ? null : obj; 

        private static readonly List<GameObject> reusedGameObjectList = new List<GameObject>(); // 仅在主线程使用
        private static readonly List<Component> reusedComponentList = new List<Component>(); // 仅在主线程使用

        public static T GetGlobalComponent<T>(string name) where T : Component => GetGlobalGameObject(name).GetOrAddComponent<T>();
        public static GameObject GetGlobalGameObject(string name) {
            // TODO: optimize this with a dictionary if we actually want use this pattern
            Debug.LogWarning("UnityUtil.GetGlobalGameObject is not optimized. Please check the code if this is used anywhere.");
            GameObject obj = GameObject.Find(name);
            if (obj != null) {
                return obj;
            }
            return new GameObject(name);
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour mono) where T : Component => mono.gameObject.GetOrAddComponent<T>();
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component {
            var component = obj.GetComponent<T>();
            return component != null ? component : obj.AddComponent<T>();
        }

        public static T GetComponentInScene<T>(this MonoBehaviour mono) => GetComponentInScene<T>(mono.gameObject.scene);
        public static T GetComponentInScene<T>(this GameObject obj) => GetComponentInScene<T>(obj.scene);
        public static T GetComponentInScene<T>(this UnityScene scene) {
            if (!scene.isLoaded) {
                Debug.LogError("GetComponentInScene is called when the scene is not loaded; any janky logic with Unity Editor? \n" +
                               $"Stacktrace:\n{Environment.StackTrace}");
                return default;
            }

            scene.GetRootGameObjects(reusedGameObjectList);
            foreach (var gameObject in reusedGameObjectList) {
                gameObject.GetComponentsInChildren(reusedComponentList);
                foreach (var comp in reusedComponentList) {
                    if (comp is T t) return t;
                }
            }

            return default;
        }

        public static void GetComponentsInScene<T>(this MonoBehaviour mono, [NotNull] List<T> result, bool clearResult = true) => GetComponentsInScene(mono.gameObject.scene, result, clearResult);
        public static void GetComponentsInScene<T>(this GameObject obj, [NotNull] List<T> result, bool clearResult = true) => GetComponentsInScene(obj.scene, result, clearResult);
        public static void GetComponentsInScene<T>(this UnityScene scene, [NotNull] List<T> result, bool clearResult = true) {
            Assert.IsNotNull(result);
            if (clearResult) result.Clear();

            if (!scene.isLoaded) {
                Debug.LogError("GetComponentsInScene is called when the scene is not loaded; any janky logic with Unity Editor? \n" +
                               $"Stacktrace:\n{Environment.StackTrace}");
                return;
            }

            scene.GetRootGameObjects(reusedGameObjectList);
            foreach (var gameObject in reusedGameObjectList) {
                gameObject.GetComponentsInChildren(reusedComponentList);
                foreach (var comp in reusedComponentList) {
                    if (comp is T t) result.Add(t);
                }
            }
        }

        public static T GetComponentSinglePerScene<T>(this UnityScene scene) where T : SinglePerSceneBehaviour<T> => SinglePerSceneBehaviour<T>.GetInstance(scene);
        public static T GetComponentSinglePerScene<T>(this MonoBehaviour mono) where T : SinglePerSceneBehaviour<T> => SinglePerSceneBehaviour<T>.GetInstance(mono.gameObject.scene);
        public static T GetComponentSinglePerScene<T>(this GameObject obj) where T : SinglePerSceneBehaviour<T> => SinglePerSceneBehaviour<T>.GetInstance(obj.scene);

        public static void GetComponentsInAllScenes<T>([NotNull] List<T> result, bool clearResult = true) {
            Assert.IsNotNull(result);
            if (clearResult) result.Clear();

            for (var i = 0; i < SceneManager.sceneCount; i++) {
                SceneManager.GetSceneAt(i).GetComponentsInScene(result, false);
            }
        }

#if UNITY_EDITOR
        // http://answers.unity.com/answers/1538287/view.html
        public static void CopySerialized(Object sourceObject, Object targetObject) {
            var source = new SerializedObject(sourceObject);
            var dest = new SerializedObject(targetObject);
            var iter = source.GetIterator();

            if (iter.NextVisible(true)) {
                while (iter.NextVisible(true)) {
                    var prop = dest.FindProperty(iter.name);
                    if (prop != null && prop.propertyType == iter.propertyType) {
                        dest.CopyFromSerializedProperty(iter);
                    }
                }
            }

            dest.ApplyModifiedProperties();
        }
#endif

        public static Vector2 DotCross(this Vector2 self, Vector2 other) {
            return new Vector2(self.x * other.x + self.y * other.y, self.x * other.y - self.y * other.x);
        }

        public static float InvLength(this Vector2 self) {
            return (float)Math.Sqrt(1.0f / (self.x * self.x + self.y * self.y));
        }

        public static float Angle(this Vector2 self) {
            return Mathf.Atan2(self.y, self.x);
        }

        public static Vector2 Transform(this Vector2 self, Vector2 other) {
            return new Vector2(self.x * other.x - self.y * other.y, self.x * other.y + self.y * other.x);
        }

        public static Vector2 LerpUnclamped(this Vector2 self, Vector2 a, Vector2 b) {
            return new Vector2(Mathf.LerpUnclamped(a.x, b.x, self.x), Mathf.LerpUnclamped(a.y, b.y, self.y));
        }

        public static Vector2 InverseLerpUnclamped(this Vector2 self, Vector2 a, Vector2 b) {
            return new Vector2(MathUtil.InverseLerpUnclamped(a.x, b.x, self.x), MathUtil.InverseLerpUnclamped(a.y, b.y, self.y));
        }

        public static Ray MultiplyRay(this Matrix4x4 matrix, in Ray ray) {
            Vector3 origin = matrix.MultiplyPoint(ray.origin);
            Vector3 direction = matrix.MultiplyVector(ray.direction);
            return new Ray(origin, direction);
        }

        public static Vector4 CastRayAlongZAxis(this Matrix4x4 matrix, in Ray ray) {
            Vector3 direction = matrix.MultiplyVector(ray.direction);
            if (Mathf.Approximately(direction.z, 0)) { // 光线平行于XY平面
                return Vector2.positiveInfinity;
            }
            Vector3 origin = matrix.MultiplyPoint(ray.origin);

            float k = origin.z / direction.z;
            Vector4 result = (Vector2)origin - (Vector2)direction * k;
            result.z = direction.z;
            result.w = k;

            return result; // (x, y, dz, k)
        }

        public static Vector2Int RoundToInt(in this Vector2 value, MathUtil.RoundingMode mode = MathUtil.RoundingMode.Round) {
            return new Vector2Int(MathUtil.RoundToInt(value.x, mode), MathUtil.RoundToInt(value.y, mode));
        }

        public static Vector2 PointToNormalizedUnclamped(in this Rect rect, Vector2 point) {
            return point.InverseLerpUnclamped(rect.min, rect.max);
        }
        public static Vector2 NormalizedToPointUnclamped(in this Rect rect, Vector2 point) {
            return point.LerpUnclamped(rect.min, rect.max);
        }
        
        public static Rect ScaleBy(in this Rect rect, Vector2 scale) {
            return new Rect(rect.position * scale, rect.size * scale);
        }
        public static Rect NormalizeTo(in this Rect rect, Vector2 size) {
            return new Rect(rect.position / size, rect.size / size);
        }

        public static float GetBeginTime(this AnimationCurve curve) => curve.keys.FirstOrDefault().time;
        public static float GetEndTime(this AnimationCurve curve) => curve.keys.LastOrDefault().time;

        #if UNITY_EDITOR
        // Reference: https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-7508561
        public static object GetBoxedValue(this SerializedProperty property) {
            #if UNITY_2022_2_OR_NEWER
            return property.boxedValue;
            #else
            object target = property.serializedObject.targetObject;
            
            string[] tokens = property.propertyPath.Replace(".Array.data[",".").Replace("]", "").Split('.'); // x.Array.data[i] --> x.i
            foreach (var token in tokens) {
                if (target == null) return null;
                if (int.TryParse(token, out int index)) {
                    if (target is not object[] array) return null;
                    target = array[index];
                } else {
                    target = GetBoxedValue_GetField(target, token);
                }
            }
            
            return target;
            #endif
        }

        private static object GetBoxedValue_GetField(object target, string token) {
            var targetType = target.GetType();
            while (targetType != null) {
                var field = targetType.GetField(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) return field.GetValue(target);
                targetType = targetType.BaseType;
            }
            return null;
        }
        
        public static bool SetBoxedValue(this SerializedProperty property, object value) {
            #if UNITY_2022_2_OR_NEWER
            property.boxedValue = value;
            return true;
            #else
            object target = property.serializedObject.targetObject;
            
            string[] tokens = property.propertyPath.Replace(".Array.data[",".").Trim(']').Split('.'); // x.Array.data[i] --> x.i
            var lastToken = tokens.Last();
            foreach (var token in tokens) {
                if (target == null) return false;
                if (int.TryParse(token, out int index)) {
                    var array = target as object[];
                    if (array == null) return false;
                    if (token == lastToken) {
                        array[index] = value;
                    } else {
                        target = array?[index];
                    }
                } else {
                    if (token == lastToken) {
                        return SetBoxedValue_SetField(target, token, value);
                    } else {
                        target = GetBoxedValue_GetField(target, token);
                    }
                }
            }
            
            return true;
            #endif
        }

        private static bool SetBoxedValue_SetField(object target, string token, object value) {
            var targetType = target.GetType();
            while (targetType != null) {
                var field = targetType.GetField(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) {
                    if (field.FieldType.IsInstanceOfType(value)) {
                        field.SetValue(target, value);
                        return true;
                    }
                    return false;
                }
                targetType = targetType.BaseType;
            }
            return false;
        }

        #endif
    }
}