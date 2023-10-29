// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using UnityEditor;
using UnityEngine;

namespace MaTech.Audio {
    public static partial class MaAudio {
        public static bool IsLoadedForUnity { get; private set; } = false;

        public static bool LoadForUnity() {
            if (!Create(AudioSettings.outputSampleRate)) {
                Debug.Log("Cannot create MaAudio. See AudioOutput.log for details.");
                return false;
            }
            
            #if UNITY_EDITOR
            EditorApplication.pauseStateChanged += onEditorPause;
            #endif
            
            Application.quitting += UnloadForUnity;
            Application.focusChanged += OnFocusChanged;
            
            isFocusLost = !Application.isFocused;
            
            Debug.Log("MaAudio created.");
            IsLoadedForUnity = true;
            return true;
        }

        public static void UnloadForUnity() {
            if (!IsLoadedForUnity) return;
            
            #if UNITY_EDITOR
            EditorApplication.pauseStateChanged -= onEditorPause;
            #endif
            
            Application.quitting -= UnloadForUnity;
            Application.focusChanged -= OnFocusChanged;
            
            Destroy();
            
            Debug.Log("MaAudio destroyed.");
            IsLoadedForUnity = false;
        }

        public static bool ReloadForUnity() {
            UnloadForUnity();
            return LoadForUnity();
        }

        private static bool isFocusLost = false;
        private static bool isEditorPaused = false;
        
        private static void OnFocusChanged(bool focused) {
            isFocusLost = !focused;
            Paused = isFocusLost || isEditorPaused;
        }

        #if MAAUDIO_LOAD_ON_STARTUP
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad() => ReloadForUnity();
        #endif
        
        #if UNITY_EDITOR
        private static readonly Action<PauseState> onEditorPause = (state) => {
            isEditorPaused = (state == PauseState.Paused);
            Paused = isFocusLost || isEditorPaused;
        };
        #endif
    }
}