// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Common.Tools;
using MaTech.Common.Utils;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using UnityEngine;
using UnityEngine.Assertions;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Processor {
    /// <summary>
    /// 一个提供了基本的note处理功能的Processor类
    /// 
    /// 按照固定的行为一次性计算时间节点与小节线，使用双指针法计算非时间节点的图形位置，并且提供二分查找供后续计算使用。
    /// 提供一组可以调整的参数，可以改变计算的行为，也提供数个可供派生类重载/实现的虚函数，用于实现模式特有的音符相关计算以及填充填充extra数据供不同模式使用
    /// </summary>
    public abstract partial class ProcessorBasic : Processor {
        private readonly TimeUnit toleranceTimeOffset = TimeUnit.FromMilliseconds(0.99);

        /// <summary> 卷轴方向上所有位置与速度值的整体缩放比例 </summary>
        [SerializeField, ReadOnlyInInspector] protected double scaleY = 1;
        
        [Tooltip("是否使用Effect数据指定的NoteSpeed值")]
        [SerializeField, ReadOnlyInInspector] protected bool applyNoteSpeedFromEffects = true;
        [Tooltip("是否使用Effect数据指定的ScrollSpeed值")]
        [SerializeField, ReadOnlyInInspector] protected bool applyScrollSpeedFromEffects = true;

        [Serializable]
        protected enum ScaleByTempoMode {
            [Tooltip("不跟随BPM缩放，无effect时音符保持恒定与常量的运动速度，此时卷轴速度与物件速度在无SV等effect时，速率为每秒")]
            NoScaling = 0,
            [Tooltip("是否根据BPM大小缩放NoteSpeed，使无Effect时beat间隔相同的两音符的显示间距相同（类似于taiko在BPM变化时产生超车分层）")]
            ScaleNoteSpeedToEvenBeat,
            [Tooltip("是否根据BPM大小缩放ScrollSpeed，使无Effect时beat间隔相同的两音符的显示间距相同（类似于BMS在BPM变化时产生急停缓降）")]
            ScaleScrollSpeedToEvenBeat,
        }
        
        [SerializeField, ReadOnlyInInspector] protected ScaleByTempoMode scaleByTempo = ScaleByTempoMode.NoScaling;
        [Tooltip("缩放速度所参考的BPM，填0则使用下一个属性指定的中位数")]
        [SerializeField, ReadOnlyInInspector] protected double scaleByTempoBPM;
        [Tooltip("前一个属性填0时，参考此值选择全BPM在时间分布上从小到大的中位数（如0.667约为2/3中位数）")]
        [SerializeField, ReadOnlyInInspector] protected double scaleByTempoPercentile = 0.667;
        
        protected double ReferenceBeatLength { get; private set; }
        protected bool NeedScaleNoteSpeedToEvenBeat => scaleByTempo is ScaleByTempoMode.ScaleNoteSpeedToEvenBeat;
        protected bool NeedScaleScrollSpeedToEvenBeat => scaleByTempo is ScaleByTempoMode.ScaleScrollSpeedToEvenBeat;

        /// <summary> 是否按照默认逻辑生成小节线信息，为false时将不会为<see cref="barList" />准备内容，完全依靠派生类向<see cref="barList" />的输出 </summary>
        [SerializeField] protected bool barEnabled = true;

        // 仅在Process时有效的临时变量
        protected QueueList<TimeCarrier> timeList;
        protected QueueList<NoteCarrier> noteList;
        protected QueueList<BarCarrier> barList;
        
        public sealed override bool Process() {
            ResultTimeList = null;
            ResultNoteList = null;
            ResultBarList = null;

            if (Tempos == null || Tempos.Count == 0)
                return false;

            timeList = new QueueList<TimeCarrier>();
            noteList = new QueueList<NoteCarrier>();
            barList = null;

            if (scaleByTempoBPM == 0) scaleByTempoBPM = CalculateScalingReferenceBPM();
            ReferenceBeatLength = scaleByTempoBPM.Near(0) ? 1 : 1 / scaleByTempoBPM;

            try {
                OnPreProcess(); // PreProcess比ProcessTime更早，不能使用FindTimeCarrier系操作
                ProcessTime();
                ProcessObjects();
                ProcessBars();
                OnPostProcess();
            } catch (Exception ex) {
                Debug.LogError($"[Processor] Exception thrown when processing chart.");
                Debug.LogException(ex);
                return false;
            }
        
            ResultTimeList = timeList;
            ResultNoteList = noteList;
            ResultBarList = barList;
            return true;
        }

        private void ProcessTime() {
            Assert.IsTrue(Tempos.Count > 0);
            
            // TODO: 把代码封装成EffectTimeline

            // 按照beat顺序排序tempo队列，如果遇到负delay或者负bpm，不处理顺序问题
            var temposSorted = new QueueList<TempoChange>(Tempos.OrderBy(tp => tp, TimedObject.ComparerStartBeat));

            if (Effects is { Count: > 0 }) {
                timeList.Capacity = Tempos.Count + Effects.Count;
                
                var effectIndices = Effects.Select((effect, index) => (effect, index)).ToDictionary(t => t.effect, t => t.index);
                var effectIndexComparer = Comparer<Effect>.Create((a, b) => effectIndices[a].CompareTo(effectIndices[b]));
                
                var effectEdges = Effects.Where(effect => effect is not null)
                    .SelectMany((effect, index) => Enumerable.Repeat((index, effect), 2).Select((t, index) => (t.index, effect, isStart: index == 0)))
                    .Select(t => (t.index, t.effect, timePoint: t.isStart ? t.effect.Start : t.effect.End, t.isStart))
                    .Where(t => t.timePoint is not null)
                    .OrderBy(t => t.timePoint.Beat)
                    .ThenBy(t => t.index)
                    .ToQueueList();

                // todo: use effectEdges.GroupBy(beat) to avoid the while(effectEdges.GetNextOrDefault()) below
                
                var activeEffects = new List<Effect>(Effects.Where(e => e.Start == null));

                var initTimeCarrier = CreateTimeCarrierForTempo(Tempos[0], effects: activeEffects);
                var lastTimeCarrier = initTimeCarrier;
                
                while (temposSorted.NextOrDefault() is var tempo) {
                    var nextTempoTimePoint = tempo?.Start ?? TimePoint.MaxValue;
                    while (effectEdges.NextIf(t => t.timePoint.Beat.CompareTo(nextTempoTimePoint.Beat) <= 0) is { effect: not null, timePoint: var effectTimePoint } t) {
                        if (t.isStart) activeEffects.OrderedInsert(t.effect, effectIndexComparer);
                        else activeEffects.Remove(t.effect);
                        if (t.timePoint.Beat >= nextTempoTimePoint.Beat) break;
                        if (effectEdges.PeekIf(t => t.timePoint.Beat.CompareTo(effectTimePoint.Beat) <= 0) is { effect: not null }) continue;
                        timeList.Add(lastTimeCarrier = CreateTimeCarrier(t.timePoint, lastTimeCarrier, activeEffects));
                    }
                    if (tempo is null) break;
                    timeList.Add(lastTimeCarrier = CreateTimeCarrierForTempo(tempo, lastTimeCarrier, activeEffects));
                }
            } else {
                // 只有tp容器的话，直接简化处理
                timeList.Capacity = Tempos.Count;

                TimeCarrier lastTimeCarrier = null;
                foreach (var tempo in temposSorted) {
                    lastTimeCarrier = CreateTimeCarrierForTempo(tempo, lastTimeCarrier);
                    timeList.Add(lastTimeCarrier);
                }
            }
        }

        private void ProcessObjects() {
            if (Objects == null) return;
            
            int indexNote = 0, countNote = Objects.Count;
            int indexTime = 0, countTime = timeList.Count;
            for (; indexTime <= countTime; ++indexTime) {
                double nextTime = indexTime == countTime ? double.PositiveInfinity : timeList[indexTime].StartTime;
                // todo: 记录indexTime来优化FindTimeCarrier的二分查找，将时间复杂度系数从O(logN)降低到最差情况O(loglogN)
                
                while (indexNote < countNote && Objects[indexNote].SafeStart.Time.OffsetBy(toleranceTimeOffset).Seconds < nextTime) {
                    OnProcessNote(Objects[indexNote]);
                    ++indexNote;
                }
            }
        }

        private void ProcessBars() {
            // todo: 合并NoteLayer和BarLayer后，提供方法完成BarUtil.CreateBars到ObjectCarrier的自动转换，令派生类自行调用
            
            if (!barEnabled) return;
            if (Tempos == null || Effects == null) {
                Debug.LogError("<b>[Processor]</b> tempos and effects must be provided to calculate barlines");
                return;
            }

            if (Objects == null || Objects.Count == 0) {
                barList = new QueueList<BarCarrier>();
                return;
            }

            var endBeat = Objects.Select(o => (o.End ?? o.SafeStart).Beat).Max();
            var bars = BarUtil.GenerateBars(Tempos, Effects, endBeat);
            
            barList = new QueueList<BarCarrier>(bars.Count);
            foreach (var bar in bars) {
                if (bar.hidden) continue;
                var timeCarrier = FindTimeCarrier(bar.timePoint);
                var timing = CreateTiming(bar.timePoint, timeCarrier);
                barList.Add(new BarCarrier() {
                    start = timing,
                    end = timing,
                    scale = timeCarrier.scale.note,
                });
            }
        }
        
        /// <summary>
        /// 实现这个函数来为每个音符根据PlayInfo指定的游玩方法生成NoteCarrier。
        /// </summary>
        /// <param name="note"> 当前处理的音符 </param>
        protected abstract void OnProcessNote(TimedObject note);

        /// <summary>
        /// 实现这个函数以便进行初始化，此时timeList仍然还没有内容，不能采样effect。
        /// </summary>
        protected virtual void OnPreProcess() { }

        /// <summary>
        /// 实现这个函数以便进行结束时处理，此时noteList和barList还未排序，仍然可以无视顺序进行修改。
        /// </summary>
        protected virtual void OnPostProcess() { }

    }
}
