// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

namespace MaTech.Gameplay.Data {
    public enum TimedObjectOrderBy {
        Anchor, Start, End
    }

    /// <summary> 在谱面时间轴上定位的物件 </summary>
    public abstract class TimedObject {
        /// <summary> 物件的开始位置，可以同anchor或者为null </summary>
        public virtual ITimePoint? Start => null;
        /// <summary> 物件的结束位置，可以同anchor或者为null </summary>
        public virtual ITimePoint? End => null;

        /// <summary> 物件的定位（effect等的参考时间）与排序基准 </summary>
        public virtual ITimePoint Anchor => Start ?? End ?? TimePoint.minValue;

        public bool HasStart => Start != null;
        public bool HasEnd => End != null;
        
        public ITimePoint SafeStart => Start ?? TimePoint.minValue;
        public ITimePoint SafeEnd => End ?? TimePoint.maxValue;
        
        public Range<TimeUnit> TimeRange => new(TimeStart, TimeEnd);
        public TimeUnit TimeStart => SafeStart.Time;
        public TimeUnit TimeEnd => SafeEnd.Time;
        
        public Range<BeatUnit> BeatRange => new(BeatStart, BeatEnd);
        public BeatUnit BeatStart => SafeStart.Beat;
        public BeatUnit BeatEnd => SafeEnd.Beat;
        
        public static IComparer<TimedObject> ComparerAnchorBeat => comparers[0];
        public static IComparer<TimedObject> ComparerAnchorTime => comparers[1];
        public static IComparer<TimedObject> ComparerStartBeat => comparers[2];
        public static IComparer<TimedObject> ComparerStartTime => comparers[3];
        public static IComparer<TimedObject> ComparerEndBeat => comparers[4];
        public static IComparer<TimedObject> ComparerEndTime => comparers[5];
        
        public static IComparer<TimedObject> GetComparer(TimedObjectOrderBy orderObject, TimePointOrderBy orderTimePoint) {
            switch (orderObject, orderTimePoint) {
            case (TimedObjectOrderBy.Anchor, TimePointOrderBy.Beat): return comparers[0];
            case (TimedObjectOrderBy.Anchor, TimePointOrderBy.Time): return comparers[1];
            case (TimedObjectOrderBy.Start, TimePointOrderBy.Beat): return comparers[2];
            case (TimedObjectOrderBy.Start, TimePointOrderBy.Time): return comparers[3];
            case (TimedObjectOrderBy.End, TimePointOrderBy.Beat): return comparers[4];
            case (TimedObjectOrderBy.End, TimePointOrderBy.Time): return comparers[5];
            default: throw new InvalidOperationException($"TimedObject.GetComparer called with unsupported enum [{orderObject}] and [{orderTimePoint}]");
            }
        }

        private static readonly IComparer<TimedObject>[] comparers = {
            CreateComparer(true, o => o.Anchor),
            CreateComparer(false, o => o.Anchor),
            CreateComparer(true, o => o.SafeStart),
            CreateComparer(false, o => o.SafeStart),
            CreateComparer(true, o => o.SafeEnd),
            CreateComparer(false, o => o.SafeEnd),
        };

        private static IComparer<TimedObject> CreateComparer(bool isBeatInsteadOfTime, Func<TimedObject, ITimePoint> whichTimePoint) {
            return new MappedComparer<TimedObject, ITimePoint>(isBeatInsteadOfTime ? TimePoint.ComparerBeat : TimePoint.ComparerTime, whichTimePoint);
        }
    }
}
