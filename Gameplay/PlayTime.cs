// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using static UnityEngine.Time;

namespace MaTech.Gameplay {
    public static class PlayTime {
        public static bool IsPlaying { get; private set; }
        
        // todo: consider to use TimeUnit struct here
        // todo: rename JudgeTime -> InputTime, ChartTime -> LogicTime, DisplayTime -> VisualTime
        // todo: use LogicTime instead of InputTime for judge logic
        // todo: remove GlobalRoll support, separate Roll on each layer, and switch to RollUnit

        public static double Time => LogicTime.Seconds;
        
        public static TimeUnit AudioTime { get; private set; }
        public static TimeUnit LogicTime { get; private set; }
        public static TimeUnit InputTime { get; private set; }
        public static TimeUnit VisualTime { get; private set; }
        
        public static double GlobalRoll { get; private set; }

        internal class Setter {
            public double offsetAudio = 0;
            public double offsetInput = 0;
            public double offsetVisual = 0;

            // todo: we didn't even update time on input thread? probably a RW mutex is required
            // todo: implement a precise AudioTime struct similarly to MaAudio core
            // todo: smooth display time
            public void UpdateTime(double audioTime, bool onFrame) {
                AudioTime = audioTime;
                LogicTime = audioTime - offsetAudio;
                InputTime = audioTime + offsetInput;
                if (onFrame) VisualTime = LogicTime + offsetVisual;
            }
            
            public void UpdateGlobalRoll(double roll) => GlobalRoll = roll;
            public void SetPlaying(bool playing) => IsPlaying = playing;
        }
        
        public enum TimeSource {
            UnityTimeScaled = 0, UnityTimeUnscaled,
            AudioTime = 10, LogicTime, VisualTime, InputTime, GlobalRoll
        }

        public static TimeUnit Select(TimeSource source) {
            return source switch {
                TimeSource.UnityTimeScaled => TimeUnit.FromSeconds(timeAsDouble),
                TimeSource.UnityTimeUnscaled => TimeUnit.FromSeconds(unscaledTimeAsDouble),
                TimeSource.AudioTime => AudioTime,
                TimeSource.LogicTime => LogicTime,
                TimeSource.InputTime => InputTime,
                TimeSource.VisualTime => VisualTime,
                TimeSource.GlobalRoll => TimeUnit.FromSeconds(GlobalRoll),
                _ => TimeUnit.Zero
            };
        }
    }
}
