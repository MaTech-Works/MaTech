// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MaTech.Gameplay.Utils {
    [ExecuteAlways]
    public class LifetimeEvents : MonoBehaviour {
        [InlineButton("Awake", "Trigger")] public UnityEvent onAwake;
        [InlineButton("Start", "Trigger")] public UnityEvent onStart;
        [InlineButton("OnEnable", "Trigger")] public UnityEvent onEnable;
        [InlineButton("OnDisable", "Trigger")] public UnityEvent onDisable;
        [InlineButton("OnDestroy", "Trigger")] public UnityEvent onDestroy;
        
        void Awake() => onAwake?.Invoke();
        void Start() => onStart?.Invoke();
        void OnEnable() => onEnable?.Invoke();
        void OnDisable() => onDisable?.Invoke();
        void OnDestroy() => onDestroy?.Invoke();
    }
}