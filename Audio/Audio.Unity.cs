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
            if (Create(AudioSettings.outputSampleRate) != 0) {
                Debug.Log("Cannot create MaAudio. See AudioOutput.log for details.");
                return false;
            }
            
            #if UNITY_EDITOR
            EditorApplication.pauseStateChanged += onEditorPause;
            #endif
            
            Debug.Log("MaAudio created.");
            IsLoadedForUnity = true;
            return true;
        }

        public static void UnloadForUnity() {
            if (!IsLoadedForUnity) return;
            
            #if UNITY_EDITOR
            EditorApplication.pauseStateChanged -= onEditorPause;
            #endif
            
            Destroy();
            
            Debug.Log("MaAudio destroyed.");
            IsLoadedForUnity = false;
        }

        public static bool ReloadForUnity() {
            UnloadForUnity();
            return LoadForUnity();
        }

        #if MAAUDIO_LOAD_ON_STARTUP
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad() {
            ReloadForUnity();
            Application.quitting += UnloadForUnity;
            Application.focusChanged += focused => Paused = !focused;
        }
        #endif

        #if UNITY_EDITOR
        private static readonly Action<PauseState> onEditorPause = (state) => Paused = (state == PauseState.Paused);
        #endif
    }
}