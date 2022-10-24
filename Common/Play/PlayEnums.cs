// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Common.Play {
    public enum PlayModeType {
        Key = 0,
        Step = 1,
        DJ = 2,
        Catch = 3,
        Pad = 4,
        Taiko = 5,
        Ring = 6,
        Slide = 7,
        Live = 8,
        Cube = 9,
        Max = 10
    }

    [Flags]
    public enum PlayModeBit {
        None = 0,
        Key = 1 << 0,
        Step = 1 << 1,
        DJ = 1 << 2,
        Catch = 1 << 3,
        Pad = 1 << 4,
        Taiko = 1 << 5,
        Ring = 1 << 6,
        Slide = 1 << 7,
        Live = 1 << 8,
        Cube = 1 << 9,
        Max = 10,
    }

    [Flags]
    public enum ModMask {
        None = 0,
        Auto = 1,
        Luck = 1 << 1,
        Flip = 1 << 2,
        Const = 1 << 3,
        Dash = 1 << 4, //16
        Rush = 1 << 5,
        Hide = 1 << 6,
        Origin = 1 << 7,
        Slow = 1 << 8, //256
        Death = 1 << 9,
        Fair = 1 << 10,
        Invert = 1 << 11, //2048
        Max = 12,
    };

    public enum PlayByType {
        Player,
        Marlo,
        ReplayPlayer,
        OnlinePlayer
    };

    public enum PlayFrom {
        Normal,
        Editor,
        Result,
        MP,
        Replay,
        Watch
    };

    public enum JudgeLevel {
        A = 0,
        B,
        C,
        D,
        E,
        MAX
    };

    [Flags]
    public enum HitTiming {
        None = 0, //占位用  也可用于需要无效/吃掉某次击打时使用，注意与IGNORE相区分

        MaskBasic = Best | Cool | Good | Bad | Miss,
        Miss = 1 << 0,
        Best = 1 << 1,
        Cool = 1 << 2,
        Good = 1 << 3,
        Bad = 1 << 4,

        MaskTiming = Late | Early,
        Late = 1 << 5,
        Early = 1 << 6,

        Combo = 1 << 7, //只加combo
        ComboBreak = 1 << 9, //只断combo
        Catch = 1 << 10, //catch模式专用加分 加combo, 不影响判定

        Hold_Start = 1 << 23, // 头判标志位，任何对hold的头部判定，请尽量带上这个位，方便UI动画判断
        Hold_End = 1 << 11, // 尾判标志位，与其他成绩位配合表示尾判抬起的成绩或者单纯传递hold结束消息
        Hold_Early = 1 << 18, // 断hold, 影响加成系数
        Hold_Continue = 1 << 24, // 断hold后重新接上
        Hold_Tick = Combo, // hold保持时的combo点，每次只处理一个过往的tick，业务逻辑要用循环处理同一帧多个tick的情况

        // taiko专用分数，占 12 ~ 15 三个二进制位
        Taiko_BigHit = 1 << 12, // 大打，与basic位配合表示大打成绩(不加combo)，与其他位配合表示对应大号版音符
        Taiko_Renda = 1 << 13, // 连打，与大打配合代表大连打分数
        Taiko_RendaBig = Taiko_Renda | Taiko_BigHit, // 大连打
        Taiko_BalloonHit = 1 << 14, // 气球打
        Taiko_BalloonPop = 1 << 15, // 气球最后一打与爆炸

        // Live模式尽量复用bit
        Slide_Start = Hold_Start, // Slide开始节点
        Slide_Continue = Hold_Continue, // 正在接Slide
        Slide_End = Hold_End, // Slide结束节点
        Slide_Break = Hold_Early, // Slide模式中间断开
        Slide_Tick = 1 << 19, // Live模式，接到Slide中间节点

        Slide_WipeBest = 1 << 22,
        Slide_Combo = Catch, //slide的combo，加少量分
        Slide_Flick = 1 << 25,

        IIDX_HoldMiss = Hold_End | Miss, // iidx在头部miss; 尾部也多一次miss

        NonMiss = Best | Cool | Good | Bad,
        Valid = Best | Cool | Good,
        SlideValid = Valid | Slide_WipeBest,
        MusicBar = Valid | Miss,
        Default = MusicBar, //默认的计分体系，含miss
        MissAuto = Miss | Late, // 超出判定范围从未被击打时的判定

        Repeat = 1 << 29, // 以同样的输入条件对当前音符进行重复判定
        Ignore = 1 << 30, // 忽略本次判定的成绩，不要下发给任何下游游戏逻辑；同时也可用来表示此音符没有参与判定
    };

    [Flags]
    public enum HitState {
        NotHit = 0,
        Hit = 1 << 0,
        Holding = 1 << 2,
        HoldCompleted = 1 << 3,
        Sounded = 1 << 4, // 发过一次声
        Appear = 1 << 5, // note已经出现过
    };

    // Unity编辑器内用，位名称
    public struct HitTimingNames {
        public static readonly string[] nameByBitIndex = {
            /*  0 --  7 */ "Miss", "Best", "Cool", "Good", "Bad", "Late", "Early", "Combo",
            /*  8 -- 15 */ null, "Combo Break", "Catch", "Hold End", "Taiko Bit 0", "Taiko Bit 1", "Taiko Bit 2", null,
            /* 16 -- 23 */ null, null, null, null, "Hold Early", "Slide Tick", "Wipe Best", "Hold Start",
            /* 24 -- 30 */ "Hold Continue", null, null, null, null, null, "Ignore",
        };
    }

    public static class EnumFlagBoilerplates {
        public static bool HasAnyFlag(this PlayModeBit self, PlayModeBit flag) => (self & flag) != 0;
        public static bool HasAllFlag(this PlayModeBit self, PlayModeBit flag) => (self & flag) == flag;
        public static bool HasAnyFlag(this ModMask self, ModMask flag) => (self & flag) != 0;
        public static bool HasAllFlag(this ModMask self, ModMask flag) => (self & flag) == flag;
        public static bool HasAnyFlag(this HitState self, HitState flag) => (self & flag) != 0;
        public static bool HasAllFlag(this HitState self, HitState flag) => (self & flag) == flag;
        public static bool HasAnyFlag(this HitTiming self, HitTiming flag) => (self & flag) != 0;
        public static bool HasAllFlag(this HitTiming self, HitTiming flag) => (self & flag) == flag;
    }
}