// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Common.Tools {
    public class DebugLogHistory : MonoBehaviour {
        private static readonly Dictionary<string, List<string>> history = new();
        private static readonly List<DebugLogHistory> instances = new();

        public static bool HasInstances => instances.Count > 0;
        public static bool IsActive => instances.Exists(o => o.isActiveAndEnabled);

        public static void PushHistory(string key, string text) {
            lock (history) {
                if (key == null) key = "";
                if (!history.ContainsKey(key))
                    history.Add(key, new List<string>(1000));
                history[key].Add(text);
            }
        }

        public static void ClearHistory(string key) {
            lock (history) {
                if (key == null) key = "";
                history[key] = new List<string>(1000);
            }
        }

        public string key;
        public int maxDisplayedCount = 10;
        public float delayPerLog = 0.1f;

        public UnityUtil.UnityEventString onUpdateLog;

        private int nextLogIndex = 0;
        private float nextLogTime = 0;

        void OnEnable() {
            nextLogTime = Time.unscaledTime;
        }

        void Awake() {
            instances.Add(this);
        }

        void OnDestroy() {
            instances.Remove(this);
        }

        void Update() {
            float targetTime = Time.unscaledTime;
            
            lock (history) {
                if (!history.ContainsKey(key))
                    return;

                bool logUpdated = false;
                if (nextLogIndex > history[key].Count) {
                    logUpdated = true;
                    nextLogIndex = history[key].Count;
                    nextLogTime = targetTime;
                } else while (nextLogIndex < history[key].Count && nextLogTime <= targetTime) {
                    logUpdated = true;
                    nextLogIndex += 1;
                    nextLogTime += delayPerLog;
                }

                if (logUpdated) {
                    int startLogIndex = Math.Max(0, nextLogIndex - maxDisplayedCount);
                    var logs = history[key].Skip(startLogIndex).Take(nextLogIndex - startLogIndex);
                    onUpdateLog.Invoke(string.Join("\n", logs));
                }

                if (nextLogTime < targetTime)
                    nextLogTime = targetTime;
            }
        }
    }
}