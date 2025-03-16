// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Input;
using MaTech.Gameplay.Logic;
using UnityEngine;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        private PlayControl control;
        private IPlayController controller;
        
        internal IPlayController Controller {
            get => controller;
            set {
                if (HasController && IsPlaying) {
                    controller.DetachController();
                }
                
                control?.Detach();
                control = new PlayControl(this); // discard the previous instance
                
                controller = value;
                
                if (HasController && IsPlaying) {
                    controller.AttachController(control);
                    controller.ResetControl(lastControllerJudgeTime);
                }
            }
        }

        public bool HasController => controller != null;
        public bool IsPlayerControl => controller != null && controller.IsPlayer;
        public bool IsNonPlayerControl => controller != null && !controller.IsPlayer;

        private TimeUnit lastControllerJudgeTime = TimeUnit.MinValue;

        private void EnableController() {
            control?.Attach(this);
            controller?.AttachController(control);
        }

        private void DisableController() {
            controller?.DetachController();
            control?.Detach();
        }
        
        private void ResetController() {
            TimeUnit judgeTime = PlayTime.JudgeTime;
            controller?.ResetControl(judgeTime);
            lastControllerJudgeTime = judgeTime;
        }
        
        private void UpdateController() {
            TimeUnit judgeTime = PlayTime.JudgeTime;
            controller?.UpdateControl(lastControllerJudgeTime = PlayTime.JudgeTime);
            lastControllerJudgeTime = judgeTime;
        }
        
        private class PlayControl : IPlayControl {
            private ChartPlayer player;
            
            public PlayControl(ChartPlayer self) { player = self; }
            
            public void Attach(ChartPlayer self) { player = self; }
            public void Detach() { player = null; }

            // ReSharper disable Unity.NoNullPropagation
            public void PlayKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime) => player?.OnKeyInput(keyCode, isDown, judgeTime);
            public void PlayTouchInput(PlayInput.Finger finger, TimeUnit judgeTime) => player?.OnTouchInput(finger, judgeTime);
            public void PlayIndexedInput(int index, bool isDown, TimeUnit judgeTime) => player?.OnIndexedInput(index, isDown, judgeTime);

            public void PlayScoreUpdate(MetaTable<ScoreType> scoreSnapshot, TimeUnit judgeTime) => player?.OnScoreUpdate(scoreSnapshot, judgeTime);
            // ReSharper restore Unity.NoNullPropagation
        }
        
    }
}