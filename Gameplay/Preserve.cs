// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Algorithm;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Scoring;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly, Preserve]

namespace MaTech.Gameplay {
    internal static class PreserveStub {
        [Preserve]
        public static void PreserveStub_DoNotCall() {
            BoxlessConvert.PreserveForEnum<EffectType>();
            BoxlessConvert.PreserveForEnum<ScoreType>();
            BoxlessConvert.PreserveForEnum<HitResult>();
            BoxlessConvert.PreserveForEnum<JudgeLogicBase.NoteHitAction>();
            BoxlessConvert.PreserveForEnum<JudgeLogicBase.EmptyHitAction>();
            BoxlessConvert.PreserveForEnum<ChartPlayer.ObjectType>();
        }
    }
}
