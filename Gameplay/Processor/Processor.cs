// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Processor {
    /// <summary>
    /// Processor类
    /// 将Chart Data的Timing，Note和Effect数据处理成可以显示的Carrier图形数据。
    /// 接收Chart的Time，Note和Effect容器作为参数，生成联系时间-图形相关量TimeCarrier和音符逻辑-图形相关量NoteCarrier的列表，以及作为可选结果的承载小节线信息的BarCarrier。
    /// todo: 与MonoBehaviour解耦，不依赖场景来查找此组件
    /// todo: 把processor细分成不同的用途，如TimeProcessor、NoteProcessor等，并且允许每个ObjectLayer使用不同的processor
    /// </summary>
    public abstract class Processor : MonoBehaviour {
        public IPlayInfo PlayInfo { get; set; }
        
        public Chart Chart => PlayInfo.Chart;
        public List<TempoChange> Tempos => Chart.tempos;
        public List<Effect> Effects => Chart.effects;
        public List<TimedObject> Objects => Chart.objects;

        public int RandomSeed => PlayInfo.RandomSeed ?? 0;
        
        // todo: 输出的list无法确定排序规则；重构时增加输出排序后版本的方法，参数可以使用Carrier上的若干Comparer
        public QueueList<TimeCarrier> ResultTimeList { get; protected set; }
        public QueueList<NoteCarrier> ResultNoteList { get; protected set; }
        public QueueList<NoteCarrier> ResultBarList { get; protected set; }

        // Generate timeList and noteList, possibly barList.
        /// <summary>
        /// Generate <see cref="ResultTimeList" /> and <see cref="ResultNoteList" />, possibly <see cref="ResultBarList" />.
        /// </summary>
        public abstract bool Process();

        public virtual RollValue TimeToRoll(in TimeValue time, in Variant keyword = default) => PlayTime.VisualTime.Value;
    }
}
