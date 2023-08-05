// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    public abstract class TouchAreaBase : MonoBehaviour, IComparable<TouchAreaBase> {
        public static readonly List<TouchAreaBase> activeAreasOrdered = new List<TouchAreaBase>(20); // main thread only
        
        [SerializeField] protected int keyIndex;
        [SerializeField] private int sortOrder;
        public bool allowSlide;
        public bool penetrate;
        
        public int KeyIndex {
            get => keyIndex;
            set {
                keyIndex = value;
                OnSortOrderChanged();
            }
        }

        public int SortOrder {
            get => sortOrder;
            set {
                sortOrder = value;
                OnSortOrderChanged();
            }
        }

        private void OnSortOrderChanged() {
            if (activeAreasOrdered.Remove(this))
                activeAreasOrdered.OrderedInsert(this);
        }

        protected virtual void OnEnable() => activeAreasOrdered.OrderedInsert(this);
        protected virtual void OnDisable() => activeAreasOrdered.Remove(this);

        protected virtual void OnValidate() => OnSortOrderChanged();
        
        public int CompareTo(TouchAreaBase other) {
            int result = sortOrder.CompareTo(other.sortOrder);
            if (result == 0) result = keyIndex.CompareTo(other.keyIndex);
            return result;
        }

        /// <summary>
        /// Ray-cast测试光线是否穿过触摸区域，可以在多线程环境调用。
        /// </summary>
        /// <param name="ray">世界坐标系的光线</param>
        /// <param name="localCastPosition">传出光线投射到局部XY平面上的坐标</param>
        /// <param name="cullBack">是否禁止从反面投射到局部XY平面？</param>
        /// <param name="castFromOrigin">是否只使用光线正半轴进行投射？</param>
        public bool RayCast(Ray ray, out Vector2 localCastPosition, bool cullBack = false, bool castFromOrigin = true) {
            Vector4 castResult = CachedWorldToLocalMatrix.CastRayAlongZAxis(ray); // (x, y, dz, k)
            localCastPosition = (Vector2)castResult;
            
            if (cullBack && castResult.z < 0) return false;  // 光线朝反向投射
            if (castFromOrigin && castResult.w > 0) return false; // 光线起始点穿过了触摸平面
            
            return Contains(localCastPosition);
        }
        
        // 多线程用的镜像数据
        protected Matrix4x4 CachedWorldToLocalMatrix { get; private set; } = Matrix4x4.identity;
        
        protected virtual void LateUpdate() {
            if (transform.hasChanged) {
                CachedWorldToLocalMatrix = UnityWorldToLocalMatrix;
            }
        }

        protected Matrix4x4 UnityWorldToLocalMatrix => AdditionalLocalTransform.inverse * transform.worldToLocalMatrix; // main thread only
        protected Matrix4x4 UnityLocalToWorldMatrix => transform.localToWorldMatrix * AdditionalLocalTransform; // main thread only
        
        protected virtual Matrix4x4 AdditionalLocalTransform => Matrix4x4.identity;

        protected abstract bool Contains(Vector2 localPosition);
        
        #if UNITY_EDITOR
        private PlayInput playInputCached = null;
        private PlayInput PlayInput => playInputCached != null ? playInputCached : playInputCached = PlayInput.GetInstance(gameObject.scene);
        
        void OnDrawGizmos() {
            Handles.matrix = UnityLocalToWorldMatrix;

            Color colorNormal = PlayInput == null ? Color.red :
                (keyIndex < 0 || keyIndex >= PlayInput.KeyCount) ? Color.gray :
                Color.HSVToRGB((float) keyIndex / PlayInput.KeyCount, 1, 1);
            Color colorSelected = Color.Lerp(colorNormal, Color.white, 0.5f);

            bool selected = Selection.Contains(gameObject);
            Handles.color = selected ? colorSelected : colorNormal;
            
            #if UNITY_EDITOR
            if (Application.isPlaying) {
                var ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
                if (RayCast(ray, out var _)) Handles.color = new Color(1f, 1f, 0.7f);
            }
            #endif
            
            DrawLocalGizmos();
            
            Handles.matrix = Matrix4x4.identity;
        }

        protected virtual void DrawLocalGizmos() {
            Handles.DrawAAPolyLine(Vector3.up, Vector3.down);
            Handles.DrawAAPolyLine(Vector3.left, Vector3.right);
        }
        #endif
    }
    
    public abstract class TouchArea3DBase : TouchAreaBase {
        public Vector3 relativePosition;
        public Vector3 relativeRotation;

        protected override Matrix4x4 AdditionalLocalTransform => Matrix4x4.TRS(relativePosition, Quaternion.Euler(relativeRotation), Vector3.one);
    }
    
    [RequireComponent(typeof(RectTransform))]
    public abstract class TouchAreaUIBase : TouchAreaBase {
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
    }
    
}