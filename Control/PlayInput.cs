// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using MaTech.Common.Unity;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaTech.Control {
    public partial class PlayInput : SinglePerSceneBehaviour<PlayInput>, IPlayController {
        [Tooltip("用来推断画面与世界坐标关系的相机，请设置成场景的主相机。")]
        public Camera touchReferenceCamera;

        [Tooltip("是否允许IndexedInput由多指触控触发多次down或up？（开启后，down和up消息不一定交替出现）")]
        public bool allowMultiplePress;

        [SerializeField]
        [Tooltip("总键位数，超过0到KeyCount-1范围的数值将不会触发IndexedInput")]
        private int keyCount = 4;
        public int KeyCount {
            get => keyCount;
            set => keyCount = value;
        }
        
        public bool simulateTouchWithMouse;
        public int mouseStartTouchID = 100;

        public Exception lastException;
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
                lastException = activeException;
                Debug.LogException(lastException);
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
    }
}
