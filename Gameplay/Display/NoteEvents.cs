// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Tools;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Scoring;
using MaTech.Gameplay.Utils;
using UnityEngine;
using UnityEngine.Events;
using static MaTech.Common.Utils.UnityUtil;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Display {
    public class NoteEvents : MonoBehaviour, INoteVisual {
        [field: SerializeField, ReadOnlyInInspector]
        public NoteLayer Layer { get; private set; }
        public NoteCarrier Carrier { get; private set; }

        [field: SerializeField, ReadOnlyInInspector] public bool IsVisualFinished { get; set; } = false;
        [field: SerializeField, ReadOnlyInInspector] public bool IgnoreDisplayWindow { get; set; } = false;
        
        [Space]
        public bool clampToDisplayWindow = false;

        [Serializable]
        public struct Events {
            [Serializable]
            public class RangeEvent : UnityEvent<float, float> { }
            
            public UnityEvent onInstantiate;
            public UnityEvent onActivate;
            public UnityEvent onDeactivate;
            public RangeEvent onUpdateRatio;
            public RangeEvent onUpdateDeltaY;
            public UnityEventFloat onUpdateRatioStart;
            public UnityEventFloat onUpdateRatioEnd;
            public UnityEventFloat onUpdateDeltaYStart;
            public UnityEventFloat onUpdateDeltaYEnd;
        }

        public Events events;
        public HitEvent[] hitEvents;

        void Awake() => events.onInstantiate.Invoke();

        void IObjectVisual<NoteCarrier, NoteLayer>.InitVisual(NoteCarrier initCarrier, NoteLayer initLayer) {
            Carrier = initCarrier;
            Layer = initLayer;
            events.onActivate.Invoke();
        }

        void IObjectVisual<NoteCarrier, NoteLayer>.FinishVisual() {
            events.onDeactivate.Invoke();
            Carrier = null;
        }

        void IObjectVisual<NoteCarrier, NoteLayer>.UpdateVisual() {
            var (startRatio, endRatio) = Layer.CalculateRatioRange(Carrier, clampToDisplayWindow);
            var (startDeltaY, endDeltaY) = Layer.CalculateDeltaYRange(Carrier, clampToDisplayWindow);
            events.onUpdateRatio.Invoke(startRatio, endRatio);
            events.onUpdateDeltaY.Invoke((float)startDeltaY, (float)endDeltaY);
            events.onUpdateRatioStart.Invoke(startRatio);
            events.onUpdateRatioEnd.Invoke(endRatio);
            events.onUpdateDeltaYStart.Invoke((float)startDeltaY);
            events.onUpdateDeltaYEnd.Invoke((float)endDeltaY);
        }

        void INoteVisual.OnHit(IJudgeUnit unit, JudgeLogicBase.NoteHitAction action, in TimeUnit judgeTime, HitResult result) {
            foreach (var e in hitEvents) {
                e.InvokeIfMatch(action, result);
            }
        }
    }
}