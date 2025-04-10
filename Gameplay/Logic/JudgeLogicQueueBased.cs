// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Data;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Logic {
    /// 基于队列的判定查询逻辑，根据队列是否为空来决定游戏是否结束
    /// 完全是特设功能，派生类需要知道这个类的完整实现，适用于大部分模式的判定
    public abstract class JudgeLogicQueueBased : JudgeLogicBase {
        /// 还未进入判定范围的carrier队列，按顺序检查进入ActiveNoteEarlyWindow范围并踢到activeNotes中。
        private readonly QueueList<NoteCarrier> pendingCarriers = new();
        /// 进入判定范围的所有carrier，会被检查是否退出了judgeWindowLate指定的判定范围
        private readonly QueueList<NoteCarrier> activeCarriers = new(1000);
        
        /// 进入判定范围的所有carrier携带的unit，在carrier进入或退出activeCarriers容器时更新
        private readonly Dictionary<IJudgeUnit, HashSet<NoteCarrier>> activeUnitsWithCarriers = new(1000);
        private readonly HashSetPool<NoteCarrier> carrierHashSetPool = new HashSetPool<NoteCarrier>(1000, 100);

        // 让activeList里的音符离miss边界远一些。100ms的额外边界暂时足够了。
        // todo: 增加接口处理退出判定范围的IJudgeUnit
        // todo: 检查PlayControl与JudgeInput的操作顺序
        private readonly TimeUnit activeNoteWindowOffset = TimeUnit.FromMilliseconds(100);
        
        protected readonly struct ReadOnlyUnits : IReadOnlyCollection<IJudgeUnit> {
            private readonly Dictionary<IJudgeUnit, HashSet<NoteCarrier>>.KeyCollection inner;
            public ReadOnlyUnits(Dictionary<IJudgeUnit, HashSet<NoteCarrier>> dict) { this.inner = dict.Keys; }

            public Dictionary<IJudgeUnit, HashSet<NoteCarrier>>.KeyCollection.Enumerator GetEnumerator() => inner.GetEnumerator();
            public int Count => inner.Count;
            
            IEnumerator<IJudgeUnit> IEnumerable<IJudgeUnit>.GetEnumerator() => inner.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
        }
        
        /// 用于判定时遍历枚举的容器
        protected ReadOnlyUnits ActiveUnits => new(activeUnitsWithCarriers);
        
        // todo: 这些应该移动到一个专用的ModeRule工厂类中，从外部inject进来这些实例依赖（现在这样写只是为了方便重构）
        protected abstract IJudgeTiming CreateJudgeTiming(IPlayInfo playInfo);
        protected abstract IScore CreateScore(IPlayInfo playInfo);

        /// 将NoteCarrier加入activeList的时机（相对于PlayTime.InputTime）
        protected virtual TimeUnit ActiveNoteEarlyWindow => Timing.WindowEarly.OffsetBy(activeNoteWindowOffset);
        /// 将NoteCarrier移除出activeList的时机（相对于PlayTime.InputTime）
        protected virtual TimeUnit ActiveNoteLateWindow => Timing.WindowLate.OffsetBy(activeNoteWindowOffset);

        protected abstract void ResetJudge(IPlayInfo playInfo);
        protected abstract void UpdateJudge(TimeUnit timeStart, TimeUnit timeEnd);

        public override bool IsFinished => (pendingCarriers == null || !pendingCarriers.HasNext) && activeCarriers.Count == 0;
        public override bool IsFailed {
            get {
                var failed = Score.Get(ScoreType.IsFailed);
                var hp = Score.Get(ScoreType.HP);
                return failed.IsBoolean ? failed.Bool : hp.Double <= 0;
            }
        }

        public sealed override void OnLoadChart(IPlayInfo playInfo, QueueList<NoteCarrier> notes) {
            // TODO: 从外部inject Score和Timing对象（如实现一个抽象工厂GameRule类），而不是令派生类自行构建
            
            Score = CreateScore(playInfo);
            Timing = CreateJudgeTiming(playInfo);
            
            Score.ResetScore(playInfo);
            Timing.ResetScore(playInfo);
            
            pendingCarriers.Clear();
            activeCarriers.Clear();
            
            activeUnitsWithCarriers.ForEach(value: hashset => carrierHashSetPool.Recycle(hashset));
            activeUnitsWithCarriers.Clear();
            
            pendingCarriers.AddRange(notes.Where(carrier => carrier.UnitOf<IJudgeUnit>() != null).OrderBy(carrier => carrier, Carrier.ComparerStartTime()));

            ResetJudge(playInfo);
        }
        
        public sealed override void OnUpdateLogicBeforeInput(TimeUnit timeBeforeInput, TimeUnit timeAfterInput) {
            PopulateActiveListUntil(timeAfterInput);
        }
        public sealed override void OnUpdateLogicAfterInput(TimeUnit timeBeforeInput, TimeUnit timeAfterInput) {
            UpdateJudge(timeBeforeInput, timeAfterInput);
            DepopulateActiveListUntil(timeBeforeInput);
        }
        
        // todo: 提供一种队列进入退出标准的override方法
        // todo: 能不能把区间查找做进一种队列性质的helper容器，而非中间类？
        protected void DepopulateActiveListUntil(TimeUnit time) {
            TimeUnit window = time.OffsetBy(ActiveNoteLateWindow.Negate());
            while (activeCarriers.NextIf(window, (carrier, window) => carrier.end.time.CompareTo(window) <= 0) is { } carrier) {
                foreach (var unit in carrier.UnitsOf<IJudgeUnit>()) {
                    if (!activeUnitsWithCarriers.TryGetValue(unit, out var set)) continue;
                    set.Remove(carrier);
                    if (set.Count == 0) {
                        activeUnitsWithCarriers.Remove(unit);
                        carrierHashSetPool.Recycle(set);
                    }
                }
            }
            activeCarriers.RemovePassed();
        }

        protected void PopulateActiveListUntil(TimeUnit time) {
            TimeUnit window = time.OffsetBy(ActiveNoteEarlyWindow);
            while (pendingCarriers.NextIf(window, (carrier, window) => carrier.start.time.CompareTo(window) < 0) is { } carrier) {
                activeCarriers.OrderedInsert(carrier, Carrier.ComparerEndTime());
                foreach (var unit in carrier.UnitsOf<IJudgeUnit>()) {
                    if (!activeUnitsWithCarriers.TryGetValue(unit, out var set)) {
                        set = carrierHashSetPool.Get();
                        activeUnitsWithCarriers.Add(unit, set);
                    }
                    set.Add(carrier);
                }
            }
        }
    }
}