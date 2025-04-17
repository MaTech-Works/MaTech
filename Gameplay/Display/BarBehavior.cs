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
    public abstract class BarBehavior : MonoBehaviour, INoteVisual {
        #if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideLabel, InlineProperty]
        [FoldoutGroup("Carrier", Expanded = false, VisibleIf = "@Carrier!=null")]
        #endif
        public NoteCarrier Carrier { get; private set; }
        public NoteLayer Layer { get; private set; }
        
        [field: SerializeField, ReadOnlyInInspector] public bool IsVisualFinished { get; set; } = false;
        [field: SerializeField, ReadOnlyInInspector] public bool IgnoreDisplayWindow { get; set; } = false;

        protected virtual void BarInit() => gameObject.SetActive(false);
        protected virtual void BarStart() => gameObject.SetActive(true);
        protected virtual void BarFinish() => gameObject.SetActive(false);
        protected virtual void BarUpdate() { }

        void Awake() => BarInit();
        
        void INoteVisual.StartVisual(NoteCarrier carrier, NoteLayer layer) {
            Carrier = carrier;
            Layer = layer;
            BarStart();
        }
        void INoteVisual.FinishVisual() {
            BarFinish();
            Carrier = null;
        }
        void INoteVisual.UpdateVisual() => BarUpdate();
    }
}
