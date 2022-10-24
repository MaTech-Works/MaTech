// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个先进先出semaphore的实现.
    /// The implementation credits to:
    /// https://stackoverflow.com/questions/23415708/how-to-create-a-fifo-strong-semaphore
    /// </summary>
    public class SemaphoreFIFO {
        private SemaphoreSlim semaphore;

        private ConcurrentQueue<TaskCompletionSource<bool>> queue =
            new ConcurrentQueue<TaskCompletionSource<bool>>();

        public SemaphoreFIFO(int initialCount) {
            semaphore = new SemaphoreSlim(initialCount);
        }

        public void Wait() {
            WaitAsync().Wait();
        }

        public Task WaitAsync() {
            var tcs = new TaskCompletionSource<bool>(); // 创建一个task状态，用于生成成功获得semaphore的task
            queue.Enqueue(tcs); // 扔进队列里
            semaphore.WaitAsync().ContinueWith(t => { // 然后让semaphore多加一次叫号，不管是哪个线程反应过来的叫号，始终都是queue里下一个task出列
                TaskCompletionSource<bool> popped;
                if (queue.TryDequeue(out popped))
                    popped.SetResult(true);
            });
            return tcs.Task; // 把task交给外面
        }

        public void Release() {
            semaphore.Release();
        }
    }
}