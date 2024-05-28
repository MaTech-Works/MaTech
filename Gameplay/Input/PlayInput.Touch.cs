// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    /// <summary>
    /// Provide support for touch-screen based 2D analog input for game play.
    /// The position of touches are in pixel-coordinates.
    /// </summary>
    public partial class PlayInput {
        public enum FingerPhase {
            Idle = 0, Down, Up, Move
        }

        private static FingerPhase PhaseFromStates(bool oldState, bool newState) =>
            (FingerPhase)((oldState ? 2 : 0) + (newState ? 1 : 0));

        private bool touchEnabled = false;

        private const int touchCapacity = 10;
        
        private List<int> touchesToPress = new List<int>(touchCapacity);
        private List<int> touchesPressing = new List<int>(touchCapacity);
        private bool firstUpdateAfterEnable = false;

        private void ResetTouch() {
            foreach (int id in touchesPressing) {
                ForceReleaseFinger(id);
            }
            touchesPressing.Clear();
        }

        private void EnableTouch() {
            if (UnityUtil.IsUnassigned(touchReferenceCamera)) {
                Debug.Log("[PlayInput] Touch Reference Camera is unassigned; touch input will not be working until assigned.");
            }
            firstUpdateAfterEnable = true;
            touchEnabled = true;
        }

        private void DisableTouch() {
            touchEnabled = false;
        }

        private void UpdateTouch() {
            if (!touchEnabled) return;
            UpdateCachedTouchAreas();
            PollTouchFromUnityInput();
            UpdateTouchIDRecord();
        }

        private void UpdateFinger(int id, Vector2 position, bool isDown) {
            bool wasDown = touchesPressing.Contains(id);
            if (isDown) touchesToPress.Add(id);
            if (touchEnabled && (wasDown || isDown)) {
                SetFingerState(id, position, PhaseFromStates(wasDown, isDown));
            }
            if (!isDown) touchesPressing.Remove(id);
        }

        private void PollTouchFromUnityInput() {
            var touchCount = UnityEngine.Input.touchCount;
            for (int i = 0; i < touchCount; ++i){
                var touch = UnityEngine.Input.GetTouch(i);
                UpdateFinger(touch.fingerId, touch.position, IsFingerTouchDown(touch.phase));
            }
            
            #if UNITY_STANDALONE || UNITY_EDITOR
            if (simulateTouchWithMouse) {
                UpdateFinger(mouseStartTouchID + 0, UnityEngine.Input.mousePosition, UnityEngine.Input.GetMouseButton(0));
                UpdateFinger(mouseStartTouchID + 1, UnityEngine.Input.mousePosition, UnityEngine.Input.GetMouseButton(1));
            }
            #endif
        }

        private void UpdateTouchIDRecord() {
            foreach (int id in touchesToPress) {
                touchesPressing.Remove(id);
            }

            if (touchesPressing.Count > 0) {
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (!firstUpdateAfterEnable) {
                    Debug.LogError($"[PlayInput] We missed a finger release. Finger ID [{string.Join(", ", touchesPressing)}]");
                }
                #endif
                foreach (int id in touchesPressing) {
                    ForceReleaseFinger(id);
                }
                touchesPressing.Clear();
            }

            (touchesToPress, touchesPressing) = (touchesPressing, touchesToPress);
            firstUpdateAfterEnable = false;
        }
        
        private static bool IsFingerTouchDown(TouchPhase phase) {
            return phase == TouchPhase.Began || phase == TouchPhase.Moved || phase == TouchPhase.Stationary;
        }
        
    }
}