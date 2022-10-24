// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Play {
    public static class PlayUtil {
        public static ModMask ValidatedMod(ModMask mods, PlayModeType mode) {
            return mods & ~(ModMask.Hide | ModMask.Fair | ModMask.Origin | DisabledModForMode(mode));
        }

        public static ModMask DisabledModForMode(PlayModeType mode) {
            switch (mode) {
            case PlayModeType.Key: return ModMask.Invert;
            case PlayModeType.Catch: return ModMask.Invert | ModMask.Const | ModMask.Origin;
            case PlayModeType.Pad: return ModMask.Luck | ModMask.Const;
            case PlayModeType.Taiko: return ModMask.Invert;
            case PlayModeType.Ring: return ModMask.Origin;
            case PlayModeType.Slide: return ModMask.Invert | ModMask.Luck | ModMask.Origin;
            case PlayModeType.Live: return ModMask.Invert;
            }
            return ModMask.None;
        }

        public static ModMask ConflictedMod(ModMask mod) {
            switch (mod) {
            case ModMask.Luck:
                return ModMask.Flip | ModMask.Invert;
            case ModMask.Flip:
            case ModMask.Invert:
                return ModMask.Luck;
            case ModMask.Rush:
                return ModMask.Dash | ModMask.Slow;
            case ModMask.Dash:
                return ModMask.Rush | ModMask.Slow;
            case ModMask.Slow:
                return ModMask.Rush | ModMask.Dash;
            }

            return ModMask.None;
        }

        public static float BoostForMod(ModMask mod) {
            float ret = 1;
            if ((mod & ModMask.Const) != ModMask.None) {
                ret *= 0.95f;
            }

            if ((mod & ModMask.Dash) != ModMask.None) {
                ret *= 1.05f;
            }

            if ((mod & ModMask.Rush) != ModMask.None) {
                ret *= 1.12f;
            }

            if ((mod & ModMask.Slow) != ModMask.None) {
                ret *= 0.55f;
            }

            return ret;
        }

        public static string JudgeToString(JudgeLevel level) {
            switch (level) {
            case JudgeLevel.A: return "EASY";
            case JudgeLevel.B: return "EASY+";
            case JudgeLevel.C: return "NORMAL";
            case JudgeLevel.D: return "NORMAL+";
            case JudgeLevel.E: return "HARD";
            }

            return "";
        }

        public static string ModToString(ModMask mod) {
            switch (mod) {
            case ModMask.Auto: return "Auto";
            case ModMask.Luck: return "Luck";
            case ModMask.Flip: return "Flip";
            case ModMask.Const: return "Const";
            case ModMask.Dash: return "Dash";
            case ModMask.Rush: return "Rush";
            case ModMask.Hide: return "Hide";
            case ModMask.Origin: return "Origin";
            case ModMask.Slow: return "Slow";
            case ModMask.Death: return "Death";
            case ModMask.Fair: return "Fair";
            case ModMask.Invert: return "Invert";
            }

            return "";
        }
    }
}