// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Tools;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Logic;
using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Display {
    public abstract class NoteBehavior : MonoBehaviour, INoteVisual, INoteHitEvent {
        #if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideLabel, InlineProperty]
        [FoldoutGroup("Carrier", Expanded = false, VisibleIf = "@Carrier!=null")]
        #endif
        public NoteCarrier Carrier { get; private set; }
        public NoteLayer Layer { get; private set; }

        [field: SerializeField, ReadOnlyInInspector(order = 100)] public bool IsVisualFinished { get; set; } = false;
        [field: SerializeField, ReadOnlyInInspector(order = 100)] public bool IgnoreDisplayWindow { get; set; } = false;

        protected virtual void NoteInit() => gameObject.SetActive(false);
        protected virtual void NoteStart() => gameObject.SetActive(true);
        protected virtual void NoteFinish() => gameObject.SetActive(false);
        protected virtual void NoteUpdate() { }
        protected virtual void NoteHit(in HitEvent hitEvent) {}

        void Awake() => NoteInit();

        void INoteVisual.StartVisual(NoteCarrier carrier, NoteLayer layer) {
            Carrier = carrier;
            Layer = layer;
            NoteStart();
        }
        void INoteVisual.FinishVisual() {
            NoteFinish();
            Carrier = null;
        }
        void INoteVisual.UpdateVisual() => NoteUpdate();

        void INoteHitEvent.OnHit(IJudgeUnit unit, JudgeLogicBase.NoteHitAction action, in TimeValue time, HitResult result) => NoteHit(new(unit, action, time, result));
    }
}
