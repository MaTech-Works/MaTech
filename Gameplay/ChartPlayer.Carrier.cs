// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Data;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        /// <summary> 图形或判定逻辑元素的三维时间轴位置 </summary>
        public struct CarrierTiming {
            /// <summary> 位于时间轴上的时间，以秒计 </summary>
            public double time;
            /// <summary> 位于图形卷轴上的位置，由Processor计算而来，由各模式自行定义 </summary>
            public double displayY;
            /// <summary> 位于节拍轴上的拍号，不一定严格对应时间，部分模式可能没有这个数值 </summary>
            public Fraction beat;

            public static CarrierTiming FromTimePoint(ITimePoint timePoint, double displayY = 0) {
                return new CarrierTiming() { time = timePoint.Time.Seconds, beat = timePoint.Beat.fraction, displayY = displayY };
            }

            public bool IsPositiveInfinity => beat == Fraction.maxValue || double.IsPositiveInfinity(time) || double.IsPositiveInfinity(time);
            public bool IsNegativeInfinity => beat == Fraction.minValue || double.IsNegativeInfinity(time) || double.IsNegativeInfinity(time);

            public static readonly CarrierTiming PositiveInfinity = new CarrierTiming() { time = double.PositiveInfinity, beat = Fraction.maxValue, displayY = double.PositiveInfinity };
            public static readonly CarrierTiming NegativeInfinity = new CarrierTiming() { time = double.NegativeInfinity, beat = Fraction.minValue, displayY = double.NegativeInfinity };
        }

        public abstract class Carrier : IComparable<Carrier> {
            // TODO: 增加不受speed scale影响的边缘时间margin

            public CarrierTiming start;
            public CarrierTiming end;
            
            public double StartTime => start.time;
            public double StartY => start.displayY;
            public Fraction StartBeat => start.beat;
            
            public double EndTime => end.time;
            public double EndY => end.displayY;
            public Fraction EndBeat => end.beat;

            public double LengthTime => end.time - start.time;
            public double LengthY => end.displayY - start.displayY;
            public Fraction LengthBeat => end.beat - start.beat;

            public static readonly Comparer<Carrier> ComparerStartOffset = Comparer<Carrier>.Create((x, y) => x.start.time.CompareTo(y.start.time));
            public static readonly Comparer<Carrier> ComparerStartY = Comparer<Carrier>.Create((x, y) => x.start.displayY.CompareTo(y.start.displayY));
            public static readonly Comparer<Carrier> ComparerStartBeat = Comparer<Carrier>.Create((x, y) => x.start.beat.CompareTo(y.start.beat));

            public static readonly Comparer<Carrier> ComparerEndOffset = Comparer<Carrier>.Create((x, y) => x.end.time.CompareTo(y.end.time));
            public static readonly Comparer<Carrier> ComparerEndY = Comparer<Carrier>.Create((x, y) => x.end.displayY.CompareTo(y.end.displayY));
            public static readonly Comparer<Carrier> ComparerEndBeat = Comparer<Carrier>.Create((x, y) => x.end.beat.CompareTo(y.end.beat));
            
            /// <summary>
            /// 默认的比较行为，以startOffset的大小为准。
            /// </summary>
            public int CompareTo(Carrier other) {
                return StartTime.CompareTo(other.StartTime);
            }
        }
    }
}