// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Scoring;
using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;
using static MaTech.Gameplay.Scoring.JudgeLogicBase;

namespace MaTech.Gameplay.Utils {
    // todo: always use this struct for OnHitNote and various methods
    [Serializable]
    public struct HitEventParameter {
        public IJudgeUnit unit;
        public NoteHitAction action;
        public TimeUnit judgeTime;
        public HitResult result;
    }

    public class HitEvents : PlayBehavior, PlayBehavior.INoteHitResult {
        [SerializeField]
        private HitEvent[] hitEvents;

        public HitEventParameter LastHit { private set; get; }
        public IJudgeUnit LastHitUnit => LastHit.unit;
        public TimeUnit LastJudgeTime => LastHit.judgeTime;
        
        public void OnHitNote(IJudgeUnit unit, NoteHitAction action, TimeUnit judgeTime, HitResult result) {
            LastHit = new() {
                unit = unit,
                action = action,
                judgeTime = judgeTime,
                result = result
            };
            foreach (var hitEvent in hitEvents) {
                hitEvent.InvokeIfMatch(action, result);
            }
        }
        
        public void OnHitEmpty(EmptyHitAction action, TimeUnit judgeTime) {}
    }
}