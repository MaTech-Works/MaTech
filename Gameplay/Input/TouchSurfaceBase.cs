﻿// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    /// <summary>
    /// 将触摸的ray映射成2d坐标的单一映射关系，尽量在场景配置中保持与判定逻辑一致（如跟随key轨道旋转并缩放，或者根据live的扇形角度区域变化大小）
    /// </summary>
    public abstract class TouchSurfaceBase : MonoBehaviour {
        private static readonly List<TouchSurfaceBase> activeSurfaces = new List<TouchSurfaceBase>(5); // main thread only
        public static TouchSurfaceBase ActiveTouchSurface => activeSurfaces.LastOrDefault();

        public static readonly Vector2Int invalidCoord = new Vector2Int(int.MinValue, int.MinValue);

        [Tooltip("禁用时，在映射区域外的点击会被映射至(-inf,-inf)")]
        public bool allowOutside = false;
        
        /// <summary> 从世界坐标ray转换为模式定义的逻辑坐标。需要多线程支持，避免使用非数学类 Unity API。 </summary>
        public abstract Vector2Int GetInputCoordFromRay(Ray ray);

        // 多线程用的镜像数据
        protected Matrix4x4 CachedWorldToLocalMatrix { get; private set; } = Matrix4x4.identity;

        protected Matrix4x4 UnityWorldToLocalMatrix => AdditionalLocalTransform.inverse * transform.worldToLocalMatrix; // main thread only
        protected Matrix4x4 UnityLocalToWorldMatrix => transform.localToWorldMatrix * AdditionalLocalTransform; // main thread only

        protected virtual Matrix4x4 AdditionalLocalTransform => Matrix4x4.identity;

        protected virtual void LateUpdate() {
            if (transform.hasChanged) {
                CachedWorldToLocalMatrix = UnityWorldToLocalMatrix;
            }
        }
        
        protected virtual void OnEnable() => activeSurfaces.Add(this);
        protected virtual void OnDisable() => activeSurfaces.Remove(this);

        #if UNITY_EDITOR
        protected virtual void DrawLocalGizmos(bool selected) {}
        
        private readonly Color colorSelected = Color.white;
        private readonly Color colorNormal = Color.grey;
        private readonly Color colorDisabled = new Color(1, 0, 0, 0.5f);

        void OnDrawGizmos() {
            bool selected = Selection.Contains(gameObject);

            Handles.color = enabled ? selected ? colorSelected : colorNormal : colorDisabled;
            Handles.matrix = UnityLocalToWorldMatrix;
            DrawLocalGizmos(selected);
            Handles.matrix = Matrix4x4.identity;
        }
        #endif
    }

    /// <summary> 提供 GetInputCoordFromLocalPosition 虚函数，使用投影到指定法向量平面的投影点作为参数 </summary>
    public abstract class TouchSurfaceFlatBase : TouchSurfaceBase {
        protected abstract Vector2Int GetInputCoordFromCastLocalPosition(Vector2 localPosition);

        public Vector3 localSpaceNormal = Vector3.forward;
        public Vector3 localSpaceUp = Vector3.up;

        public sealed override Vector2Int GetInputCoordFromRay(Ray ray) {
            Vector2 localPosition = CachedWorldToLocalMatrix.CastRayAlongZAxis(ray);
            return GetInputCoordFromCastLocalPosition(localPosition);
        }

        protected override Matrix4x4 AdditionalLocalTransform => Matrix4x4.LookAt(Vector3.zero, localSpaceNormal, localSpaceUp);
        
        #if UNITY_EDITOR
        protected override void DrawLocalGizmos(bool selected) {
            base.DrawLocalGizmos(selected);
            Handles.DrawAAPolyLine(Vector3.up, Vector3.down);
            Handles.DrawAAPolyLine(Vector3.left, Vector3.right);
        }
        #endif
    }

    /// <summary> 提供 GetInputCoordFromRectNormalizedPosition 虚函数，使用映射到 rect 的 0-1 标准化坐标 </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class TouchSurfaceUIBase : TouchSurfaceBase {
        protected abstract Vector2Int GetInputCoordFromRectNormalizedPosition(Vector2 normalizedPosition);

        public sealed override Vector2Int GetInputCoordFromRay(Ray ray) {
            Vector2 localPosition = CachedWorldToLocalMatrix.CastRayAlongZAxis(ray);
            if (!CachedLocalRect.Contains(localPosition) && !allowOutside) return invalidCoord;
            Vector2 normalizedPosition = CachedLocalRect.PointToNormalizedUnclamped(localPosition);
            return GetInputCoordFromRectNormalizedPosition(normalizedPosition);
        }

        protected Rect CachedLocalRect { get; private set; } // snapshot for multithreading

        private RectTransform rectTransform;
        protected RectTransform RectTransform {
            get {
                if (rectTransform == null) rectTransform = (RectTransform) transform;
                return rectTransform;
            }
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            CachedLocalRect = RectTransform.rect;
        }

        #if UNITY_EDITOR
        [SerializeField]
        private Vector2Int gizmosGridCount = new Vector2Int(8, 8);
        
        protected override void DrawLocalGizmos(bool selected) {
            base.DrawLocalGizmos(selected);

            Rect rect = RectTransform.rect;
            Vector2 size = rect.size;
            Vector2 min = rect.min;
            Vector2 max = rect.max;

            for (int i = 0; i <= gizmosGridCount.x; ++i) {
                Vector2 v = Vector2.Lerp(min, max, (float)i / gizmosGridCount.x);
                float offset = allowOutside ? size.y / 4 : 0;
                if (i != 0 && i != gizmosGridCount.x) offset /= 2;
                Handles.DrawAAPolyLine(new Vector2(v.x, min.y - offset), new Vector2(v.x, max.y + offset));
            }
            for (int i = 0; i <= gizmosGridCount.y; ++i) {
                Vector2 v = Vector2.Lerp(min, max, (float)i / gizmosGridCount.y);
                float offset = allowOutside ? size.x / 4 : 0;
                if (i != 0 && i != gizmosGridCount.y) offset /= 2;
                Handles.DrawAAPolyLine(new Vector2(min.x - offset, v.y), new Vector2(max.x + offset, v.y));
            }
        }
        #endif
    }
    
}