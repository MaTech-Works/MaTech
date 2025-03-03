// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System.Collections.Generic;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using UnityEngine.UIElements;
using TimeUnit = MaTech.Gameplay.Data.TimeUnit;

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

            public static CarrierTiming FromTimePoint(ITimePoint timePoint, double roll = 0) {
                return new CarrierTiming() { time = timePoint.Time, beat = timePoint.Beat, roll = roll };
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
            public Fraction StartBeat => start.beat.Fraction;
            
            public double EndTime => end.time.Seconds;
            public double EndRoll => end.roll;
            public Fraction EndBeat => end.beat.Fraction;

            public double LengthTime => end.time.Seconds - start.time.Seconds;
            public double LengthRoll => end.roll - start.roll;
            public Fraction LengthBeat => end.beat.Fraction - start.beat.Fraction;
            
            public override string ToString() => $"{start} == {end}";
                
            public static Comparer<Carrier> ComparerStartTime => comparers[0];
            public static Comparer<Carrier> ComparerStartRoll => comparers[1];
            public static Comparer<Carrier> ComparerStartBeat => comparers[2];

            public static Comparer<Carrier> ComparerEndTime => comparers[3];
            public static Comparer<Carrier> ComparerEndRoll => comparers[4];
            public static Comparer<Carrier> ComparerEndBeat => comparers[5];

            private static readonly Comparer<Carrier>[] comparers = {
                Comparer<Carrier>.Create((x, y) => x.start.time.CompareTo(y.start.time)),
                Comparer<Carrier>.Create((x, y) => x.start.roll.CompareTo(y.start.roll)),
                Comparer<Carrier>.Create((x, y) => x.start.beat.CompareTo(y.start.beat)),
                Comparer<Carrier>.Create((x, y) => x.end.time.CompareTo(y.end.time)),
                Comparer<Carrier>.Create((x, y) => x.end.roll.CompareTo(y.end.roll)),
                Comparer<Carrier>.Create((x, y) => x.end.beat.CompareTo(y.end.beat)),
            };
        }
    }
}