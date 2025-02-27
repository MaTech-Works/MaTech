// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Scoring {
    // TODO: 合并进JudgeLogic，不在核心层面定义判定数据接口
    public interface IJudgeTiming {
        void Init(IPlayInfo playInfo);
        
        /// <summary> 判定逻辑的起始生效范围，返回正的绝对值（返回负数会让起始位置比音符更晚）。 </summary>
        TimeUnit WindowEarly { get; }
        /// <summary> 判定逻辑的结束生效范围，返回正的绝对值（返回负数会让结束位置比音符更早）。 </summary>
        TimeUnit WindowLate { get; }
            
        HitResult JudgeNoteHit(IJudgeUnit unit, JudgeLogicBase.NoteHitAction action, TimeUnit judgeTime);
    }
}
