// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaTech.Control {
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

        void IPlayController.ResetControl(int judgeTime) => ResetInput();
        void IPlayController.UpdateControl(int judgeTime) => UpdateInput();

        private IPlayControl playControl;

        // TODO: 需要开发MaTime库来支持实时采样判定时间
        private int RealtimeJudgeTime => 0; //PlayTime.JudgeTime;
        
        private void SendKeyInput(KeyCode keyCode, bool isDown) => SendKeyInput(keyCode, isDown, RealtimeJudgeTime);
        private void SendKeyInput(KeyCode keyCode, bool isDown, int judgeTime) {
            Profiler.BeginSample("PlayInput.OnKeyInput", this);
            try {
                playControl?.PlayKeyInput(keyCode, isDown, judgeTime);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void SendTouchInput(Finger finger) => SendTouchInput(finger, RealtimeJudgeTime);
        private void SendTouchInput(Finger finger, int judgeTime) {
            Profiler.BeginSample("PlayInput.OnTouchInput", this);
            try {
                playControl?.PlayTouchInput(finger, judgeTime);
            } catch (Exception ex) {
                activeException = ex;
            }
            Profiler.EndSample();
        }
        
        private void SendIndexedInput(int index, bool isDown) => SendIndexedInput(index, isDown, RealtimeJudgeTime);
        private void SendIndexedInput(int index, bool isDown, int judgeTime) {
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
            if (keyFingerCount[index] == 0 || allowMultiplePress) {
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
            if (keyFingerCount[index] == 0 || allowMultiplePress) {
                SendIndexedInput(index, false);
            }
        }

        
    }
}