// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Common.Data;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay.Scoring {
    public enum ScoreType {
        Score, Combo, Accuracy, HP, IsFullCombo, IsAllPerfect
        // Tips: 可以使用 EnumEx 来扩展额外的 ScoreType
    }
    
    public interface IScore {
        void Init(IPlayInfo info);
        void Finish();
        
        void HandleScoreResult(HitResult result, TimeUnit judgeTime);
        
        void GetSnapshot(MetaTable<ScoreType> outScoreSnapshot);
        Variant GetValue(EnumEx<ScoreType> scoreType);
        
        bool IsScoreAuthentic { get; }
    }
}