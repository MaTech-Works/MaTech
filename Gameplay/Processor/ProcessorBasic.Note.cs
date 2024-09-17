// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Gameplay.Time;
using static MaTech.Gameplay.ChartPlayer;

#nullable enable

namespace MaTech.Gameplay.Processor {
    public partial class ProcessorBasic {
        protected NoteCarrier CreateNoteCarrier(DataEnum<ObjectType> type, TimedObject note, ITimePoint? overrideStart = null, ITimePoint? overrideEnd = null, ITimePoint? overrideAnchor = null) {
            return new NoteCarrier() {
                // TODO: 实现一种同时计算start和end的CreateTiming方法，正确计算卷轴回退时的Y值极值
                start = CreateTiming(overrideStart ?? note.Start),
                end = CreateTiming(overrideEnd ?? note.End),
                type = type,
                scaleY = FindTimeCarrier(overrideAnchor ?? note.Anchor).noteScaleY,
            };
        }
        
        protected void AddNoteCarrier(NoteCarrier note) {
            noteList.Add(note);
        }

    }
}
