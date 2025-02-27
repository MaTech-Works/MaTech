// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Scoring;

namespace MaTech.Gameplay.Display {
    public class NoteLayer : ObjectLayer<ChartPlayer.NoteCarrier, NoteLayer> {
        public int HandleNoteHit(ChartPlayer.IJudgeUnit unit, JudgeLogicBase.NoteHitAction action, in TimeUnit judgeTime, HitResult result) {
            int count = 0;
            foreach (var carrier in RealizedCarriers) {
                if (carrier is not ChartPlayer.NoteCarrier noteCarrier) continue;
                if (!noteCarrier.ContainsUnit(unit)) continue;
                var visual = FindVisual<ChartPlayer.INoteVisual>(noteCarrier);
                if (visual == null) continue;
                visual.OnHit(unit, action, judgeTime, result);
                count += 1;
            }
            return count;
        }
    }
}