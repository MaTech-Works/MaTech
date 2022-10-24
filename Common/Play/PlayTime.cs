// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Algorithm;
using MaTech.Common.Utils;

namespace MaTech.Common.Play {
    // TODO: make this class non-static (find a different paradigm that allows non-global time)
    public static class PlayTime {
        public static bool IsPlaying { get; private set; }

        public static double AudioTime { get; private set; }
        public static double ChartTime { get; private set; }

        public static int JudgeTime { get; private set; }

        // TODO: expose smoothed display time here

        public static double DisplayTime { get; private set; }
        public static double ScrollPosition { get; private set; }

        internal class Setter {
            public double offsetAudio = 0;
            public double offsetJudge = 0;
            public double offsetDisplay = 0;

            public void UpdateTime(double audioTime, bool onFrame) {
                AudioTime = audioTime;
                ChartTime = audioTime - offsetAudio;
                JudgeTime = MathUtil.RoundToInt(ChartTime + offsetJudge);

                if (onFrame) DisplayTime = ChartTime + offsetDisplay;
            }

            public void UpdateScrollPosition(double val) {
                ScrollPosition = val;
            }

            public void SetPlaying(bool playing) => IsPlaying = playing;
        }
    }
}