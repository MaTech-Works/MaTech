// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using UnityEngine.Assertions;

namespace MaTech.Gameplay.Processor {
    public static class BarUtil {
        public readonly struct BarInfo {
            public readonly TimePoint timePoint;
            public readonly bool hidden;

            internal BarInfo(in BeatUnit beat, in TimeUnit time, bool hidden) {
                this.timePoint = new TimePoint { Beat = beat, Time = time };
                this.hidden = hidden;
            }
        }
        
        /// <summary>
        /// 用effect列表生成bar的beat位置，time留空。
        /// 输入的effect列表需要按照beat顺序排序，第一个signature表示bar的开始。
        /// </summary>
        public static List<BarInfo> GenerateBarBeats(IList<Effect> effects, in FractionMixed endBeat, int maxBarCount = 99999) {
            // TODO: 封装成EffectTimeline
            // TODO: 处理Signature和ShowBar的End
            
            FractionMixed barLength = new FractionMixed(4);
            bool showBar = true;
            
            // Extract beat-signature and beat-showBar info from effects
            var listSign = new List<(FractionMixed, FractionMixed)>();
            var listShowBar = new List<(FractionMixed, bool)>();
            foreach (var effect in effects) {
                switch (effect.type.Value) {
                case EffectType.Signature when effect.Start == null: barLength = effect.value.start.Fraction; break;
                case EffectType.ShowBar when effect.Start == null: showBar = effect.value.start.Bool; break;
                case EffectType.Signature: listSign.Add((effect.Start.Beat, effect.value.start.Fraction)); break;
                case EffectType.ShowBar: listShowBar.Add((effect.Start.Beat, effect.value.start.Bool)); break;
                }
            }
            
            listSign.Add((FractionMixed.maxValue, FractionMixed.maxValue));
            listShowBar.Add((FractionMixed.maxValue, true));
            
            var listBar = new List<BarInfo>();
            
            FractionMixed nextSignBeat = listSign[0].Item1;
            FractionMixed nextShowBarBeat = listShowBar[0].Item1;
            int indexSign = 0;
            int indexShowBar = 0;
            
            for (FractionMixed currBeat = nextSignBeat; currBeat <= endBeat; currBeat += barLength) {
                while (nextSignBeat <= currBeat) {
                    // IMPORTANT: Reset currBeat to the beginning of signature changes;
                    // We restart the bar counting on each signature change.
                    currBeat = listSign[indexSign].Item1;
                    barLength = listSign[indexSign].Item2;
                    ++indexSign;
                    Assert.IsTrue(indexSign < listSign.Count);
                    nextSignBeat = listSign[indexSign].Item1;
                }
                while (nextShowBarBeat <= currBeat) {
                    showBar = listShowBar[indexShowBar].Item2;
                    ++indexShowBar;
                    Assert.IsTrue(indexShowBar < listShowBar.Count);
                    nextShowBarBeat = listShowBar[indexShowBar].Item1;
                }
                listBar.Add(new BarInfo(currBeat, TimeUnit.Zero, !showBar));
                if (listBar.Count >= maxBarCount)
                    return listBar;
            }
            
            return listBar;
        }
        
        /// <summary>
        /// 用effect和tempo列表生成bar的beat和offset。
        /// 输入的effect和tempo列表需要按照beat顺序排序。
        /// </summary>
        public static List<BarInfo> GenerateBars(IList<TempoChange> tempos, IList<Effect> effects, in FractionMixed endBeat, int maxBarCount = 99999) {
            List<BarInfo> bars = GenerateBarBeats(effects, endBeat, maxBarCount);

            int barCount = bars.Count;
            int barIndex = 0;
            
            // TODO: 封装成EffectTimeline与多次顺序插值方法
            // TODO: 导入Span.dll，用Span<BarInfo>和foreach ref修改bar数据

            TempoChange? lastTempo = null!;
            foreach (var tempo in tempos.Append(tempoAtInfinity)) {
                if (tempo is null) continue;
                lastTempo ??= tempo;
                for (; barIndex < barCount; ++barIndex) {
                    var timePoint = bars[barIndex].timePoint;
                    if (timePoint.Beat.Fraction >= (tempo?.Start ?? TimePoint.maxValue).Beat.Fraction)
                        break;
                    timePoint.Time = lastTempo.CalculateTimeFromBeat(timePoint.Beat);
                }
                if (barIndex >= barCount)
                    break;
                lastTempo = tempo;
            }
            
            return bars;
        }

        private static readonly TempoChange tempoAtInfinity = new(TimePoint.maxValue, double.PositiveInfinity);
    }
}