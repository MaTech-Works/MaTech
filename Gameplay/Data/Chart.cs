// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Audio;
using MaTech.Common.Algorithm;

namespace MaTech.Gameplay.Data {
    public class Chart {
        // todo: 将tempo封装至Timeline类，不在这里计算time
        // todo: 将effects封装至Timeline类，从Processor提取功能
        // todo: 思考Timeline如何组成成Chart类，也许可以不再使用此类，或者提供一种默认或可组合的结构与序列化支持
        
        public readonly List<TempoChange> tempos = new List<TempoChange>();
        public readonly List<Effect> effects = new List<Effect>();
        public readonly List<TimedObject> objects = new List<TimedObject>();

        public readonly SampleTrack sampleTrack = new SampleTrack();
        
        public TimeUnit CalculateTimeFromBeat(BeatUnit beat) {
            if (tempos.Count == 0) return TimeUnit.MinValue;
            int index = tempos.IndexOfLastMatchedValue(beat, (tempo, beat) => tempo.SafeStart.Beat.Fraction <= beat.Fraction);
            return tempos[index == -1 ? 0 : index].CalculateTimeFromBeat(beat);
        }
    }
}