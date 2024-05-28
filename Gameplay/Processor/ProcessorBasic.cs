// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Common.Tools;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Time;
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

        /// <summary> 计算各种速度时的参考用beatLength，由calcRefBeatLen()函数从所有的bpm中选择得到，除非必须切勿更改，有需要可以调整scaleY值 </summary>
        [SerializeField, ReadOnlyInInspector] protected double refBeatLen;
        /// <summary> Y值，卷轴速度与音符速度（也就是所有和逻辑距离有关的输出量）的整体缩放大小，默认是1 </summary>
        [SerializeField, ReadOnlyInInspector] protected double scaleY = 1;

        /// <summary> 将缩放前的卷轴速度强制固定为1 </summary>
        [SerializeField, ReadOnlyInInspector] protected bool scrollConstant;
        /// <summary> 在ScrollConstant为true时effect指定的scroll值仍然生效；ScrollConstant为false时无效 </summary>
        [SerializeField, ReadOnlyInInspector] protected bool forceScroll;
        /// <summary> 无视所有effect中的HS数据，但与adjustHS不冲突 </summary>
        [SerializeField, ReadOnlyInInspector] protected bool ignoreHS;
        /// <summary> 在ScrollConstant为true时，根据BPM大小调整HS，使得原本HS相同、beat间隔相同的两音符的显示间距相同；ScrollConstant为false时无效，因为此时音符的显示间隔是固定的 </summary>
        [SerializeField, ReadOnlyInInspector] protected bool adjustHS;

        /// <summary> 是否按照默认逻辑生成小节线信息，为false时将不会为<see cref="barList" />准备内容，完全依靠派生类向<see cref="barList" />的输出 </summary>
        [SerializeField] protected bool barEnabled = true;

        /// <summary> 是否按照默认逻辑生成曲目轨道，为false时将不会为<see cref="audioTrack" />准备内容，完全依靠派生类向<see cref="audioTrack" />的输出 </summary>
        [SerializeField] protected bool audioEnabled = true;
        /// <summary> 是否将音符的采样也加入到音轨里 </summary>
        [SerializeField] protected bool noteAsSample = true;
    
        // 仅在Process时有效的临时变量
        protected QueueList<TimeCarrier> timeList;
        protected QueueList<NoteCarrier> noteList;
        protected QueueList<BarCarrier> barList;
        
        public sealed override void Process() {
            ResultTimeList = null;
            ResultNoteList = null;
            ResultBarList = null;

            if (Tempos == null || Tempos.Count == 0)
                return;

            timeList = new QueueList<TimeCarrier>();
            noteList = new QueueList<NoteCarrier>();
            barList = null;
            
            // Initialize refBeatLen if not yet calculated
            if (refBeatLen <= 0)
                refBeatLen = CalculateRefBeatLen();

            try {
                OnPreProcess(); // PreProcess比ProcessTime更早，不能使用FindTimeCarrier系操作
                ProcessTime();
                ProcessObjects();
                ProcessBars();
                OnPostProcess();
            } catch (Exception ex) {
                Debug.LogError($"[Processor] Exception thrown when processing chart.");
                Debug.LogException(ex);
                return;
            }

            // Finally sort the objects
            SortCarriers<NoteCarrier, NoteLayer>(noteList, 0);
            SortCarriers<BarCarrier, BarLayer>(barList, 0);
        
            ResultTimeList = timeList;
            ResultNoteList = noteList;
            ResultBarList = barList;
        }

        private void ProcessTime() {
            Assert.IsTrue(Tempos.Count > 0);
            
            // TODO: 把代码封装成EffectTimeline

            // 按照beat顺序排序tempo队列，如果遇到负delay或者负bpm，不处理顺序问题
            var temposSorted = new QueueList<TempoChange>(Tempos.OrderBy(tp => tp, TimedObject.ComparerStartBeat));

            if (Effects is { Count: > 0 }) {
                // 将开头和结尾分别列出并排序，先是无头的开始，然后是剩下的，同beat先开始后结束
                var starts = Effects.Where(effect => effect.Start != null).Select(effect => (effect, beat: effect.Start.Beat, isStart: true));
                var ends = Effects.Where(effect => effect.End != null).Select(effect => (effect, beat: effect.End.Beat, isStart: false));
                var effectsSorted = starts.Concat(ends).OrderBy(tuple => tuple.beat).ThenBy(tuple => tuple.isStart ? 0 : 1).ToQueueList();

                timeList.Capacity = temposSorted.Count + effectsSorted.Count;
                
                // 为第一个tp前的effect提供参考
                var startingReferenceTimeCarrier = CreateTimeWithInitEffects(Tempos[0], Effects.Where(e => e.Start == null));

                var lastTimeCarrier = startingReferenceTimeCarrier;
                var nextTimeBeat = startingReferenceTimeCarrier.StartBeat;
                while (true) {
                    while (effectsSorted.HasNext && effectsSorted.Peek().beat < nextTimeBeat) {
                        var next = effectsSorted.Next();
                        timeList.Add(lastTimeCarrier = CreateTimeFromEffect(next.effect, lastTimeCarrier, next.isStart));
                    }

                    if (!temposSorted.HasNext) break;

                    timeList.Add(lastTimeCarrier = CreateTimeFromTempo(temposSorted.Next(), lastTimeCarrier));
                    nextTimeBeat = temposSorted.PeekOrDefault()?.Start.Beat ?? Fraction.maxValue;
                }
            } else {
                // 只有tp容器的话，直接简化处理
                timeList.Capacity = temposSorted.Count;

                TimeCarrier lastTimeCarrier = null;
                foreach (var tempo in temposSorted) {
                    lastTimeCarrier = CreateTimeFromTempo(tempo, lastTimeCarrier);
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
                
                while (indexNote < countNote && Objects[indexNote].StartOrMin.Time.OffsetBy(toleranceTimeOffset).Seconds < nextTime) {
                    OnProcessNote(Objects[indexNote]);
                    ++indexNote;
                }
            }
        }

        private void ProcessBars() {
            // TODO: 提供方法完成BarUtil.CreateBars到ObjectCarrier的自动转换，令派生类自行调用
            
            if (!barEnabled) return;
            if (Tempos == null || Effects == null) {
                Debug.LogError("<b>[Processor]</b> tempos and effects must be provided to calculate barlines");
                return;
            }

            if (Objects == null || Objects.Count == 0) {
                barList = new QueueList<BarCarrier>();
                return;
            }

            var endBeat = Objects.Select(o => (o.End ?? o.StartOrMin).Beat).Max();
            var bars = BarUtil.CreateBars(Tempos, Effects, endBeat);
            
            barList = new QueueList<BarCarrier>(bars.Count);
            foreach (var bar in bars) {
                if (bar.hidden) continue;
                var timing = CreateTiming(bar.timePoint);
                var timeCarrier = FindTimeCarrier(bar.timePoint);
                barList.Add(new BarCarrier() {
                    start = timing,
                    end = timing,
                    scaleY = timeCarrier.EffectiveNoteVelocity,
                });
            }
        }
        
        /// <summary>
        /// 实现这个函数来为每个音符根据PlayInfo指定的游玩方法生成NoteCarrier。
        /// </summary>
        /// <param name="note"> 当前处理的音符 </param>
        protected abstract void OnProcessNote(TimedObject note);

        /// <summary>
        /// 实现这个函数以便进行初始化，此时timeList仍然还没有内容，不能对三条时间轴进行采样。
        /// </summary>
        protected virtual void OnPreProcess() { }

        /// <summary>
        /// 实现这个函数以便进行结束时处理，此时noteList和barList还未排序，仍然可以无视顺序进行修改。
        /// </summary>
        protected virtual void OnPostProcess() { }

    }
}
