// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using MaTech.Audio;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay.Data {
    public class Chart {
        // TODO: 将tempos封装至TempoTimeline类并始终维护内部列表有序
        // TODO: 将effects封装至EffectTimeline类并始终维护内部列表有序，并且提供插值采样的方法
        
        public readonly List<TempoChange> tempos = new List<TempoChange>();
        public readonly List<Effect> effects = new List<Effect>();
        public readonly List<TimedObject> objects = new List<TimedObject>();

        public readonly SampleTrack sampleTrack = new SampleTrack();

        private readonly Func<TempoChange, BeatUnit, bool> funcMatchTempo = (tempo, beat) => tempo.StartOrMin.Beat.fraction <= beat.fraction;
        public TimeUnit CalculateTimeFromBeat(BeatUnit beat) {
            if (tempos.Count == 0) return TimeUnit.MinValue;
            int index = tempos.IndexOfLastMatchedValue(beat, funcMatchTempo);
            return tempos[index == -1 ? 0 : index].CalculateTimeFromBeat(beat);
        }

        // 从malody移植多模式parser架构的价值不高，其实现也不必与gameplay架构耦合，但是parser里有一些方便的文件处理功能可以日后作为独立功能移植。
        // TODO: 待移植parser后重新设计此接口
        public DummyChartParser CreateParser() => new DummyChartParser();
        public bool FullLoaded => true;

        public class DummyChartParser {
            public void Parse(bool fullReload) { }
        }
    }
}