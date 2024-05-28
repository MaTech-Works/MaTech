// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEngine;
#if UNITY_STANDALONE_WIN
using System.Collections.Generic;
using MaTech.Gameplay.Input.NativeInput;
#endif

namespace MaTech.Gameplay.Input {
    /// <summary>
    /// Provide support for distinctive button-like input for individual keys in game play.
    /// </summary>
    public partial class PlayInput {
        private KeyBinding keyBinding;
        public KeyBinding KeyBinding {
            get => keyBinding;
            set {
                keyBinding = value;
                OnKeyBindingChanged();
            }
        }

        private const int MaxKeyEnum = 512;
        
        private bool[] keyState;
        private void SetKeyState(KeyCode keyCode, bool isDown) {
            int i = (int)keyCode;
            lock (keyState) {
                if (!inputEnabled || i < 0 || i >= keyState.Length || keyState[i] == isDown) return;
                SendKeyInput(keyCode, isDown);
                keyState[i] = isDown;
            }
        }
        
        private bool[] indexState;
        private void SetIndexState(int index, bool isDown) {
            lock (indexState) {
                if (!inputEnabled || index < 0 || index >= keyCount || indexState[index] == isDown) return;
                if (isDown) {
                    IncreaseFingerCountAtIndex(index);
                } else {
                    DecreaseFingerCountAtIndex(index);
                }
                indexState[index] = isDown;
            }
        }
        
        private void ResetKeyState() {
            if (keyState != null) {
                for (int i = 0; i < keyState.Length; ++i) {
                    SetKeyState((KeyCode)i, false);
                }
            }
            if (indexState != null) {
                for (int i = 0; i < indexState.Length; ++i) {
                    SetIndexState(i, false);
                }
            }
            keyState = new bool[MaxKeyEnum];
            indexState = new bool[keyCount];
        }

        #if UNITY_STANDALONE_WIN
        private struct RawKeyInfo {
            public int index;
            public KeyCode keyCode;
        }
        private Dictionary<RawKey, RawKeyInfo> dictRawKey = new Dictionary<RawKey, RawKeyInfo>();

        private bool hasLastInput = false;
        private float lastRawInputTime = 0;

        private bool isUnityInputActive = false;

        private void OnRawKey(RawKey rawKey, bool isDown) {
            if (!isUnityInputActive) return;
            hasLastInput = true;
            if (dictRawKey.TryGetValue(rawKey, out RawKeyInfo info)) {
                SetIndexState(info.index, isDown);
                SetKeyState(info.keyCode, isDown);
            }
        }
        #endif

        private void ResetKey() {
            ResetKeyState();
        }

        private void EnableKey() {
            OnKeyBindingChanged();
            
            #if UNITY_STANDALONE_WIN
            RawInput.OnKey += OnRawKey;
            RawInput.Start();
            #endif
        }

        private void DisableKey() {
            #if UNITY_STANDALONE_WIN
            RawInput.Stop();
            RawInput.OnKey -= OnRawKey;
            #endif
        }
        
        private void OnKeyBindingChanged() {
            #if UNITY_STANDALONE_WIN
            Dictionary<RawKey, RawKeyInfo> newDictRawKey = new Dictionary<RawKey, RawKeyInfo>(keyCount);
            for (int i = 0; i < keyCount; ++i) {
                newDictRawKey[keyBinding.RawKeyAt(i)] = new RawKeyInfo { index = i, keyCode = keyBinding.KeyCodeAt(i) };
            }
            dictRawKey = newDictRawKey;
            #endif
        }
        
        // 和RawKey一起生效时，作为fallback逻辑处理不支持的键位，或者丢失键盘消息时以一帧的延迟复位键盘状态
        private void UpdateKey() {
            #if UNITY_STANDALONE_WIN
            if (hasLastInput) lastRawInputTime = UnityEngine.Time.realtimeSinceStartup;
            if (UnityEngine.Time.realtimeSinceStartup < lastRawInputTime + 0.1) return; 
            isUnityInputActive = Application.isFocused;
            #endif
            for (int i = 0; i < keyCount; ++i) {
                KeyCode keyCode = keyBinding.KeyCodeAt(i);
                bool isDown = false;
                if (keyCode >= KeyCodes.JoystickAxis1P) {
                    var d = (keyCode - KeyCodes.JoystickAxis1P);
                    var val = UnityEngine.Input.GetAxisRaw($"A{d / 2 + 1}");
                    if (d % 2 == 1 && val < -0.05f) {
                        isDown = true;
                    }else if (d % 2 == 0 && val > 0.05f) {
                        isDown = true;
                    }
                } else {
                    isDown = UnityEngine.Input.GetKey(keyCode);
                }
                SetIndexState(i, isDown);
                SetKeyState(keyCode, isDown);
            }
        }
    }
}