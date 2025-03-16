// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using UnityEngine;
using static MaTech.Gameplay.ChartPlayer;
using static MaTech.Gameplay.Logic.JudgeLogicBase;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace MaTech.Gameplay.Logic {
    public interface IHitEventSource {
        public HitEvent LastHit { get; }
        public IJudgeUnit LastUnit => LastHit.unit;
        public NoteHitAction LastAction => LastHit.action;
        public HitResult LastResult => LastHit.result;
        public TimeUnit LastTime => LastHit.time;
    }
    
    public class HitEventMethod : MonoBehaviour, IHitEventSource {
        #if ODIN_INSPECTOR
        [PropertySpace(0, 8)]
        #endif
        public HitEventBinding[] hitEvents;

        #if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, HideLabel, InlineProperty, BoxGroup("Last Hit")]
        #endif
        public HitEvent LastHit { get; private set; }

        public void Invoke(IHitEventSource source) { if (source is not null) Invoke(source.LastHit); }
        public void Invoke(Object source) => Invoke(source as IHitEventSource);
        
        public void Invoke(in HitEvent hit) => hitEvents?.InvokeAll(LastHit = hit);

        public void Invoke(Variant data) {
            if (data.As<IHitEventSource>() is {} results) Invoke(results);
            else if (data.Is<HitEvent>()) Invoke(data.As<HitEvent>());
        }
    }
}