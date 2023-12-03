// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using UnityEngine;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        public enum BusyReason {
            Idle, Loading, Unloading
        }

        public bool IsBusy => BusyState != BusyReason.Idle;
        public BusyReason BusyState { get; private set; }

        private BusyLock SetBusy(BusyReason reason) {
            BusyState = reason;
            return new BusyLock(this);
        }

        private bool CheckBusy(string methodNameInLog = null) {
            if (IsBusy) {
                if (methodNameInLog != null)
                    Debug.LogError($"<b>[ChartPlayer]</b> Still busy ({BusyState.ToString()}), cannot proceed to method \"{methodNameInLog}\" now.", this);
                return true;
            }
            return false;
        }

        private readonly struct BusyLock : IDisposable {
            private readonly ChartPlayer self;
            public BusyLock(ChartPlayer self) => this.self = self;
            public void Dispose() => self.BusyState = BusyReason.Idle;
        }
    }
}