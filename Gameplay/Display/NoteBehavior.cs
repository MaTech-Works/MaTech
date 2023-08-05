// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Tools;
using MaTech.Gameplay.Scoring;
using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Display {
    public abstract class NoteBehavior : MonoBehaviour, INoteVisual {
        [SerializeField, ReadOnlyInInspector] protected NoteCarrier carrier;
        [SerializeField, ReadOnlyInInspector] protected NoteLayer layer;

        [field: SerializeField, ReadOnlyInInspector] public bool IsVisualFinished { get; set; } = false;
        [field: SerializeField, ReadOnlyInInspector] public bool IgnoreDisplayWindow { get; set; } = false;

        protected virtual void ActivateObject() => gameObject.SetActive(true);
        protected virtual void DeactivateObject() => gameObject.SetActive(false);
        protected abstract void UpdateObject();

        protected virtual void HitObject(JudgeLogicBase.NoteHitAction action, HitResult result) {}

        void Awake() => DeactivateObject();

        void IObjectVisual<NoteCarrier, NoteLayer>.InitVisual(NoteCarrier initCarrier, NoteLayer initLayer) {
            this.carrier = initCarrier;
            this.layer = initLayer;
            ActivateObject();
        }

        void IObjectVisual<NoteCarrier, NoteLayer>.FinishVisual() {
            DeactivateObject();
            carrier = null;
        }

        void IObjectVisual<NoteCarrier, NoteLayer>.UpdateVisual() => UpdateObject();

        void INoteVisual.OnHit(JudgeLogicBase.NoteHitAction action, HitResult result) => HitObject(action, result);
    }
}
