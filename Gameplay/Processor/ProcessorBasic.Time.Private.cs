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
            Effect[] effects;
            if (isStart) {
                // 优化：若传入的effect无尾，移除无尾的同类型effect
                effects = relative.effects.Where(x => effect.HasEnd || x.HasEnd || x.type != effect.type).Append(effect).ToArray();
            } else {
                // 只需要将自己移除
                effects = relative.effects.Where(x => x != effect).ToArray();
            }

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
            time.speed = CalculateTimeCarrierSpeed(time);
            time.start.displayY = CalculateYFromTime(time.StartTime, relative ?? time) + time.jumpTime * time.speed;
            time.noteVelocityScale = (adjustHS && scrollConstant) ? (refBeatLen / time.tempo.beatLength) : 1;
        }

        private void ApplyRangedEffectToTimeCarrier(TimeCarrier time, Effect effect) {
            switch (effect.type.Value) {
            case EffectType.ScrollSpeed: time.scrollVelocity = effect.value.Double; break;
            case EffectType.NoteSpeed: time.noteVelocity = effect.value.Double; break;
            case EffectType.Chorus: time.chorus = effect.value.Bool; break;
            }
        }

        private void ApplyMarginEffectToTimeCarrier(TimeCarrier time, Effect effect, bool isStart) {
            switch (effect.type.Value) {
            case EffectType.ScrollJump:
                if (isStart) {
                    time.jumpTime += effect.value.Double;
                } else {
                    time.jumpTime -= effect.value.Double;
                }
                break;
            }
        }

        private double CalculateTimeCarrierSpeed(TimeCarrier time) {
            if (scrollConstant && !forceScroll) return scaleY;
            if (scrollConstant) return time.scrollVelocity * scaleY;
            return time.scrollVelocity * refBeatLen * scaleY / time.tempo.beatLength;
        }

        private double CalculateRefBeatLen(double percentile = 0.667) {
            if (Tempos == null || Tempos.Count == 0)
                throw new ArgumentException("No time points passed to the processor");

            // 只有一个TimePoint，或者不知道音符范围的情况下，返回第一个beatLength
            int timePointCount = Tempos.Count;
            if (Objects == null || Objects.Count == 0 || timePointCount == 1)
                return Tempos[0].beatLength;

            double minOffset = Objects.Select(o => o.StartOrMin.Time.Seconds).Min();
            double maxOffset = Objects.Select(o => o.EndOrMax.Time.Seconds).Max();

            // 两个及以上的TimePoint
            // 把所有的beatLength与区间长度都扔进列表，然后按照大小排序
            List<Tuple<double, double>> beatLenList = new List<Tuple<double, double>>(timePointCount);
            beatLenList.Add(Tuple.Create(Tempos[0].beatLength, Math.Min(Math.Max(Tempos[1].Start.Time.Seconds, minOffset), maxOffset) - minOffset));
            for (int i = 1, len = timePointCount - 1; i < len; ++i)
            {
                double beginOffset = i == 0 ? minOffset : Math.Min(Math.Max(Tempos[i].Start.Time.Seconds, minOffset), maxOffset);
                double endOffset = i == timePointCount - 1 ? maxOffset : Math.Min(Math.Max(Tempos[i + 1].Start.Time.Seconds, minOffset), maxOffset);
                beatLenList.Add(Tuple.Create(Tempos[i].beatLength, endOffset - beginOffset));
            }
            var lastTimePoint = Tempos[timePointCount - 1]; // 因为至少有两个TimePoint，所以不会重复
            beatLenList.Add(Tuple.Create(lastTimePoint.beatLength, maxOffset - Math.Min(Math.Max(lastTimePoint.Start.Time.Seconds, minOffset), maxOffset)));
            beatLenList.Sort();

            // 取beatLength自大到小（BPM自小到大）的(percentile*offset)位置的beatLength作为结果
            double refPosition = (maxOffset - minOffset) * (1.0 - percentile);
            foreach (var tuple in beatLenList)
            {
                refPosition -= tuple.Item2;
                if (refPosition < 0)
                    return tuple.Item1;
            }
            return beatLenList[timePointCount - 1].Item1;
        }

        /// <summary>
        /// 为Carrier列表排序，使得有hs存在的情况下，音符按照排序后的顺序依次通过指定的卷轴位置。
        /// 排序算法为希尔排序（不稳定排序）。
        /// </summary>
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