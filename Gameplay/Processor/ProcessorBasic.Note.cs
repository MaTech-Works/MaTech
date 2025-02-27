// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using static MaTech.Gameplay.ChartPlayer;

#nullable enable

namespace MaTech.Gameplay.Processor {
    public partial class ProcessorBasic {
        protected NoteCarrier CreateNoteCarrier(DataEnum<ObjectType> type, TimedObject note, ITimePoint? overrideStart = null, ITimePoint? overrideEnd = null, ITimePoint? overrideAnchor = null) {
            return new NoteCarrier {
                type = type,
                // TODO: 实现一种同时计算start和end的CreateTiming方法，正确计算卷轴回退时的Y值极值
                start = CreateTiming(overrideStart ?? note.StartOrMin),
                end = CreateTiming(overrideEnd ?? note.EndOrMax),
                scaleY = (overrideAnchor ?? note.Anchor) is {} anchor ? FindTimeCarrier(anchor).noteScaleY : 1.0f
            };
        }
        
        protected void AddNoteCarrier(NoteCarrier note) {
            noteList.Add(note);
        }

        protected NoteCarrier EmplaceNoteCarrier(DataEnum<ObjectType> type, TimedObject note, params IUnit[] units) {
            var carrier = CreateNoteCarrier(type, note);
            carrier.AssignUnits(units);
            AddNoteCarrier(carrier);
            return carrier;
        }
        protected NoteCarrier EmplaceNoteCarrier(DataEnum<ObjectType> type, TimedObject note, ITimePoint? overrideStart = null, ITimePoint? overrideEnd = null, params IUnit[] units) {
            var carrier = CreateNoteCarrier(type, note, overrideStart, overrideEnd);
            carrier.AssignUnits(units);
            AddNoteCarrier(carrier);
            return carrier;
        }
        protected NoteCarrier EmplaceNoteCarrier(DataEnum<ObjectType> type, TimedObject note, ITimePoint? overrideStart = null, ITimePoint? overrideEnd = null, ITimePoint? overrideAnchor = null, params IUnit[] units) {
            var carrier = CreateNoteCarrier(type, note, overrideStart, overrideEnd, overrideAnchor);
            carrier.AssignUnits(units);
            AddNoteCarrier(carrier);
            return carrier;
        }
    }
}
