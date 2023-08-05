// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Gameplay.Scoring {
    // TODO: 改成一个BitFlag结构，并移动定义到客户端业务代码
    // TODO: 为BitFlag结构增加简单的二进制加密
    [Flags]
    public enum HitResult {
        None = 0, //占位用  也可用于需要无效/吃掉某次击打时使用，注意与IGNORE相区分
        
        // 默认定义
        
        MaskBasic = Miss | Score1 | Score2 | Score3 | Score4,
        Miss = 1 << 0,
        Score1 = 1 << 1,
        Score2 = 1 << 2,
        Score3 = 1 << 3,
        Score4 = 1 << 4,

        MaskTiming = Late | Early,
        Late = 1 << 5,
        Early = 1 << 6,

        Combo = 1 << 8, //只加combo
        ComboBreak = 1 << 9, //只断combo
        Wipe = 1 << 10,
        Catch = 1 << 11,
        Flick = 1 << 12,
        Bomb = 1 << 13,
        Pass = 1 << 14,
        Linked = 1 << 15,

        // 按住操作
        Hold_Start = 1 << 17,
        Hold_End = 1 << 18,
        Hold_Tick = 1 << 19, // 中途节点
        Hold_Progress = 1 << 20, // 保持有效，更新进度
        Hold_Break = 1 << 21, // 断判
        Hold_Continue = 1 << 22, // 重新接上
        
        Activate = 1 << 24,
        Deactivate = 1 << 25,
        
        Mute = 1 << 26,
        
        Delay = 1 << 27, // 延迟结算判定结果
        Finish = 1 << 28, // 结束这个判定单元的全部判定结算
        Repeat = 1 << 29, // 以同样的输入条件对当前音符进行重复判定
        Ignore = 1 << 30, // 忽略本次判定的成绩，不要下发给任何下游游戏逻辑；同时也可用来表示此音符没有参与判定
    };

    #if UNITY_EDITOR
    /// <summary> HitResult在编辑器里下拉菜单中显示的名字，可以重载 </summary>
    public static class HitResultEditorNames {
        public static string[] Value {
            get => overriddenNames ?? internalNames;
            set => overriddenNames = value;
        }

        private static string[] overriddenNames;
        private static readonly string[] internalNames = {
            /*  0 --  7 */ "Miss", "Score1", "Score2", "Score3", "Score4", "Late", "Early", null,
            /*  8 -- 15 */ "Combo", "Combo Break", "Wipe", "Catch", "Flick", "Bomb", "Pass", "Linked",
            /* 16 -- 23 */ null, "Hold Start", "Hold End", "Hold Tick", "Hold Progress", "Hold Break", "Hold Continue", null,
            /* 24 -- 30 */ "Activate", "Deactivate", "Mute", "Delay", "Finish", "Repeat", "Ignore",
        };
    }
    #endif
    
    public static class EnumFlagBoilerplates {
        public static bool HasAnyFlag(this HitResult self, HitResult any) => (self & any) != 0;
        public static bool HasAllFlag(this HitResult self, HitResult all) => (self & all) == all;
        public static bool HasAnyFlagExcept(this HitResult self, HitResult any, HitResult except) => (self & any) != 0 && (self & except) == 0;
    }
}
