// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Data;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaTech.Gameplay.Input {
    public partial class PlayInput {
        public bool IsPlayer => true;

        void IPlayController.AttachController(IPlayControl control) {
            playControl = control;
            InputEnabled = true;
        }

        void IPlayController.DetachController() {
            InputEnabled = false;
            playControl = null;
        }

        void IPlayController.ResetControl(TimeUnit time) => ResetInput();
        void IPlayController.UpdateControl(TimeUnit time) => UpdateInput();

        private IPlayControl playControl;

        // TODO: 令外部送入输入时间码，以便这里根据高精度计时器和平台定义的输入时间码来相对计算InputTime
        private TimeUnit RealtimeTime => PlayTime.InputTime;
        
        private void SendKeyInput(KeyCode keyCode, bool isDown) => SendKeyInput(keyCode, isDown, RealtimeTime);
        private void SendKeyInput(KeyCode keyCode, bool isDown, in TimeUnit time) {
            Profiler.BeginSample("PlayInput.OnKeyInput", this);
            try {
                playControl?.PlayKeyInput(keyCode, isDown, time);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void SendTouchInput(Finger finger) => SendTouchInput(finger, RealtimeTime);
        private void SendTouchInput(Finger finger, in TimeUnit time) {
            Profiler.BeginSample("PlayInput.OnTouchInput", this);
            try {
                playControl?.PlayTouchInput(finger, time);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void SendIndexedInput(int index, bool isDown) => SendIndexedInput(index, isDown, RealtimeTime);
        private void SendIndexedInput(int index, bool isDown, in TimeUnit time) {
            Profiler.BeginSample("PlayInput.OnIndexedInput", this);
            try {
                playControl?.PlayIndexedInput(index, isDown, time);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void IncreaseFingerCountAtIndex(int index) {
            if (index < 0 || index >= keyCount) return;
            if (keyFingerCount.Count != keyCount) {
                keyFingerCount.Resize(keyCount);
            }
            if (keyFingerCount[index] == 0 || allowUnfilteredIndexedInput) {
                SendIndexedInput(index, true);
            }
            keyFingerCount[index] += 1;
        }

        private void DecreaseFingerCountAtIndex(int index) {
            if (index < 0 || index >= keyCount) return;
            if (keyFingerCount.Count != keyCount) {
                keyFingerCount.Resize(keyCount);
            }
            keyFingerCount[index] -= 1;
            if (keyFingerCount[index] == 0 || allowUnfilteredIndexedInput) {
                SendIndexedInput(index, false);
            }
        }
    }
}