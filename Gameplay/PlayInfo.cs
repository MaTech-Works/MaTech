// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay {
    // TODO: 考虑一下PlayInfo结构是否从继承改为组合
    public interface IPlayInfo {
        Chart Chart { get; }

        public enum MetaType {
            Summary = 0, Description, Comment,
            Title = 10, Musician, Artist, Designer, Album, Package,
            Music = 20, Image, Video, Cover, Animation, Scene,
            Difficulty = 30, Level, Style,
            StartTime = 40, EndTime, Length,
            Tempo = 50, MinTempo, MaxTempo, MeanTempo,
            NoteCount = 60, AccuracyDenominator, MaxCombo, MaxScore,
            ExtensionStart = 100
        }
        
        IMeta<MetaType> Metadata { get; }
        
        // TODO: 根据ChartPlayer需要内部处理的信息，多加几个默认的时间定义
        TimeUnit? TrackStartTime { get; }
        TimeUnit? FinishCheckTime { get; }
        
        int? RandomSeed { get; }

        bool NeedAutoPlay { get; }
        ReplayFile Replay { get; }
    }
}