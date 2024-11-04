// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Unity;
using MaTech.Common.Utils;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace MaTech.Gameplay.Input {
    public partial class PlayInput : SinglePerSceneBehaviour<PlayInput>, IPlayController {
        [Tooltip("用来推断画面与世界坐标关系的相机，请设置成场景的主相机。")]
        public Camera touchReferenceCamera;

        [Tooltip("是否允许IndexedInput由多指触控触发多次down或up？（开启后，down和up消息不一定交替出现）")]
        public bool allowUnfilteredIndexedInput;

        [SerializeField]
        [Tooltip("总键位数，超过0到KeyCount-1范围的数值将不会触发IndexedInput")]
        private int keyCount = 4;
        public int KeyCount {
            get => keyCount;
            set => keyCount = value;
        }
        
        [Tooltip("从鼠标输入额外生成两个触摸点，左键右键各一个；会过滤掉与多指触摸触点位置重合的鼠标输入")]
        public bool additionalTouchFromMouse;
        public int mouseStartTouchID = 100;

        public Exception LastException { get; private set; }
        private Exception activeException;

        public void ResetInput() {
            ResetTouch();
            ResetKey();
        }
        
        public void UpdateInput() {
            Profiler.BeginSample("PlayInput.UpdateInput()", this);
            UpdateKey();
            UpdateTouch();
            Profiler.EndSample();
            
            if (activeException != null) {
                LastException = activeException;
                Debug.LogException(activeException);
            }
        }

        private bool inputEnabled;
        public bool InputEnabled {
            get => inputEnabled;
            set {
                if (inputEnabled == value) return;
                inputEnabled = value;
                if (inputEnabled) {
                    EnableTouch();
                    EnableKey();
                } else {
                    DisableTouch();
                    DisableKey();
                }
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            InputEnabled = false; // to properly turn off callbacks
        }

        void Reset() {
            touchReferenceCamera = Camera.main;
            if (UnityUtil.IsUnassigned(touchReferenceCamera)) {
                Debug.LogWarning("[PlayInput] Touch Reference Camera is unassigned; touch input will not be working until assigned.");
            }
        }
    }
}
