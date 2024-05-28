// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace MaTech.Gameplay.Scoring {
    // TODO: 实现一个BitFlag结构体与简单的二进制加密
    [Flags]
    public enum HitResult {
        None = 0,
        Bit0 = 1 << 0,
        Bit1 = 1 << 1,
        Bit2 = 1 << 2,
        Bit3 = 1 << 3,
        Bit4 = 1 << 4,
        Bit5 = 1 << 5,
        Bit6 = 1 << 6,
        Bit7 = 1 << 7,
        Bit8 = 1 << 8,
        Bit9 = 1 << 9,
        Bit10 = 1 << 10,
        Bit11 = 1 << 11,
        Bit12 = 1 << 12,
        Bit13 = 1 << 13,
        Bit14 = 1 << 14,
        Bit15 = 1 << 15,
        Bit16 = 1 << 16,
        Bit17 = 1 << 17,
        Bit18 = 1 << 18,
        Bit19 = 1 << 19,
        Bit20 = 1 << 20,
        Bit21 = 1 << 21,
        Bit22 = 1 << 22,
        Bit23 = 1 << 23,
        Bit24 = 1 << 24,
        Bit25 = 1 << 25,
        Bit26 = 1 << 26,
        Bit27 = 1 << 27,
        Bit28 = 1 << 28,
        Bit29 = 1 << 29,
        Bit30 = 1 << 30,
        Bit31 = 1 << 31,
    };

    #if UNITY_EDITOR
    /// <summary> HitResult在编辑器里下拉菜单中显示的名字，可以重载 </summary>
    public static class HitResultEditorNames {
        public static string[] Value {
            get => overriddenNames ?? internalNames;
            set => overriddenNames = value;
        }

        private static string[] overriddenNames;
        private static readonly string[] internalNames;

        static HitResultEditorNames() {
            internalNames = new string[32];
            for (int i = 0; i < 32; ++i) {
                internalNames[i] = ((HitResult)i).ToString();
            }
        }
    }
    #endif
    
    public static class EnumFlagBoilerplates {
        public static bool HasAnyFlag(this HitResult self, HitResult any) => (self & any) != 0;
        public static bool HasAllFlag(this HitResult self, HitResult all) => (self & all) == all;
        public static bool HasAnyFlagExcept(this HitResult self, HitResult any, HitResult except) => (self & any) != 0 && (self & except) == 0;
        public static bool HasAllFlagExcept(this HitResult self, HitResult all, HitResult except) => (self & all) == all && (self & except) == 0;
    }
}
