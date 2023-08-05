// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using UnityEngine;

namespace MaTech.Common.Utils {
    public static class ThreadUtil {
        public static Thread MainThread { get; private set; }
        public static bool IsCurrentMainThread => Thread.CurrentThread.ManagedThreadId == MainThread.ManagedThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void FindUnityMainThread() {
            MainThread = Thread.CurrentThread;
        }
    }
}
