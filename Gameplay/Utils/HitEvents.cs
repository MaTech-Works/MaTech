// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Display;
using MaTech.Gameplay.Scoring;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay.Utils {
    public class HitEvents : PlayBehavior, PlayBehavior.INoteHitResult {
        public HitEvent[] hitEvents;

        public void OnHitNote(ChartPlayer.IJudgeUnit unit, JudgeLogicBase.NoteHitAction action, TimeUnit judgeTime, HitResult result) {
            foreach (var hitEvent in hitEvents) {
                hitEvent.InvokeIfMatch(action, result);
            }
        }
        
        public void OnHitEmpty(JudgeLogicBase.EmptyHitAction action, TimeUnit judgeTime) {}
    }
}