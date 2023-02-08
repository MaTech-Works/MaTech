// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;

namespace MaTech.Common.Unity {
    /// <summary>
    /// 一个根据元素指定的延时苏醒时刻，在外部传入某个时刻，超过这个苏醒时刻值时，对元素调用回调的调度器。
    /// </summary>
    public class TimedUpdateScheduler<T, TTime> where TTime : IComparable<TTime> {
        private class TimeInfo : IComparable<TimeInfo> {
            public TTime wakeTime;
            public T obj;
            public int CompareTo(TimeInfo info) => wakeTime.CompareTo(info.wakeTime);
        }

        private readonly Action<T> awake;
        private readonly PriorityQueue<TimeInfo> queue;
        private readonly ObjectPool<TimeInfo> pool;
        
        public TimedUpdateScheduler(Action<T> onAwake, int initCapacity = 0) {
            awake = onAwake;
            queue = new PriorityQueue<TimeInfo>(initCapacity);
            pool = new ObjectPool<TimeInfo>(() => new TimeInfo(), initCapacity);
        }

        public void Add(T obj, TTime wakeTime) {
            var info = pool.Get();
            info.wakeTime = wakeTime;
            info.obj = obj;
            queue.Push(info);
        }

        public void Update(TTime time) {
            while (queue.HasNext) {
                var info = queue.Peek();
                if (info.wakeTime.CompareTo(time) <= 0) {
                    queue.Pop();
                    awake?.Invoke(info.obj);
                }
            }
        }
    }
}