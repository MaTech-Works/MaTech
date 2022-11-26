using System;
using System.Runtime.InteropServices;

namespace MaTech.Control.NativeInput {
    /// API design similar to UnityRawInput plugin, windows only
    public static partial class RawInput {
        public static event Action<RawKey, bool> OnKey;

        public static bool IsRunning { get; private set; }
        private static object mutex = new object();

        public static bool Start() {
            lock (mutex) {
                if (IsRunning) return false;
                return SetHook();
            }
        }

        public static void Stop() {
            lock (mutex) {
                RemoveHook();
            }
        }

        private static bool SetHook() {
            IsRunning = Private.HookKeyboard(KeyProc);
            if (IsRunning) {
                UnityEngine.Debug.Log($"Input thread is running");
            } else {
                UnityEngine.Debug.LogError($"Input thread failed to start");
            }
            return IsRunning;
        }

        private static void RemoveHook() {
            if (!IsRunning) return;
            Private.UnhookKeyboard();
            IsRunning = false;
        }

        [AOT.MonoPInvokeCallback(typeof(Private.CallbackKeyInput))]
        private static void KeyProc(uint vkCode, bool isDown) {
            OnKey?.Invoke((RawKey) vkCode, isDown);
        }

        private static class Private {
            private const string LIBNAME = "NativeInput.dll";

            public delegate void CallbackKeyInput(uint vkCode, bool isDown);

            #if UNITY_STANDALONE_WIN
            [DllImport(LIBNAME)] public static extern bool HookKeyboard(CallbackKeyInput onKeyInput);
            [DllImport(LIBNAME)] public static extern void UnhookKeyboard();
            #else
            public static bool HookKeyboard(CallbackKeyInput onKeyInput) => false;
            public static void UnhookKeyboard() {}
            #endif
        }
    }
}
