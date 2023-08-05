// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace MaTech.Gameplay.Display {
    public abstract partial class PlayBehavior {
        private interface IBehaviorListMemberOperation {
            void TryAdd(PlayBehavior behavior);
            void TryRemove(PlayBehavior behavior);
        }

        public abstract class BehaviorList<T> : IEnumerable<T> {
            public Enumerator GetEnumerator() => new Enumerator(this);

            public async UniTask WhenAll(Func<T, UniTask> callback) => await UniTask.WhenAll(Enumerable.Select(this, callback));
            public void ForEach(Action<T> callback) {
                foreach (var t in this) callback(t);
            }

            public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator {
                private readonly BehaviorList<T> self;
                private List<T>.Enumerator inner;

                public Enumerator(BehaviorList<T> self) {
                    self.LockList();
                    this.self = self;
                    this.inner = self.EnumeratorFromList;
                }
                public void Dispose() => self.UnlockList();

                public T Current => inner.Current;
                object IEnumerator.Current => Current;

                public bool MoveNext() {
                    while (inner.MoveNext()) {
                        if (inner.Current is PlayBehavior)
                            return true;
                    }
                    return false;
                }

                public void Reset() => ((IEnumerator)inner).Reset();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            protected abstract void LockList();
            protected abstract void UnlockList();

            protected abstract List<T>.Enumerator EnumeratorFromList { get; }
        }

        private class MainThreadBehaviorList<T> : BehaviorList<T>, IBehaviorListMemberOperation {
            private readonly List<T> behaviors = new List<T>();
            private readonly List<T> cacheAddList = new List<T>();
            private readonly List<T> cacheRemoveList = new List<T>();
            private int readerCount = 0;

            private readonly Predicate<T> conditionRemove;

            public MainThreadBehaviorList(ICollection<IBehaviorListMemberOperation> targetOperationList, int initCapacity = 32) {
                behaviors.Capacity = initCapacity;
                targetOperationList.Add(this);
                conditionRemove = item => item == null || cacheRemoveList.Contains(item);
            }

            protected override List<T>.Enumerator EnumeratorFromList => behaviors.GetEnumerator();

            public virtual void TryAdd(PlayBehavior behavior) {
                if (!(behavior is T t)) return;
                if (readerCount == 0) {
                    behaviors.Add(t);
                } else {
                    cacheAddList.Add(t);
                    cacheRemoveList.Remove(t);
                }
            }
            public virtual void TryRemove(PlayBehavior behavior) {
                if (!(behavior is T t)) return;
                if (readerCount == 0) {
                    behaviors.Remove(t);
                } else {
                    cacheAddList.Remove(t);
                    cacheRemoveList.Add(t);
                }
            }

            protected override void LockList() {
                Assert.IsTrue(readerCount >= 0);
                readerCount += 1;
            }
            protected override void UnlockList() {
                readerCount -= 1;
                Assert.IsTrue(readerCount >= 0);
                if (readerCount == 0) FlushCachedLists();
            }

            private void FlushCachedLists() {
                if (cacheRemoveList.Count > 0) {
                    behaviors.RemoveAll(conditionRemove);
                    cacheRemoveList.Clear();
                }
                if (cacheAddList.Count > 0) {
                    behaviors.AddRange(cacheAddList);
                    cacheAddList.Clear();
                }
            }
        }

        private class ThreadSafeBehaviorList<T> : MainThreadBehaviorList<T> {
            private readonly object mutexReaderCount = new object();

            public ThreadSafeBehaviorList(ICollection<IBehaviorListMemberOperation> targetOperationList, int initCapacity = 32)
                : base(targetOperationList, initCapacity) { }

            public override void TryAdd(PlayBehavior behavior) {
                lock (mutexReaderCount) {
                    base.TryAdd(behavior);
                }
            }
            public override void TryRemove(PlayBehavior behavior) {
                lock (mutexReaderCount) {
                    base.TryRemove(behavior);
                }
            }

            protected override void LockList() {
                lock (mutexReaderCount) {
                    base.LockList();
                }
            }
            protected override void UnlockList() {
                lock (mutexReaderCount) {
                    base.UnlockList();
                }
            }
        }
    }
}