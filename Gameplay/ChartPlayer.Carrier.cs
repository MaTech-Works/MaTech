// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System.Collections.Generic;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        /// <summary> 图形或判定逻辑元素的三维时间轴位置 </summary>
        public struct CarrierTiming {
            /// <summary> 位于时间轴上的时间，以秒计 </summary>
            public TimeUnit time;
            /// <summary> 位于节拍轴上的拍号，不一定严格对应时间，部分模式可能没有这个数值 </summary>
            public BeatUnit beat;
            /// <summary> 位于图形卷轴上的位置，由Processor计算而来，由各模式自行定义 </summary>
            public double roll;

            public static CarrierTiming FromTimePoint(ITimePoint timePoint, double? roll = null) {
                return new CarrierTiming() { time = timePoint.Time, beat = timePoint.Beat, roll = roll ?? 0.0 };
            }

            public bool IsMax => beat.IsMax || time.IsMax || double.IsPositiveInfinity(roll);
            public bool IsMin => beat.IsMin || time.IsMin || double.IsNegativeInfinity(roll);

            public static CarrierTiming MaxValue => new() { time = TimeUnit.MaxValue, beat = BeatUnit.MaxValue, roll = double.PositiveInfinity };
            public static CarrierTiming MinValue => new() { time = TimeUnit.MinValue, beat = BeatUnit.MinValue, roll = double.NegativeInfinity };
            
            public override string ToString() => $"Beat {beat}, Time {time}, Roll {roll}";
        }

        public abstract class Carrier {
            public CarrierTiming start;
            public CarrierTiming end;
            
            public double StartTime => start.time.Seconds;
            public double StartRoll => start.roll;
            public FractionMixed StartBeat => start.beat.Fraction;
            
            public double EndTime => end.time.Seconds;
            public double EndRoll => end.roll;
            public FractionMixed EndBeat => end.beat.Fraction;

            public double LengthTime => end.time.Seconds - start.time.Seconds;
            public double LengthRoll => end.roll - start.roll;
            public FractionMixed LengthBeat => end.beat.Fraction - start.beat.Fraction;
            
            // todo: Range<U> and RollUnit (notice that multi-dimension roll can be supported as multiple tracks)
            // todo: reverse word order for naming, such as StartTime
            
            public override string ToString() => $"{start} == {end}";

            public static IComparer<Carrier> ComparerStartTime(bool aligned = true) => aligned ? Comparers.alignedStartTime : Comparers.preciseStartTime;
            public static IComparer<Carrier> ComparerStartBeat(bool aligned = true) => aligned ? Comparers.alignedStartBeat : Comparers.preciseStartBeat;
            public static IComparer<Carrier> ComparerStartRoll() => Comparers.preciseStartRoll;

            public static IComparer<Carrier> ComparerEndTime(bool aligned = true) => aligned ? Comparers.alignedEndTime : Comparers.preciseEndTime;
            public static IComparer<Carrier> ComparerEndBeat(bool aligned = true) => aligned ? Comparers.alignedEndBeat : Comparers.preciseEndBeat;
            public static IComparer<Carrier> ComparerEndRoll() => Comparers.preciseEndRoll;

            private static class Comparers {
                public static readonly IComparer<Carrier> alignedStartTime = Comparer<Carrier>.Create((x, y) => x.start.time.CompareTo(y.start.time, true));
                public static readonly IComparer<Carrier> alignedStartBeat = Comparer<Carrier>.Create((x, y) => x.start.beat.CompareTo(y.start.beat, true));
                
                public static readonly IComparer<Carrier> alignedEndTime = Comparer<Carrier>.Create((x, y) => x.end.time.CompareTo(y.end.time, true));
                public static readonly IComparer<Carrier> alignedEndBeat = Comparer<Carrier>.Create((x, y) => x.end.beat.CompareTo(y.end.beat, true));
                
                public static readonly IComparer<Carrier> preciseStartTime = Comparer<Carrier>.Create((x, y) => x.start.time.CompareTo(y.start.time, false));
                public static readonly IComparer<Carrier> preciseStartBeat = Comparer<Carrier>.Create((x, y) => x.start.beat.CompareTo(y.start.beat, false));
                public static readonly IComparer<Carrier> preciseStartRoll = Comparer<Carrier>.Create((x, y) => x.start.roll.CompareTo(y.start.roll));
                
                public static readonly IComparer<Carrier> preciseEndTime = Comparer<Carrier>.Create((x, y) => x.end.time.CompareTo(y.end.time, false));
                public static readonly IComparer<Carrier> preciseEndBeat = Comparer<Carrier>.Create((x, y) => x.end.beat.CompareTo(y.end.beat, false));
                public static readonly IComparer<Carrier> preciseEndRoll = Comparer<Carrier>.Create((x, y) => x.end.roll.CompareTo(y.end.roll));
            };
        }
    }
}