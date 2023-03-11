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
    public class TimedObjectSinglePoint : TimedObject {
        protected readonly TimePoint t;

        public TimedObjectSinglePoint() => t = new TimePoint();
        public TimedObjectSinglePoint(ITimePoint otherTimePoint) => t = new TimePoint(otherTimePoint);

        public override ITimePoint? Start => t;
        public override ITimePoint? End => t;
    }

    public class TimedObjectStartOnly : TimedObject {
        protected readonly TimePoint t;

        public TimedObjectStartOnly() => t = new TimePoint();
        public TimedObjectStartOnly(ITimePoint otherTimePoint) => t = new TimePoint(otherTimePoint);

        public override ITimePoint? Start => t;
        public override ITimePoint? End => null;
    }

    public class TimedObjectRanged : TimedObject {
        protected readonly (TimePoint start, TimePoint end) t;

        public override ITimePoint Start => t.start;
        public override ITimePoint End => t.end;

        public TimedObjectRanged() => t = (new TimePoint(), new TimePoint());
        public TimedObjectRanged(ITimePoint otherStart, ITimePoint otherEnd) => t = (new TimePoint(otherStart), new TimePoint(otherEnd));

    }
}