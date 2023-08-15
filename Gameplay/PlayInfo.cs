// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay {
    // TODO: 考虑一下PlayInfo结构是否从继承改为组合
    public interface IPlayInfo {
        Chart Chart { get; }
        
        // TODO: 根据ChartPlayer需要内部处理的信息，多加几个默认的时间定义
        TimeUnit? TrackStartTime { get; }
        TimeUnit? FinishCheckTime { get; }
        
        int? RandomSeed { get; }

        bool NeedAutoPlay { get; }
        ReplayFile Replay { get; }
    }
}