// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Algorithm {
	public static class FuncRAII {
        public static FuncRAII<T> Lock<T>(this T self, Action<T> actionLock, Action<T> actionUnlock) => new(self, actionLock, actionUnlock);
    }

    public readonly struct FuncRAII<T> : IDisposable {
		private readonly Action<T> actionUnlock;
		private readonly T self;

        public FuncRAII(in T self, Action<T> actionLock, Action<T> actionUnlock) {
            this.actionUnlock = actionUnlock;
            this.self = self;
            actionLock?.Invoke(self);
        }

        public void Dispose() {
            actionUnlock?.Invoke(self);
        }
    }
}