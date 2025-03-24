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
                    controller.ResetControl(lastControllerTime);
                }
            }
        }

        public bool HasController => controller != null;
        public bool IsPlayerControl => controller != null && controller.IsPlayer;
        public bool IsNonPlayerControl => controller != null && !controller.IsPlayer;

        private TimeUnit lastControllerTime = TimeUnit.MinValue;

        private void EnableController() {
            control?.Attach(this);
            controller?.AttachController(control);
        }

        private void DisableController() {
            controller?.DetachController();
            control?.Detach();
        }
        
        private void ResetController() {
            TimeUnit time = PlayTime.InputTime;
            controller?.ResetControl(time);
            lastControllerTime = time;
        }
        
        private void UpdateController() {
            TimeUnit time = PlayTime.InputTime;
            controller?.UpdateControl(lastControllerTime = PlayTime.InputTime);
            lastControllerTime = time;
        }
        
        private class PlayControl : IPlayControl {
            private ChartPlayer player;
            
            public PlayControl(ChartPlayer self) { player = self; }
            
            public void Attach(ChartPlayer self) { player = self; }
            public void Detach() { player = null; }

            // ReSharper disable Unity.NoNullPropagation
            public void PlayKeyInput(KeyCode keyCode, bool isDown, TimeUnit time) => player?.OnKeyInput(keyCode, isDown, time);
            public void PlayTouchInput(PlayInput.Finger finger, TimeUnit time) => player?.OnTouchInput(finger, time);
            public void PlayIndexedInput(int index, bool isDown, TimeUnit time) => player?.OnIndexedInput(index, isDown, time);

            public void PlayScoreUpdate(MetaTable<ScoreType> scoreSnapshot, TimeUnit time) => player?.OnScoreUpdate(scoreSnapshot, time);
            // ReSharper restore Unity.NoNullPropagation
        }
        
    }
}