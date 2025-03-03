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
                start = SampleTiming(overrideStart ?? note.SafeStart),
                end = SampleTiming(overrideEnd ?? note.SafeEnd),
                scale = SampleNoteSpeed(overrideAnchor ?? note.Anchor),
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
