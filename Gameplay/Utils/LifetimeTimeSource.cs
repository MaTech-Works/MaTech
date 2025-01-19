// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Utils;
using MaTech.Gameplay.Time;
using UnityEngine;
using static MaTech.Common.Utils.UnityUtil;

namespace MaTech.Gameplay.Utils {
    public class LifetimeTimeSource : MonoBehaviour {
        [Serializable]
        public enum LifetimeStart {
            Awake, Start, OnEnable
        }

        public LifetimeStart lifetimeStart = LifetimeStart.OnEnable;
        public PlayTime.TimeSource timeSource = PlayTime.TimeSource.VisualTime;

        [Space]
        public UnityEventFloat onUpdate;
        public UnityEventFloat onLateUpdate;

        private TimeUnit timeStart;
        
        // todo: support loop time here rather than relying on curve methods; proceed timeStart ahead on each loop

        void Awake() { if (lifetimeStart is LifetimeStart.Awake) timeStart = PlayTime.Select(timeSource); }
        void Start() { if (lifetimeStart is LifetimeStart.Start) timeStart = PlayTime.Select(timeSource); }
        void OnEnable() { if (lifetimeStart is LifetimeStart.OnEnable) timeStart = PlayTime.Select(timeSource); }

        void Update() { onUpdate.Invoke((float)PlayTime.Select(timeSource).DeltaSince(timeStart).Seconds); }
        void LateUpdate() { onLateUpdate.Invoke((float)PlayTime.Select(timeSource).DeltaSince(timeStart).Seconds); }
    }
}