// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Tools;
using MaTech.Gameplay.Scoring;
using MaTech.Gameplay.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static MaTech.Common.Utils.UnityUtil;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Display {
    public class BarEvents : MonoBehaviour, IBarVisual {
        [field: SerializeField, ReadOnlyInInspector]
        public BarLayer Layer { get; private set; }
        public BarCarrier Carrier { get; private set; }

        [field: SerializeField, ReadOnlyInInspector] public bool IsVisualFinished { get; set; } = false;
        [field: SerializeField, ReadOnlyInInspector] public bool IgnoreDisplayWindow { get; set; } = false;
        
        [Space]
        public bool clampToDisplayWindow = false;

        [Serializable]
        public struct Events {
            public UnityEvent onInstantiate;
            public UnityEvent onActivate;
            public UnityEvent onDeactivate;
            public UnityEventFloat onUpdateRatio;
            public UnityEventFloat onUpdateDeltaY;
        }

        public Events events;

        void Awake() => events.onInstantiate.Invoke();

        void IObjectVisual<BarCarrier, BarLayer>.InitVisual(BarCarrier initCarrier, BarLayer initLayer) {
            Carrier = initCarrier;
            Layer = initLayer;
            events.onActivate.Invoke();
        }

        void IObjectVisual<BarCarrier, BarLayer>.FinishVisual() {
            events.onDeactivate.Invoke();
            Carrier = null;
        }

        void IObjectVisual<BarCarrier, BarLayer>.UpdateVisual() {
            var ratio = Layer.CalculateRatio(Carrier.StartRoll, Carrier, clampToDisplayWindow);
            var deltaY = Layer.CalculateDeltaY(Carrier.StartRoll, Carrier, clampToDisplayWindow);
            events.onUpdateRatio.Invoke(ratio);
            events.onUpdateDeltaY.Invoke((float)deltaY);
        }
    }
}