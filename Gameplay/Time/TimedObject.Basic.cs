// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

namespace MaTech.Gameplay.Time {
    public class TimedObjectSinglePoint : TimedObject {
        protected readonly ITimePoint t;

        public TimedObjectSinglePoint(ITimePoint? injectedStartAndEnd = null) => t = injectedStartAndEnd ?? new TimePoint();

        public override ITimePoint Start => t;
        public override ITimePoint End => t;

        public TimePoint? ModifiableTimePoint => t as TimePoint;
    }

    public class TimedObjectStartOnly : TimedObject {
        protected readonly ITimePoint t;

        public TimedObjectStartOnly(ITimePoint? injectedStart = null) => t = injectedStart ?? new TimePoint();

        public override ITimePoint Start => t;
        public override ITimePoint? End => null;
        
        public TimePoint? ModifiableTimePoint => t as TimePoint;
    }

    public class TimedObjectRanged : TimedObject {
        protected readonly (ITimePoint start, ITimePoint end) t;
        
        public TimedObjectRanged(ITimePoint? injectedStart = null, ITimePoint? injectedEnd = null)
            => t = (injectedStart ?? new TimePoint(), injectedEnd ?? new TimePoint());

        public override ITimePoint Start => t.start;
        public override ITimePoint End => t.end;

        public TimePoint? ModifiableTimePointStart => t.start as TimePoint;
        public TimePoint? ModifiableTimePointEnd => t.end as TimePoint;
    }
}
