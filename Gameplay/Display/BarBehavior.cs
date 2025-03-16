// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Tools;
using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Display {
    // TODO: remove this class and always use NoteBehavior. see comments on BarCarrier
    public abstract class BarBehavior : MonoBehaviour, IBarVisual {
        #if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideLabel, InlineProperty]
        [FoldoutGroup("Carrier", Expanded = false, VisibleIf = "@Carrier!=null")]
        #endif
        public BarCarrier Carrier { get; private set; }
        public BarLayer Layer { get; private set; }
        
        [field: SerializeField, ReadOnlyInInspector] public bool IsVisualFinished { get; set; } = false;
        [field: SerializeField, ReadOnlyInInspector] public bool IgnoreDisplayWindow { get; set; } = false;

        protected virtual void BarInit() => gameObject.SetActive(false);
        protected virtual void BarStart() => gameObject.SetActive(true);
        protected virtual void BarFinish() => gameObject.SetActive(false);
        protected virtual void BarUpdate() { }

        void Awake() => BarInit();
        
        void IObjectVisual<BarCarrier, BarLayer>.StartVisual(BarCarrier carrier, BarLayer layer) {
            Carrier = carrier;
            Layer = layer;
            BarStart();
        }
        void IObjectVisual<BarCarrier, BarLayer>.FinishVisual() {
            BarFinish();
            Carrier = null;
        }
        void IObjectVisual<BarCarrier, BarLayer>.UpdateVisual() => BarUpdate();
    }
}
