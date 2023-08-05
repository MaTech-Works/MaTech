// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Time;
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

        void IPlayController.ResetControl(TimeUnit judgeTime) => ResetInput();
        void IPlayController.UpdateControl(TimeUnit judgeTime) => UpdateInput();

        private IPlayControl playControl;

        // TODO: 令外部送入帧同步非实时的时间码，以便这里根据高精度计时器和平台定义的输入时间码来相对计算JudgeTime
        private TimeUnit RealtimeJudgeTime => PlayTime.JudgeTime;
        
        private void SendKeyInput(KeyCode keyCode, bool isDown) => SendKeyInput(keyCode, isDown, RealtimeJudgeTime);
        private void SendKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime) {
            Profiler.BeginSample("PlayInput.OnKeyInput", this);
            try {
                playControl?.PlayKeyInput(keyCode, isDown, judgeTime);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void SendTouchInput(Finger finger) => SendTouchInput(finger, RealtimeJudgeTime);
        private void SendTouchInput(Finger finger, TimeUnit judgeTime) {
            Profiler.BeginSample("PlayInput.OnTouchInput", this);
            try {
                playControl?.PlayTouchInput(finger, judgeTime);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void SendIndexedInput(int index, bool isDown) => SendIndexedInput(index, isDown, RealtimeJudgeTime);
        private void SendIndexedInput(int index, bool isDown, TimeUnit judgeTime) {
            Profiler.BeginSample("PlayInput.OnIndexedInput", this);
            try {
                playControl?.PlayIndexedInput(index, isDown, judgeTime);
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