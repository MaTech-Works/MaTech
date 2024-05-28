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
        /// <summary>
        /// 用默认规则创建一个新NoteCarrier：用note.start和note.end（如果不为null）计算剔除区间，用note.start为图形效果采样时间轴。
        /// </summary>
        /// <param name="note"> 原note </param>
        /// <param name="start"> 剔除区间的开始位置 </param>
        /// <param name="end"> 剔除区间的结束位置，不填则同start </param>
        /// <param name="anchor"> 图形效果（主要是hs）的采样位置，不填则同start </param>
        protected NoteCarrier CreateNoteCarrier(DataEnum<ObjectType> type, TimedObject note, TimePoint? overrideStart = null, TimePoint? overrideEnd = null, TimePoint? overrideAnchor = null) {
            return new NoteCarrier() {
                // TODO: 实现一种同时计算start和end的CreateTiming方法，正确计算卷轴回退时的Y值极值
                start = CreateTiming(overrideStart ?? note.Start),
                end = CreateTiming(overrideEnd ?? note.End),
                type = type,
                scaleY = FindTimeCarrier(overrideAnchor ?? note.Anchor).EffectiveNoteVelocity,
            };
        }
        
        /// <summary>
        /// 将一个NoteCarrier加入最后输出的note列表，并填写一些额外的必要数据。
        /// 可以传入null来单纯增加音符总数。
        /// </summary>
        /// <param name="note"> 输出的NoteCarrier </param>
        /// <returns> 如果成功添加音符则与输入相同，否则返回null </returns>
        protected void AddNoteCarrier(NoteCarrier note) {
            noteList.Add(note);
        }

    }
}
