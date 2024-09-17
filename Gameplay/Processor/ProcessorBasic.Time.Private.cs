// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using UnityEngine.Assertions;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Processor {
    public partial class ProcessorBasic {
        private TimeCarrier CreateTimeFromTempo(TempoChange tempo, TimeCarrier relative = null) {
            var result = new TimeCarrier() {
                start = CarrierTiming.FromTimePoint(tempo.Start),
                tempo = tempo,
            };
            if (relative != null) result.CopyEffectFrom(relative);
            UpdateTimeCarrierSpeedAndY(result, relative);
            return result;
        }

        private TimeCarrier CreateTimeWithInitEffects(TempoChange tempo, IEnumerable<Effect> initEffects) {
            var result = new TimeCarrier() {
                start = CarrierTiming.FromTimePoint(tempo.Start),
                tempo = tempo,
                effects = initEffects.ToArray(),
            };
            
            // 初始化时无需加入effect的【边缘效果】，只需要按顺序加入【区间效果】即可
            foreach (var e in result.effects) {
                Assert.IsNull(e.Start, "[Processor] TimeCarrier can only be initialized with null-start effects.");
                ApplyRangedEffectToTimeCarrier(result, e);
            }

            // 最后根据effect生效后的状态计算画面数据
            UpdateTimeCarrierSpeedAndY(result, null);

            return result;
        }

        private TimeCarrier CreateTimeFromEffect(Effect effect, TimeCarrier relative, bool isStart = true) {
            Assert.IsNotNull(relative, "[Processor] Need a valid relative to create a TimeCarrier for an effect.");
            Assert.IsFalse(isStart && effect.Start == null, "[Processor] Cannot process start of effect that has no start.");
            Assert.IsFalse(!isStart && effect.End == null, "[Processor] Cannot process end of effect that has no end.");

            var result = new TimeCarrier() {
                start = CarrierTiming.FromTimePoint(isStart ? effect.Start : effect.End),
                tempo = relative.tempo,
            };

            // 筛选当前生效的effect列表，生成一个静态的数组之后填写到TimeCarrier里
            // 优化：若传入的effect无尾，移除无尾的同类型effect，否则只需要将自己移除
            Effect[] effects = isStart ?
                relative.effects.Where(x => effect.HasEnd || x.HasEnd || x.type != effect.type).Append(effect).ToArray() :
                relative.effects.Where(x => x != effect).ToArray();

            // 计算当前所有effect的【区间效果】生效后的状态值
            if (isStart) {
                // 开始effect时只需要覆盖先前的值，然后加入【边缘效果】
                result.CopyEffectFrom(relative);
                ApplyRangedEffectToTimeCarrier(result, effect);
            } else {
                // 结束effect时需要根据仍然生效的其他effect重新计算状态值，然后加入取消effect的【边缘效果】
                foreach (var e in effects) {
                    ApplyRangedEffectToTimeCarrier(result, e);
                }
            }

            // 仅对本次改变生效状态的effect计算【边缘效果】
            ApplyMarginEffectToTimeCarrier(result, effect, isStart);

            // 最后根据effect生效后的状态计算画面数据
            UpdateTimeCarrierSpeedAndY(result, relative);

            // 填写effect记录
            result.effects = effects;
            result.effectActivated = isStart ? effect : null;
            result.effectDeactivated = isStart ? null : effect;

            return result;
        }

        private void UpdateTimeCarrierSpeedAndY(TimeCarrier time, TimeCarrier relative) {
            time.start.displayY = CalculateTimeCarrierDisplayY(time, relative ?? time);
            time.speed = CalculateTimeCarrierSpeed(time);
            time.noteScaleY = CalculateTimeCarrierNoteScaleY(time);
        }

        private void ApplyRangedEffectToTimeCarrier(TimeCarrier time, Effect effect) {
            switch (effect.type.Value) {
            case EffectType.ScrollSpeed: time.effectScrollSpeed = effect.value.Double; break;
            case EffectType.NoteSpeed: time.effectNoteSpeed = effect.value.Double; break;
            }
        }

        private void ApplyMarginEffectToTimeCarrier(TimeCarrier time, Effect effect, bool isStart) {
            switch (effect.type.Value) {
            case EffectType.ScrollJump:
                if (isStart) {
                    time.effectJumpTime += effect.value.Double;
                } else {
                    time.effectJumpTime -= effect.value.Double;
                }
                break;
            }
        }

        private double CalculateTimeCarrierDisplayY(TimeCarrier time, TimeCarrier relative) {
            return CalculateYFromTime(time.StartTime, relative) + time.effectJumpTime * time.speed;
        }
        
        private double CalculateTimeCarrierSpeed(TimeCarrier time) {
            double speed = applyScrollSpeedFromEffects ? time.effectScrollSpeed : 1;
            double scale = NeedScaleScrollSpeedToEvenBeat ? ReferenceBeatLength / time.tempo.beatLength : 1;
            return speed * scale * scaleY;
        }
        
        private double CalculateTimeCarrierNoteScaleY(TimeCarrier time) {
            double speed = applyNoteSpeedFromEffects ? time.effectNoteSpeed : 1;
            double scale = NeedScaleNoteSpeedToEvenBeat ? ReferenceBeatLength / time.tempo.beatLength : 1;
            return speed * scale;
        }

        private double CalculateScalingReferenceBPM() {
            Assert.IsNotNull(Tempos, "[Processor] Tempo list should not be null to calculate scaling reference BPM.");
            Assert.IsFalse(Tempos.Count == 0, "[Processor] Tempo list should not be empty to calculate scaling reference BPM.");

            int tempoCount = Tempos.Count;
            if (Objects == null || Objects.Count == 0 || tempoCount == 1)
                return Tempos[0].beatLength;

            double minTime = Objects.Select(o => o.StartOrMin.Time.Seconds).Min();
            double maxTime = Objects.Select(o => o.EndOrMax.Time.Seconds).Max();
            double GetClampedTime(TempoChange tempo) => Math.Min(Math.Max(tempo.Start.Time.Seconds, minTime), maxTime);

            List<(double bpm, double length)> sortedBPMs = new(tempoCount);
            for (int i = 0, n = tempoCount - 1; i <= n; ++i)
            {
                double timeBegin = i == 0 ? minTime : GetClampedTime(Tempos[i]);
                double timeEnd = i == n ? maxTime : GetClampedTime(Tempos[i + 1]);
                sortedBPMs.Add((Tempos[i].BPM, timeEnd - timeBegin));
            }
            sortedBPMs.Sort();

            double remainingLength = (maxTime - minTime) * scaleByTempoPercentile;
            foreach (var tuple in sortedBPMs)
            {
                remainingLength -= tuple.length;
                if (remainingLength < 0)
                    return tuple.bpm;
            }
            return sortedBPMs.Last().bpm;
        }

        /// <summary>
        /// 为Carrier列表排序，使得有hs存在的情况下，音符按照排序后的顺序依次通过指定的卷轴位置。
        /// 排序算法为希尔排序（不稳定排序）。
        /// </summary>
        /// <param name="list"> 被排序的列表 </param>
        /// <param name="sortDeltaY"> 为每个Carrier计入的排序提前量，排序的结果会在计入hs超车影响的情况下，在相对判定点的这个位置是有序的 </param>
        public static void SortCarriers<TCarrier, TLayer>(IList<TCarrier> list, double sortDeltaY)
            where TCarrier : ObjectCarrier<TCarrier, TLayer>
            where TLayer : ObjectLayer<TCarrier, TLayer> {
            if (list == null) return;

            // 计算提前量，并与Carrier成对放入数据列表
            int n = list.Count, i;
            List<SortData<TCarrier, TLayer>> sortList = new List<SortData<TCarrier, TLayer>>(n);
            for (i = 0; i < n; ++i) {
                var o = list[i];
                sortList.Add(new SortData<TCarrier, TLayer> {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    sortY = sortDeltaY == 0 ? o.StartY : o.StartY - sortDeltaY / o.scaleY,
                    carrier = o
                });
            }

            // 因为ProcessorBasic按照谱面顺序处理内容，所以输出的列表应当已经基本有序，用希尔排序来提高效率。
            ShellSort.Hibbard(sortList);

            // 往回写结果
            for (i = 0; i < n; ++i) {
                list[i] = sortList[i].carrier;
            }
        }

        private struct SortData<TCarrier, TLayer> : IComparable<SortData<TCarrier, TLayer>> 
            where TCarrier : ObjectCarrier<TCarrier, TLayer>
            where TLayer : ObjectLayer<TCarrier, TLayer> {
            public double sortY;
            public TCarrier carrier;
            public int CompareTo(SortData<TCarrier, TLayer> other) {
                return sortY.CompareTo(other.sortY);
            }
        }

    }
}