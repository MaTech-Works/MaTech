// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using UnityEngine.Assertions;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Processor {
    public partial class ProcessorBasic {
        private CarrierTiming CreateTiming(ITimePoint timePoint, TimeCarrier relative = null, in Variant keyword = default)
            => CarrierTiming.FromTimePoint(timePoint, relative?.SampleRoll(timePoint.Time));
        
        private TimeCarrier CreateTimeCarrierForTempo(TempoChange tempo, TimeCarrier relative = null, IEnumerable<Effect> effects = null) {
            double scale = ReferenceBeatLength / tempo.beatSeconds;
            var result = new TimeCarrier() {
                start = CreateTiming(tempo.SafeStart, relative),
                tempo = tempo,
                effects = effects?.ToLookup() ?? relative?.effects,
                scale = (NeedScaleScrollSpeedToEvenBeat ? scale : 1.0, NeedScaleNoteSpeedToEvenBeat ? scale : 1.0)
            };
            return result;
        }
        
        private TimeCarrier CreateTimeCarrier(ITimePoint timePoint, TimeCarrier relative = null, IEnumerable<Effect> effects = null) {
            var result = new TimeCarrier() {
                start = CreateTiming(timePoint, relative),
                tempo = relative?.tempo,
                effects = effects?.ToLookup() ?? relative?.effects,
                scale = relative?.scale ?? (1.0, 1.0)
            };
            return result;
        }
        
        private double CalculateScalingReferenceBPM() {
            Assert.IsNotNull(Tempos, "[Processor] Tempo list should not be null to calculate scaling reference BPM.");
            Assert.IsFalse(Tempos.Count == 0, "[Processor] Tempo list should not be empty to calculate scaling reference BPM.");

            int tempoCount = Tempos.Count;
            if (Objects == null || Objects.Count == 0 || tempoCount == 1)
                return Tempos[0].beatSeconds;

            double minTime = Objects.Select(o => o.SafeStart.Time.Seconds).Min();
            double maxTime = Objects.Select(o => o.SafeEnd.Time.Seconds).Max();
            double GetClampedTime(TempoChange tempo) => Math.Min(Math.Max(tempo.Start.Time.Seconds, minTime), maxTime);

            List<(double bpm, double length)> sortedTempos = new(tempoCount);
            for (int i = 0, n = tempoCount - 1; i <= n; ++i)
            {
                double timeBegin = i == 0 ? minTime : GetClampedTime(Tempos[i]);
                double timeEnd = i == n ? maxTime : GetClampedTime(Tempos[i + 1]);
                sortedTempos.Add((Tempos[i].BPM, timeEnd - timeBegin));
            }
            sortedTempos.Sort();

            double remainingLength = (maxTime - minTime) * scaleByTempoPercentile;
            foreach (var tuple in sortedTempos)
            {
                remainingLength -= tuple.length;
                if (remainingLength < 0)
                    return tuple.bpm;
            }
            return sortedTempos.Last().bpm;
        }

        /// <summary>
        /// 为Carrier列表排序，使得有hs存在的情况下，音符按照排序后的顺序依次通过指定的卷轴位置。
        /// 排序算法为希尔排序（不稳定排序）。
        /// </summary>
        /// <param name="list"> 被排序的列表 </param>
        /// <param name="byStart"> 按照开始还是结束边界排序 </param>
        /// <param name="windowDeltaRoll"> 为每个Carrier计入的排序提前量，排序的结果会在计入hs超车影响的情况下，在相对判定点的这个位置是有序的 </param>
        public static void SortCarriersByRoll(IList<NoteCarrier> list, bool byStart, double? windowDeltaRoll = null) {
            if (list == null) return;

            // 计算提前量，并与Carrier成对放入数据列表
            int n = list.Count, i;
            List<SortData> sortList = new List<SortData>(n);
            for (i = 0; i < n; ++i) {
                var o = list[i];
                sortList.Add(new SortData {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    sortRoll = windowDeltaRoll is null ? o.StartRoll : o.TargetRoll(windowDeltaRoll.Value, byStart),
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

        private struct SortData : IComparable<SortData> {
            public NoteCarrier carrier;
            public double sortRoll;
            public int CompareTo(SortData other) => sortRoll.CompareTo(other.sortRoll);
        }
    }
}