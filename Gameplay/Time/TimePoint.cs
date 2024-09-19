// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Data;

namespace MaTech.Gameplay.Time {
    public enum TimePointOrderBy {
        Beat = 0,
        Time = 1,
    }

    /// <summary> 基于节拍与时间双重定位的乐理时间码（只读接口） </summary>
    public interface ITimePoint {
        /// <summary> 根据 BPM (Beat Per Minutes) 定义的节拍值，一拍通常是一个四分音符 </summary>
        public BeatUnit Beat { get; }

        /// <summary> 音轨时间，若音源在音轨0秒处开始播放，则对应音源的时间 </summary>
        public TimeUnit Time { get; }

        /// <summary> 按照 Beat 所定位的延迟前时间，Time = TimeOfBeat + Delay </summary>
        public TimeUnit TimeOfBeat { get; }

        /// <summary> Time 属性相对于 Beat 所定位的时间点的延迟，Time = TimeOfBeat + Delay </summary>
        public TimeUnit Delay { get; }
    }

    /// <summary> 基于节拍与时间双重定位的乐理时间码 </summary>
    public class TimePoint : ITimePoint {
        public static TimePoint MinValue { get; } = new() { Beat = Fraction.minValue, Time = TimeUnit.MinValue };
        public static TimePoint MaxValue { get; } = new() { Beat = Fraction.maxValue, Time = TimeUnit.MaxValue };

        private BeatUnit beat = 0;
        private TimeUnit time = TimeUnit.Zero;
        private TimeUnit delay;

        public TimePoint(ITimePoint? other = null) {
            if (other != null) CopyFrom(other);
        }

        public void CopyFrom(ITimePoint other) {
            beat = other.Beat;
            time = other.Time;
            delay = other.Delay;
        }

        public BeatUnit Beat {
            get => beat;
            set => beat = value;
        }

        public TimeUnit Time {
            get => time;
            set => time = value;
        }

        public TimeUnit TimeOfBeat {
            get => time.OffsetBy(delay.Negate());
            set => time = value.OffsetBy(delay);
        }

        public TimeUnit Delay {
            get => delay;
            set => SetDelay(value, keepTimeOfBeat: true);
        }
        
        public void SetDelay(TimeUnit value, bool keepTimeOfBeat) {
            if (keepTimeOfBeat) {
                SetDelayAndTimeOfBeat(value, TimeOfBeat);
            } else {
                delay = value;
            }
        }

        public void SetDelayAndTime(TimeUnit delay, TimeUnit time) {
            this.time = time;
            this.delay = delay;
        }

        public void SetDelayAndTimeOfBeat(TimeUnit delay, TimeUnit timeOfBeat) {
            this.time = timeOfBeat.OffsetBy(delay);
            this.delay = delay;
        }

        public void SetToMaxValue() => CopyFrom(MaxValue);
        public void SetToMinValue() => CopyFrom(MinValue);

        private class ComparerBeatImpl : IComparer<ITimePoint> {
            public int Compare(ITimePoint left, ITimePoint right) => left.Beat.CompareTo(right.Beat);
        }
        private class ComparerTimeImpl : IComparer<ITimePoint> {
            public int Compare(ITimePoint left, ITimePoint right) => left.Time.CompareTo(right.Time);
        }
        
        private static readonly IComparer<ITimePoint>[] comparers = { new ComparerBeatImpl(), new ComparerTimeImpl() };
        private static readonly Comparison<ITimePoint>[] comparisons = { ComparerBeat.Compare, ComparerTime.Compare };

        public static IComparer<ITimePoint> ComparerBeat => comparers[(int)TimePointOrderBy.Beat];
        public static IComparer<ITimePoint> ComparerTime => comparers[(int)TimePointOrderBy.Time];
        public static IComparer<ITimePoint> GetComparer(TimePointOrderBy order) => comparers[(int)order];

        public static Comparison<ITimePoint> ComparisonBeat => comparisons[(int)TimePointOrderBy.Beat];
        public static Comparison<ITimePoint> ComparisonTime => comparisons[(int)TimePointOrderBy.Time];
        public static Comparison<ITimePoint> GetComparison(TimePointOrderBy order) => comparisons[(int)order];

        public override string ToString() => $"Beat {beat}, Time {time}";
    }
}