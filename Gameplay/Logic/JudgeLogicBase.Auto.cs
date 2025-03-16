// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Input;
using UnityEngine.Assertions;

namespace MaTech.Gameplay.Logic {
    public abstract partial class JudgeLogicBase {
        public abstract class AutoPlayControllerBase : IPlayController {
            public bool IsPlayer => false;
            private IPlayControl control;
            
            public void AttachController(IPlayControl playControl) => control = playControl;
            public void DetachController() => control = null;

            public void ResetControl(TimeUnit judgeTime) => ResetAutoPlay(judgeTime);
            public void UpdateControl(TimeUnit judgeTime) {
                Assert.IsNotNull(control);
                UpdateAutoPlay(judgeTime, control);
            }

            protected abstract void ResetAutoPlay(TimeUnit judgeTime);
            protected abstract void UpdateAutoPlay(TimeUnit judgeTime, IPlayControl control);

        }
    }
}