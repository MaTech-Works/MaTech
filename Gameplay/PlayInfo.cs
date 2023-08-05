// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay {
    // TODO: 考虑一下PlayInfo结构是否从继承改为组合
    public abstract class PlayInfo {
        public abstract Chart Chart { get; }

        public virtual TimeUnit StartTime => TimeUnit.FromMinutes(0);
        public virtual TimeUnit EndTime => TimeUnit.FromMinutes(2);
        
        public virtual int? RandomSeed => null;

        public virtual bool IsAuto => false;
        public virtual ReplayFile Replay => null;
        
        public bool Validate() {
            if (Chart == null) {
                return false;
            }
            
            /*
            if (RandomSeed != null && Replay != null && RandomSeed != Replay.Seed) {
                return false;
            }
            */
            
            return true;
        }
    }
}