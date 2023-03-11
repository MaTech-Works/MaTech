// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common;
using MaTech.Common.Play;

namespace MaTech.Play {
    public struct PlayInfo {
        public static PlayInfo Current { get; set; }

        // TODO: 把这三个类搬运到MaChart库
        /*
        private Chart.MaChart chart;
        private Song song;
        private ReplayFile replay;
        */
        
        // TODO: 定义一个meta接口，放置具体数据与方法的实现
        // public IPlayMeta meta;

        public int randomSeed;

        public int? startOffset;
        public int? endOffset;

        /*
        public MaChart Chart {
            get => chart;
            set {
                chart = value;
                if (value != null) {
                    song = chart.Song;
                    playMode = chart.PlayMode;
                } else {
                    song = null;
                    playMode = PlayModeType.Max;
                }
            }
        }

        public Song Song {
            get => song;
            set {
                song = value;
                chart = null;
                playMode = PlayModeType.Max;
            }
        }

        public ReplayFile Replay {
            get => replay;
            set {
                replay = value;
                //Chart = ChartManager.G.GetChartByHash(replay.ChartHash);
                level = (JudgeLevel) replay.Score.Level;
                Mod = (ModMask) replay.Score.Mod;
                proJudge = replay.Score.Pro;
                customJudge = CustomJudgeProfile.Parse(replay.Score.Custom);
            }
        }
        */

        public bool Validate() {
            /*
            if (chart == null) {
                return false;
            }
            */

            // seed的唯一生成处
            // TODO: 将清洁后的helper移动到common库，再启用seed生成代码
            /*
            if (randomSeed == 0) {
                if (replay != null) {
                    randomSeed = replay.Seed;
                } else {
                    randomSeed = new Random((int) TimeUtil.Now).Next();
                }
            }
            */
            
            // TODO: 为IPlayMeta实现相应的接口
            // meta.Validate(this);

            return true;
        }
    }
}