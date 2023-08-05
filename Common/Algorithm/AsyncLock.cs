// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MaTech.Common.Algorithm {
    public class AsyncLock : IDisposable {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public async UniTask<AsyncLock> LockAsync() {
            await semaphore.WaitAsync();
            return this;
        }

        public void Dispose() {
            semaphore.Release();
        }
    }
}