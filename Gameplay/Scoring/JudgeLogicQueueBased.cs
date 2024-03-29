﻿// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Time;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Scoring {
    /// 基于队列的判定查询逻辑，根据队列是否为空来决定游戏是否结束
    /// 完全是特设功能，派生类需要知道这个类的完整实现，适用于大部分模式的判定
    public abstract class JudgeLogicQueueBased : JudgeLogicBase {
        /// 还未进入判定范围的音符队列，按顺序检查进入ActiveNoteEarlyWindow范围并踢到activeNotes中。
        private readonly QueueList<NoteCarrier> pendingNotes = new QueueList<NoteCarrier>();
        /// 进入判定范围的所有音符，会被检查是否退出了judgeWindowLate指定的时间范围
        private readonly Queue<NoteCarrier> activeNotes = new Queue<NoteCarrier>(1000);

        protected readonly struct ReadOnlyQueue<T> : IReadOnlyCollection<T> {
            private readonly Queue<T> inner;
            public ReadOnlyQueue(Queue<T> inner) { this.inner = inner; }

            public Queue<T>.Enumerator GetEnumerator() => inner.GetEnumerator();
            public int Count => inner.Count;
            
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)inner).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)inner).GetEnumerator();
        }
        
        /// 用于判定时遍历枚举的容器
        protected ReadOnlyQueue<NoteCarrier> ActiveNotes => new ReadOnlyQueue<NoteCarrier>(activeNotes);
        
        // 让activeList里的音符离miss边界远一些。100ms的额外边界暂时足够了。
        // todo: 为IJudgeState增加接口来表示判定是否处理完成，而非用固定的WindowOffset来保证完成判定处理
        private readonly TimeUnit activeNoteWindowOffset = TimeUnit.FromMilliseconds(100);
        
        // todo: 这些应该移动到一个专用的ModeRule工厂类中，从外部inject进来这些实例依赖（现在这样写只是为了方便重构）
        protected abstract IJudgeTiming CreateJudgeTiming(IPlayInfo playInfo);
        protected abstract IScore CreateScore(IPlayInfo playInfo);

        /// 将NoteCarrier加入activeList的时机（相对于PlayTime.JudgeTime）
        protected virtual TimeUnit ActiveNoteEarlyWindow => Timing.WindowEarly.OffsetBy(activeNoteWindowOffset);
        /// 将NoteCarrier移除出activeList的时机（相对于PlayTime.JudgeTime）
        protected virtual TimeUnit ActiveNoteLateWindow => Timing.WindowLate.OffsetBy(activeNoteWindowOffset);

        protected abstract void ResetJudge(IPlayInfo playInfo);
        protected abstract void UpdateJudge(TimeUnit judgeTimeStart, TimeUnit judgeTimeEnd);

        public override bool IsFinished => (pendingNotes == null || !pendingNotes.HasNext) && activeNotes.Count == 0;
        public override bool IsFailed {
            get {
                var failed = Score.GetValue(ScoreType.IsFailed);
                var hp = Score.GetValue(ScoreType.HP);
                return failed.IsBoolean ? failed.Bool : hp.Double <= 0;
            }
        }

        public sealed override void OnLoadChart(IPlayInfo playInfo, Processor.Processor processor) {
            // TODO: 从外部inject Score和Timing对象（如实现一个抽象工厂GameRule类），而不是令派生类自行构建
            
            Score = CreateScore(playInfo);
            Timing = CreateJudgeTiming(playInfo);
            
            Score.Init(playInfo);
            Timing.Init(playInfo);
            
            pendingNotes.ClearAndRestart();
            activeNotes.Clear();
            
            pendingNotes.AddRange(processor.ResultNoteList.Where(carrier => carrier.judgeState != null));

            ResetJudge(playInfo);
        }
        
        public sealed override void OnUpdateLogicBeforeInput(TimeUnit judgeTimeBeforeInput, TimeUnit judgeTimeAfterInput) {
            PopulateActiveListUntil(judgeTimeAfterInput);
        }
        public sealed override void OnUpdateLogicAfterInput(TimeUnit judgeTimeBeforeInput, TimeUnit judgeTimeAfterInput) {
            UpdateJudge(judgeTimeBeforeInput, judgeTimeAfterInput);
            DepopulateActiveListUntil(judgeTimeBeforeInput);
        }
        
        // todo: 能不能把区间查找做进一种队列性质的helper容器，而非中间类？
        protected void DepopulateActiveListUntil(TimeUnit judgeTime) {
            // todo: 这里应当以某个“最后一次处理输入消息的时间”值为标准弹出note
            while (activeNotes.Count > 0) {
                var carrier = activeNotes.Peek();
                if (carrier.EndTime > judgeTime.Seconds - ActiveNoteLateWindow.Seconds)
                    break;
                activeNotes.Dequeue();
            }
        }

        protected void PopulateActiveListUntil(TimeUnit judgeTime) {
            while(pendingNotes.HasNext) {
                var carrier = pendingNotes.Peek();
                if (carrier.StartTime > judgeTime.Seconds + ActiveNoteEarlyWindow.Seconds) break;
                pendingNotes.Skip();
                activeNotes.Enqueue(carrier);
            }
        }
    }
}