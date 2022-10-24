// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MaTech.Common.Algorithm;
using MaTech.Common.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace MaTech.Common.Utils {
    public static class UnityExtend {
        [Serializable] public class UnityEventInt : UnityEvent<int> { }
        [Serializable] public class UnityEventBool : UnityEvent<bool> { }
        [Serializable] public class UnityEventFloat : UnityEvent<float> { }
        [Serializable] public class UnityEventDouble : UnityEvent<double> { }
        [Serializable] public class UnityEventString : UnityEvent<string> { }
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

        public static void ResetLocally(this RectTransform rectTransform, float minX = 0, float maxX = 1,
            float minY = 0, float maxY = 1) {
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchorMin = new Vector2(minX, minY);
            rectTransform.anchorMax = new Vector2(maxX, maxY);
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

        /// <summary>
        /// 避免重复计算sizeDelta和anchoredPosition
        /// </summary>
        public static void SetOffset(this RectTransform rectTransform, Vector2 min, Vector2 max) {
            var anchoredPosition = rectTransform.anchoredPosition;
            var sizeDelta = rectTransform.sizeDelta;
            var pivot = rectTransform.pivot;
            var pivotReversed = Vector2.one - pivot;

            var a = min - (anchoredPosition - Vector2.Scale(sizeDelta, pivot));
            var b = max - (anchoredPosition + Vector2.Scale(sizeDelta, pivotReversed));
            rectTransform.sizeDelta += b - a;
            rectTransform.anchoredPosition += Vector2.Scale(a, pivotReversed) + Vector2.Scale(a, pivot);
        }

        public static Vector4 ToVector(this Rect rect) {
            return new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
        }

        public static bool Contains(this LayerMask mask, int layer) {
            if (layer < 0 || layer >= 32) return false;
            return (mask & (1 << layer)) != 0;
        }

#if UNITY_EDITOR
        /// <summary> Checks if the object is literally null. In build, this will not check Unity's object lifecycle. </summary>
        public static bool IsNull(Object obj) => ReferenceEquals(obj, null) || !obj;
        /// <summary> Checks if the object is literally not null. In build, this will not check Unity's object lifecycle. </summary>
        public static bool IsNotNull(Object obj) => !ReferenceEquals(obj, null) && obj;
#else
        /// <summary> Checks if the object is literally null, without checking Unity's object lifecycle. </summary>
        public static bool IsNull(Object obj) => ReferenceEquals(obj, null);
        /// <summary> Checks if the object is literally not null, without checking Unity's object lifecycle. </summary>
        public static bool IsNotNull(Object obj) => !ReferenceEquals(obj, null);
#endif

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

        public static void GetComponentsInScene<T>(this MonoBehaviour mono, [NotNull] List<T> result, bool clearResult = true) => GetComponentsInScene<T>(mono.gameObject.scene, result, clearResult);
        public static void GetComponentsInScene<T>(this GameObject obj, [NotNull] List<T> result, bool clearResult = true) => GetComponentsInScene<T>(obj.scene, result, clearResult);
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
            result.Clear();

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
    }
}