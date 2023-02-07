// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

#nullable enable

namespace MaTech.Chart.Objects {
    public enum TimedObjectOrderBy {
        Anchor, Start, End
    }

    /// <summary> 在谱面时间轴上定位的物件 </summary>
    public abstract class TimedObject {
        /// <summary> 物件的开始位置，可以同anchor，指定null则为谱面开始时即生效（无头） </summary>
        public virtual ITimePoint? Start => null;
        /// <summary> 物件的结束位置，可以同anchor，指定null则为谱面直到结束都生效（无尾） </summary>
        public virtual ITimePoint? End => null;

        /// <summary> 物件的定位（effect等的参考时间）与排序基准 </summary>
        public virtual ITimePoint Anchor => Start ?? End ?? TimePoint.MinValue;

        public bool HasStart => Start != null;
        public bool HasEnd => End != null;
        
        public ITimePoint StartOrMin => Start ?? TimePoint.MinValue;
        public ITimePoint EndOrMax => End ?? TimePoint.MaxValue;
        
        public static readonly IComparer<TimedObject> ComparerAnchorBeat = CreateComparer(true, o => o.Anchor);
        public static readonly IComparer<TimedObject> ComparerAnchorTime = CreateComparer(false, o => o.Anchor);
        public static readonly IComparer<TimedObject> ComparerStartBeat = CreateComparer(true, o => o.StartOrMin);
        public static readonly IComparer<TimedObject> ComparerStartTime = CreateComparer(false, o => o.StartOrMin);
        public static readonly IComparer<TimedObject> ComparerEndBeat = CreateComparer(true, o => o.EndOrMax);
        public static readonly IComparer<TimedObject> ComparerEndTime = CreateComparer(false, o => o.EndOrMax);
        
        public static IComparer<TimedObject> GetComparer(TimedObjectOrderBy orderObject, TimePointOrderBy orderTimePoint) {
            switch (orderObject, orderTimePoint) {
            case (TimedObjectOrderBy.Anchor, TimePointOrderBy.Beat): return ComparerAnchorBeat;
            case (TimedObjectOrderBy.Anchor, TimePointOrderBy.Time): return ComparerAnchorTime;
            case (TimedObjectOrderBy.Start, TimePointOrderBy.Beat): return ComparerStartBeat;
            case (TimedObjectOrderBy.Start, TimePointOrderBy.Time): return ComparerStartTime;
            case (TimedObjectOrderBy.End, TimePointOrderBy.Beat): return ComparerEndBeat;
            case (TimedObjectOrderBy.End, TimePointOrderBy.Time): return ComparerEndTime;
            default: throw new InvalidOperationException($"TimedObject.GetComparer called with unsupported enum [{orderObject}] and [{orderTimePoint}]");
            }
        }

        private static IComparer<TimedObject> CreateComparer(bool isBeatInsteadOfTime, Func<TimedObject, ITimePoint> whichTimePoint) {
            return new MappedComparer<TimedObject, ITimePoint>(isBeatInsteadOfTime ? TimePoint.ComparerBeat : TimePoint.ComparerTime, whichTimePoint);
        }
    }
}
