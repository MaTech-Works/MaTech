// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Display {
    // TODO: remove this class and always use NoteBehavior. see comments on BarCarrier
    public class BarBehavior : MonoBehaviour, IBarVisual {
        protected BarCarrier carrier;
        protected BarLayer layer;
        
        protected virtual bool UseGameObjectSetActive => true;

        protected virtual void Awake() {
            if (UseGameObjectSetActive) gameObject.SetActive(false);
        }
        
        public virtual void InitVisual(BarCarrier carrier, BarLayer layer) {
            this.carrier = carrier;
            this.layer = layer;
            if (UseGameObjectSetActive) gameObject.SetActive(true);
        }

        public virtual void FinishVisual() {
            if (UseGameObjectSetActive) gameObject.SetActive(false);
            carrier = null;
        }

        public virtual void UpdateVisual() {}

        public virtual bool IsVisualFinished => false;
        public virtual bool IgnoreDisplayWindow => false;

    }
}
