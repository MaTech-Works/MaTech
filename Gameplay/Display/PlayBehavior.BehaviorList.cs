// Copyright (c) 2024, LuiCat (as MaTech)
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

        internal abstract class BehaviorList<T> : IEnumerable<T> {
            public Enumerator GetEnumerator() => new Enumerator(this);

            public async UniTask WhenAll(Func<T, UniTask> callback) => await UniTask.WhenAll(Enumerable.Select(this, callback));
            public void ForEach(Action<T> callback) { foreach (var t in this) callback(t); }
            
            public abstract int ActiveCount { get; }
            public bool IsEmpty => ActiveCount == 0;

            /// <summary> 适用于using语句，在尚未发起枚举的情况下使用 </summary>
            public DisposableLock LockRAII() => new DisposableLock(this);

            public struct Enumerator : IEnumerator<T> {
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

            public struct DisposableLock : IDisposable {
                private BehaviorList<T> self;
                
                public DisposableLock(BehaviorList<T> self) {
                    self?.LockList();
                    this.self = self;
                }

                public void Unlock() {
                    self?.UnlockList();
                    self = null;
                }
                
                public void Dispose() => Unlock();
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
            
            public override int ActiveCount => behaviors.Count;

            public MainThreadBehaviorList(ICollection<IBehaviorListMemberOperation> targetOperationList, int initCapacity = 32) {
                behaviors.Capacity = initCapacity;
                targetOperationList.Add(this);
                conditionRemove = item => item == null || cacheRemoveList.Contains(item);
            }

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

            protected override List<T>.Enumerator EnumeratorFromList => behaviors.GetEnumerator();

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

            public override int ActiveCount {
                get {
                    lock (mutexReaderCount) {
                        return base.ActiveCount;
                    }
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
        
        private static readonly List<IBehaviorListMemberOperation> lists = new List<IBehaviorListMemberOperation>();
        
        // Thread safe
        internal static BehaviorList<IKeyInputEarly> ListKeyInputEarly { get; } = new ThreadSafeBehaviorList<IKeyInputEarly>(lists);
        internal static BehaviorList<ITouchInputEarly> ListTouchInputEarly { get; } = new ThreadSafeBehaviorList<ITouchInputEarly>(lists);
        internal static BehaviorList<IIndexedInputEarly> ListIndexedInputEarly { get; } = new ThreadSafeBehaviorList<IIndexedInputEarly>(lists);

        // Main thread only
        internal static BehaviorList<PlayBehavior> ListAll { get; } = new MainThreadBehaviorList<PlayBehavior>(lists);
        internal static BehaviorList<IKeyInput> ListKeyInput { get; } = new MainThreadBehaviorList<IKeyInput>(lists);
        internal static BehaviorList<ITouchInput> ListTouchInput { get; } = new MainThreadBehaviorList<ITouchInput>(lists);
        internal static BehaviorList<IIndexedInput> ListIndexedInput { get; } = new MainThreadBehaviorList<IIndexedInput>(lists);
        internal static BehaviorList<INoteHitResult> ListNoteHitResult { get; } = new MainThreadBehaviorList<INoteHitResult>(lists);
        internal static BehaviorList<IScoreUpdate> ListScoreUpdate { get; } = new MainThreadBehaviorList<IScoreUpdate>(lists);
        internal static BehaviorList<IScoreResult> ListScoreResult { get; } = new MainThreadBehaviorList<IScoreResult>(lists);

        protected virtual void OnEnable() {
            //Debug.Log($"PlayBehavior OnEnable: {this} #{GetInstanceID()}");
            foreach (var list in lists) {
                list.TryAdd(this);
            }
        }
        
        protected virtual void OnDisable() {
            //Debug.Log($"PlayBehavior OnDisable: {this} #{GetInstanceID()}");
            foreach (var list in lists) {
                list.TryRemove(this);
            }
        }
        
        // todo: remove virtual OnEnable/OnDisable probably with some event/injection or player loop delegate
    }
}