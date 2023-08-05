// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using UnityEngine;

namespace MaTech.Common.Unity {
    public class CachedValueByFrame<T> {
        private readonly Func<T> getter;
        private T value;

        public int CachedFrame { get; private set; }
        public T CachedValue {
            get {
                #if UNITY_EDITOR
                if ((Time.frameCount != CachedFrame || !Application.isPlaying) && getter != null)
                #else
                if (Time.frameCount != CachedFrame && getter != null)
                #endif
                {
                    value = getter();
                    CachedFrame = Time.frameCount;
                }
                return value;
            }
            set => this.value = value;
        }

        public void Expire() {
            CachedFrame = int.MinValue;
        }

        public CachedValueByFrame(Func<T> getter, T defaultValue = default) {
            this.getter = getter;
            this.value = defaultValue;
            CachedFrame = int.MinValue;
        }

        public override string ToString() {
            return CachedValue.ToString();
        }

        public static implicit operator T(CachedValueByFrame<T> c) => c == null ? default : c.CachedValue;
    }
}