// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Gameplay.Logic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static MaTech.Common.Utils.UnityUtil;
using static MaTech.Gameplay.ChartPlayer;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Display {
    public class NoteEvents : NoteBehavior {
        public bool setActiveOnInitFinish = false;
        public bool clampToDisplayWindow = false;
        
        [Space]
        public Events events;
        public HitEventBinding[] hitEvents;
        
        #if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideLabel, InlineProperty]
        [FoldoutGroup("Last Hit", Expanded = false)]
        #endif
        public HitEvent LastHit { get; private set; }
        
        #if ODIN_INSPECTOR
        [InlineProperty, HideLabel]
        #endif
        [Serializable]
        public struct Events {
            [Serializable] public class RangeEvent : UnityEvent<float, float> { }
            [Serializable] public class LayerEvent : UnityEvent<NoteLayer> { }
            [Serializable] public class CarrierEvent : UnityEvent<NoteCarrier> { }
            
            [FormerlySerializedAs("onInstantiate")]
            [FoldoutGroup("Lifetime Events")] public UnityEvent onInit;
            [FormerlySerializedAs("onActivate")]
            [FoldoutGroup("Lifetime Events")] public UnityEvent onStart;
            [FormerlySerializedAs("onDeactivate")]
            [FoldoutGroup("Lifetime Events")] public UnityEvent onFinish;

            [FoldoutGroup("Ownership Events")] public LayerEvent onUpdateLayer;
            [FoldoutGroup("Ownership Events")] public CarrierEvent onUpdateCarrier;
            
            [FoldoutGroup("Positional Events")] public RangeEvent onUpdateRoll;
            [FoldoutGroup("Positional Events")] public RangeEvent onUpdateRatio;
            [FoldoutGroup("Positional Events")] public UnityEventFloat onUpdateRollStart;
            [FoldoutGroup("Positional Events")] public UnityEventFloat onUpdateRatioStart;
            [FoldoutGroup("Positional Events")] public UnityEventFloat onUpdateRollEnd;
            [FoldoutGroup("Positional Events")] public UnityEventFloat onUpdateRatioEnd;

            public void UpdateRange(((double start, double end) roll, (float start, float end) ratio) range) {
                onUpdateRoll.Invoke((float)range.roll.start, (float)range.roll.end);
                onUpdateRatio.Invoke(range.ratio.start, range.ratio.end);
                UpdateStart((range.roll.start, range.ratio.start));
                UpdateEnd((range.roll.end, range.ratio.end));
            }
            public void UpdateStart((double roll, float ratio) start) {
                onUpdateRollStart.Invoke((float)start.roll);
                onUpdateRatioStart.Invoke(start.ratio);
            }
            public void UpdateEnd((double roll, float ratio) end) {
                onUpdateRollEnd.Invoke((float)end.roll);
                onUpdateRatioEnd.Invoke(end.ratio);
            }

            public bool HasRange => onUpdateRoll.NotEmpty() || onUpdateRatio.NotEmpty();
            public bool HasStart => onUpdateRollStart.NotEmpty() || onUpdateRatioStart.NotEmpty();
            public bool HasEnd => onUpdateRollEnd.NotEmpty() || onUpdateRatioEnd.NotEmpty();

            public (bool start, bool end) NeedCalculation => HasRange ? (true, true) : (HasStart, HasEnd);
        }

        protected override void NoteInit() {
            events.onInit.Invoke();
        }

        protected override void NoteStart() {
            LastHit = HitEvent.Empty;
            events.onUpdateLayer.Invoke(Layer);
            events.onUpdateCarrier.Invoke(Carrier);
            if (setActiveOnInitFinish) gameObject.SetActive(true);
            events.onStart.Invoke();
        }

        protected override void NoteFinish() {
            events.onFinish.Invoke();
            if (setActiveOnInitFinish) gameObject.SetActive(false);
        }

        protected override void NoteUpdate() {
            var calc = events.NeedCalculation;
            var start = calc.start ? Layer.CalculateDeltaRollAndRatio(Carrier.StartRoll, Carrier, clampToDisplayWindow) : default;
            var end = calc.end ? Layer.CalculateDeltaRollAndRatio(Carrier.EndRoll, Carrier, clampToDisplayWindow) : default;
            if (calc is (true, true)) {
                events.onUpdateRoll.Invoke((float)start.roll, (float)end.roll);
                events.onUpdateRatio.Invoke(start.ratio, end.ratio);
            }
            if (calc is (true, _)) {
                events.onUpdateRollStart.Invoke((float)start.roll);
                events.onUpdateRatioStart.Invoke(start.ratio);
            }
            if (calc is (_, true)) {
                events.onUpdateRollEnd.Invoke((float)end.roll);
                events.onUpdateRatioEnd.Invoke(end.ratio);
            }
        }

        protected override void NoteHit(in HitEvent hitEvent) => hitEvents.InvokeAll(LastHit = hitEvent);
    }
}