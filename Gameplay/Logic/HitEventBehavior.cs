// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using static MaTech.Gameplay.ChartPlayer;
using static MaTech.Gameplay.Logic.JudgeLogicBase;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Logic {
    public class HitEventBehavior : PlayBehavior, PlayBehavior.INoteHitResult, IHitEventSource {
        #if ODIN_INSPECTOR
        [PropertySpace(0, 8)]
        #endif
        public HitEventBinding[] hitEvents;

        #if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideLabel, InlineProperty, BoxGroup("Last Hit")]
        #endif
        public HitEvent LastHit { get; private set; }
        
        public void OnHitNote(IJudgeUnit unit, NoteHitAction action, TimeUnit time, HitResult result) => hitEvents?.InvokeAll(LastHit = new(unit, action, time, result));
        public void OnHitEmpty(EmptyHitAction action, TimeUnit time) {}
    }
}