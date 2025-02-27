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
        
        // todo: always use TimeUnit struct here

        public static double AudioTime { get; private set; }
        public static double ChartTime { get; private set; }
        
        public static TimeUnit JudgeTime { get; private set; }
        
        public static double DisplayTime { get; private set; }
        public static double DisplayY { get; private set; }

        internal class Setter {
            public double offsetAudio = 0;
            public double offsetJudge = 0;
            public double offsetDisplay = 0;

            public void UpdateTime(double audioTime, bool onFrame) {
                AudioTime = audioTime;
                ChartTime = audioTime - offsetAudio;
                JudgeTime = TimeUnit.FromSeconds(audioTime + offsetJudge);
                
                // TODO: Smooth display time
                if (onFrame) DisplayTime = ChartTime + offsetDisplay;
            }
            
            public void UpdateDisplayY(double displayY) {
                DisplayY = displayY;
            }
            
            public void SetPlaying(bool playing) => IsPlaying = playing;

        }
        
        public enum TimeSource {
            UnityTimeScaled = 0, UnityTimeUnscaled,
            AudioTime = 10, ChartTime, VisualTime, JudgeTime, ScrollTime
        }

        public static TimeUnit Select(TimeSource source) {
            return source switch {
                TimeSource.UnityTimeScaled => TimeUnit.FromSeconds(timeAsDouble),
                TimeSource.UnityTimeUnscaled => TimeUnit.FromSeconds(unscaledTimeAsDouble),
                TimeSource.AudioTime => TimeUnit.FromSeconds(AudioTime),
                TimeSource.ChartTime => TimeUnit.FromSeconds(ChartTime),
                TimeSource.VisualTime => TimeUnit.FromSeconds(DisplayTime),
                TimeSource.JudgeTime => JudgeTime,
                TimeSource.ScrollTime => TimeUnit.FromSeconds(DisplayY),
                _ => TimeUnit.Zero
            };
        }
    }
}
