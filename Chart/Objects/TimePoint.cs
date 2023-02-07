// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Data;

#nullable enable

namespace MaTech.Chart.Objects {
    public enum TimePointOrderBy {
        Beat, Time
    }

    /// <summary> 基于节拍与时间双重定位的乐理时间码（只读接口） </summary>
    public interface ITimePoint {
        /// <summary> 根据 BPM (Beat Per Minutes) 定义的节拍值，一拍通常是一个四分音符 </summary>
        public BeatUnit Beat { get; }

        /// <summary> 以毫秒计，在时间轴上的时间 </summary>
        public TimeUnit Time { get; }

        /// <summary> 按照 Beat 所定位的延迟前时间，Time = TimeOfBeat + Delay </summary>
        public TimeUnit TimeOfBeat { get; }

        /// <summary> Time 属性相对于 Beat 所定位的时间点的延迟，Time = TimeOfBeat + Delay </summary>
        public float Delay { get; }
    }

    /// <summary> 基于节拍与时间双重定位的乐理时间码 </summary>
    public class TimePoint : ITimePoint {
        public static readonly ITimePoint MinValue = new TimePoint { Beat = Fraction.minValue, Time = double.NegativeInfinity };
        public static readonly ITimePoint MaxValue = new TimePoint { Beat = Fraction.maxValue, Time = double.PositiveInfinity };

        private BeatUnit beat = 0;
        private TimeUnit time = 0;
        private float delay;

        public TimePoint() {}
        public TimePoint(ITimePoint? other) {
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
            get => time.OffsetBy(-delay);
            set => time = value.OffsetBy(delay);
        }

        public float Delay {
            get => delay;
            set => SetDelay(value, keepTimeOfBeat: true);
        }
        
        public void SetDelay(float value, bool keepTimeOfBeat) {
            if (keepTimeOfBeat) {
                SetDelayAndTimeOfBeat(value, TimeOfBeat);
            } else {
                delay = value;
            }
        }

        public void SetDelayAndTime(float delay, TimeUnit time) {
            this.time = time;
            this.delay = delay;
        }

        public void SetDelayAndTimeOfBeat(float delay, TimeUnit timeOfBeat) {
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

        public static readonly IComparer<ITimePoint> ComparerBeat = new ComparerBeatImpl();
        public static readonly IComparer<ITimePoint> ComparerTime = new ComparerTimeImpl();
        public static IComparer<ITimePoint> GetComparer(TimePointOrderBy order) => order == TimePointOrderBy.Beat ? ComparerBeat : ComparerTime;

        public static readonly Comparison<ITimePoint> ComparisonBeat = ComparerBeat.Compare;
        public static readonly Comparison<ITimePoint> ComparisonTime = ComparerTime.Compare;
        public static Comparison<ITimePoint> GetComparison(TimePointOrderBy order) => order == TimePointOrderBy.Beat ? ComparisonBeat : ComparisonTime;

        public override string ToString() => $"{beat}, {time}ms";
    }
}