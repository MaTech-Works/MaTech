// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common;
using MaTech.Common.Play;

namespace MaTech.Play {
    public class PlayInfo {
        public static PlayInfo Current { get; set; }

        // TODO: 把这三个类搬运到MaChart库
        /*
        private Chart.MaChart chart;
        private Song song;
        private ReplayFile replay;
        */

        public PlayModeType playMode;
        public PlayByType playBy;
        public PlayFrom playFrom;

        public ModMask Mod { get; set; }
        public ModMask ModForPlay { get; private set; }

        public JudgeLevel level;
        public bool proJudge;

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

            ModForPlay = PlayUtil.ValidatedMod(Mod, playMode);

            // 如果是来自watch的mp场景，主播端的validate会把mod设置为正确值，这里不用再验
            if (playFrom == PlayFrom.MP) {
                ModForPlay &= ~(ModMask.Auto | ModMask.Luck | ModMask.Death);
            } else {
                ModForPlay &= ~ModMask.Fair;
            }

            // 观众不能有死亡模式
            if (playFrom == PlayFrom.Watch) {
                ModForPlay &= ~ModMask.Death;
            }

            if (IsAuto) {
                playBy = PlayByType.Marlo;
            }

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

            return true;
        }

        public ModMask TurnOnMod(ModMask mod) {
            var turnoff = PlayUtil.ConflictedMod(mod);
            if (turnoff != ModMask.None) {
                Mod &= ~turnoff;
            }

            Mod |= mod;
            return Mod;
        }

        public ModMask TurnOffMod(ModMask mod) {
            Mod &= ~mod;

            return Mod;
        }

        public bool IsAuto => ModForPlay.HasAnyFlag(ModMask.Auto);
        public bool IsOrigin => ModForPlay.HasAnyFlag(ModMask.Origin);

        public void TurnOffAllMods() { Mod = ModMask.None; }

        public void Reset() {
            playBy = PlayByType.Player;
            playFrom = PlayFrom.Normal;
            Mod = ModMask.None;
            startOffset = null;
            endOffset = null;
            //replay = null;
            //replayLegacy = null;
        }

        /// <summary>
        /// 用于固化play信息的场合
        /// </summary>
        /// <returns></returns>
        public PlayInfo Dump() => new PlayInfo {
            //chart = chart,
            //song = song,
            level = level,
            playMode = playMode,
            playFrom = playFrom,
            playBy = playBy,
            Mod = Mod,
            ModForPlay = ModForPlay,
            startOffset = startOffset,
            //replay = replay,
            //replayLegacy = replayLegacy,
            proJudge = proJudge,
            randomSeed = randomSeed,
        };
    }
}