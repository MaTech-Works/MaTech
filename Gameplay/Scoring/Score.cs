// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Common.Data;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay.Scoring {
    public enum ScoreType {
        Score, Combo, Accuracy, HP, IsFailed, IsFullCombo, IsAllPerfect
        // Tips: 可以使用 DataEnum 来扩展额外的 ScoreType
    }
    
    public interface IScore : IMetaVisitable<ScoreType> {
        void Init(IPlayInfo info);
        void Finish(bool isFailed);
        
        void HandleScoreResult(HitResult result, TimeUnit judgeTime);
        
        bool IsScoreAuthentic { get; }
    }
}