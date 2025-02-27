// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Tools;
using MaTech.Gameplay.Data;

namespace MaTech.Gameplay.Scoring {
    public abstract partial class JudgeLogicBase {
        public delegate bool NoteHitFilterFunc(ChartPlayer.IJudgeUnit unit, NoteHitAction action, TimeUnit judgeTime, HitResult result);
        public delegate bool EmptyHitFilterFunc(EmptyHitAction action, TimeUnit judgeTime);

        public NoteHitFilterFunc logFilterOnNoteHit;
        public EmptyHitFilterFunc logFilterOnEmptyHit;

        private void LogNoteHit(ChartPlayer.IJudgeUnit unit, NoteHitAction action, TimeUnit judgeTime, HitResult result) {
            if (DebugLogHistory.HasInstances && logFilterOnNoteHit is not null) {
                if (logFilterOnNoteHit(unit, action, judgeTime, result))
                    DebugLogHistory.PushHistory("play", $"{judgeTime}: note hit {action} [{unit}] --> [{result.ToEditorName()}]");
            }
        }

        private void LogEmptyHit(EmptyHitAction action, TimeUnit judgeTime) {
            if (DebugLogHistory.HasInstances && logFilterOnEmptyHit is not null) {
                if (logFilterOnEmptyHit(action, judgeTime))
                    DebugLogHistory.PushHistory("play", $"{judgeTime}: empty hit {action}");
            }
        }
    }
}