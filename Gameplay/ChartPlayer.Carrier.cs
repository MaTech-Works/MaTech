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
            /// <summary> Carrier的开始位置，用于剔除物体的显示 </summary>
            public CarrierTiming start;
            
            public double StartTime => start.time;
            public double StartY => start.displayY;
            public Fraction StartBeat => start.beat;
            
            public virtual double EndTime => start.time;
            public virtual double EndY => start.displayY;
            public virtual Fraction EndBeat => start.beat;

            public static readonly Comparer<Carrier> ComparerStartOffset = Comparer<Carrier>.Create((x, y) => x.start.time.CompareTo(y.start.time));
            public static readonly Comparer<Carrier> ComparerStartY = Comparer<Carrier>.Create((x, y) => x.start.displayY.CompareTo(y.start.displayY));
            public static readonly Comparer<Carrier> ComparerStartBeat = Comparer<Carrier>.Create((x, y) => x.start.beat.CompareTo(y.start.beat));

            public static readonly Comparer<Carrier> ComparerEndOffset = Comparer<Carrier>.Create((x, y) => x.EndTime.CompareTo(y.EndTime));
            public static readonly Comparer<Carrier> ComparerEndY = Comparer<Carrier>.Create((x, y) => x.EndY.CompareTo(y.EndY));
            public static readonly Comparer<Carrier> ComparerEndBeat = Comparer<Carrier>.Create((x, y) => x.EndBeat.CompareTo(y.EndBeat));
            
            /// <summary>
            /// 默认的比较行为，以startOffset的大小为准。
            /// </summary>
            public int CompareTo(Carrier other) {
                return StartTime.CompareTo(other.StartTime);
            }
        }

        public abstract class CarrierRanged : Carrier {
            /// <summary> Carrier的结束位置，用于剔除物体的显示 </summary>
            public CarrierTiming end;
            
            public override double EndTime => end.time;
            public override double EndY => end.displayY;
            public override Fraction EndBeat => end.beat;
        }
    }
}