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
using MaTech.Gameplay.Time;
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
        /// 用effect列表生成bar的beat位置，time是没有填写的。
        /// 输入的effect列表需要按照beat顺序排序，第一个signature表示bar的开始。
        /// </summary>
        public static List<BarInfo> CreateBars(List<Effect> effects, in Fraction endBeat, int maxBarCount = 99999) {
            // TODO: 封装成EffectTimeline
            // TODO: 处理Signature和ShowBar的End
            
            Fraction barLength = new Fraction(4);
            bool showBar = true;
            
            // Extract beat-signature and beat-showBar info from effects
            var listSign = new List<(Fraction, Fraction)>();
            var listShowBar = new List<(Fraction, bool)>();
            foreach (var effect in effects) {
                switch (effect.type.Value) {
                case EffectType.Signature when effect.Start == null: barLength = effect.value.Fraction; break;
                case EffectType.ShowBar when effect.Start == null: showBar = effect.value.Bool; break;
                case EffectType.Signature: listSign.Add((effect.Start.Beat, effect.value.Fraction)); break;
                case EffectType.ShowBar: listShowBar.Add((effect.Start.Beat, effect.value.Bool)); break;
                }
            }
            
            listSign.Add((Fraction.maxValue, Fraction.maxValue));
            listShowBar.Add((Fraction.maxValue, true));
            
            var listBar = new List<BarInfo>();
            
            Fraction nextSignBeat = listSign[0].Item1;
            Fraction nextShowBarBeat = listShowBar[0].Item1;
            int indexSign = 0;
            int indexShowBar = 0;
            
            for (Fraction currBeat = nextSignBeat; currBeat <= endBeat; currBeat += barLength) {
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
        public static List<BarInfo> CreateBars(List<TempoChange> tempos, List<Effect> effects, in Fraction endBeat, int maxBarCount = 99999) {
            List<BarInfo> bars = CreateBars(effects, endBeat, maxBarCount);
            if (tempos.Count == 0 || bars.Count == 0)
                return bars;

            int barCount = bars.Count;
            int barIndex = 0;
            
            // TODO: 封装成EffectTimeline与多次顺序插值方法
            // TODO: 导入Span.dll，用Span<BarInfo>和foreach ref修改bar数据

            var lastTempo = tempos.First();
            foreach (var tempo in tempos.Append(null)) {
                for (; barIndex < barCount; ++barIndex) {
                    var timePoint = bars[barIndex].timePoint;
                    if (timePoint.Beat.fraction >= (tempo?.Start ?? TimePoint.MaxValue).Beat.fraction)
                        break;
                    timePoint.Time = lastTempo.CalculateTimeFromBeat(timePoint.Beat);
                }
                if (barIndex >= barCount)
                    break;
                lastTempo = tempo!;
            }
            
            return bars;
        }

    }
}