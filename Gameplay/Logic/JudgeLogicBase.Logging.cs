// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Tools;
using MaTech.Gameplay.Data;

namespace MaTech.Gameplay.Logic {
    public abstract partial class JudgeLogicBase {
        public delegate bool NoteHitFilterFunc(ChartPlayer.IJudgeUnit unit, NoteHitAction action, TimeUnit time, HitResult result);
        public delegate bool EmptyHitFilterFunc(EmptyHitAction action, TimeUnit time);

        public NoteHitFilterFunc logFilterOnNoteHit;
        public EmptyHitFilterFunc logFilterOnEmptyHit;

        private void LogNoteHit(ChartPlayer.IJudgeUnit unit, NoteHitAction action, TimeUnit time, HitResult result) {
            if (DebugLogHistory.HasInstances && logFilterOnNoteHit is not null) {
                if (logFilterOnNoteHit(unit, action, time, result))
                    DebugLogHistory.PushHistory("play", $"{time}: note hit {action} [{unit}] -- [{result.ToEditorName()}]");
            }
        }

        private void LogEmptyHit(EmptyHitAction action, TimeUnit time) {
            if (DebugLogHistory.HasInstances && logFilterOnEmptyHit is not null) {
                if (logFilterOnEmptyHit(action, time))
                    DebugLogHistory.PushHistory("play", $"{time}: empty hit {action}");
            }
        }
    }
}