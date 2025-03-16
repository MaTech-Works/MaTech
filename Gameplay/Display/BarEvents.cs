// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static MaTech.Common.Utils.UnityUtil;
using static MaTech.Gameplay.ChartPlayer;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Display {
    public class BarEvents : BarBehavior {
        public bool setActiveForLifetime = true;
        public bool clampToDisplayWindow = false;

        [Space]
        public Events events;
        
        #if ODIN_INSPECTOR
        [InlineProperty, HideLabel]
        #endif
        [Serializable]
        public struct Events {
            [Serializable] public class LayerEvent : UnityEvent<BarLayer> { }
            [Serializable] public class CarrierEvent : UnityEvent<BarCarrier> { }
            
            [FormerlySerializedAs("onInstantiate")]
            [FoldoutGroup("Lifetime Events")] public UnityEvent onInit;
            [FormerlySerializedAs("onActivate")]
            [FoldoutGroup("Lifetime Events")] public UnityEvent onStart;
            [FormerlySerializedAs("onDeactivate")]
            [FoldoutGroup("Lifetime Events")] public UnityEvent onFinish;
            
            [FoldoutGroup("Ownership Events")] public LayerEvent onUpdateLayer;
            [FoldoutGroup("Ownership Events")] public CarrierEvent onUpdateCarrier;
            
            [FoldoutGroup("Positional Events")] public UnityEventFloat onUpdateRoll;
            [FoldoutGroup("Positional Events")] public UnityEventFloat onUpdateRatio;
        }
        
        protected override void BarInit() {
            events.onInit.Invoke();
            if (setActiveForLifetime) gameObject.SetActive(false);
        }
        
        protected override void BarStart() {
            events.onUpdateLayer.Invoke(Layer);
            events.onUpdateCarrier.Invoke(Carrier);
            if (setActiveForLifetime) gameObject.SetActive(true);
            events.onStart.Invoke();
        }
        
        protected override void BarFinish() {
            events.onFinish.Invoke();
            if (setActiveForLifetime) gameObject.SetActive(false);
        }
        
        protected override void BarUpdate() {
            if (events.onUpdateRoll.NotEmpty()) {
                var deltaY = Layer.CalculateDeltaRoll(Carrier.StartRoll, Carrier, clampToDisplayWindow);
                events.onUpdateRoll.Invoke((float)deltaY);
            }
            if (events.onUpdateRatio.NotEmpty()) {
                var ratio = Layer.CalculateRatio(Carrier.StartRoll, Carrier, clampToDisplayWindow);
                events.onUpdateRatio.Invoke(ratio);
            }
        }
    }
}