// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Data {
    public interface IBitMask {
        int MaskValue { get; }
    }

    public static class BitMaskMethods {
        public static bool HasAnyFlag(this IBitMask self, IBitMask mask) => (self.MaskValue & mask.MaskValue) != 0;
        public static bool HasAllFlag(this IBitMask self, IBitMask mask) => (self.MaskValue & mask.MaskValue) == mask.MaskValue;
    }

    /// 定义一个新BitMask需要加的boilerplate代码
    public struct BitMaskBoilerplate : IBitMask {
        public int MaskValue { get; }
        public BitMaskBoilerplate(int flagValue) => MaskValue = flagValue;

        public static implicit operator BitMaskBoilerplate(int flagValue) => new BitMaskBoilerplate(flagValue);
        public static explicit operator int(BitMaskBoilerplate mask) => mask.MaskValue;

        public static BitMaskBoilerplate operator|(BitMaskBoilerplate left, BitMaskBoilerplate right) => left.MaskValue | right.MaskValue;
        public static BitMaskBoilerplate operator&(BitMaskBoilerplate left, BitMaskBoilerplate right) => left.MaskValue & right.MaskValue;
        public static BitMaskBoilerplate operator^(BitMaskBoilerplate left, BitMaskBoilerplate right) => left.MaskValue ^ right.MaskValue;
        public static BitMaskBoilerplate operator~(BitMaskBoilerplate mask) => ~mask.MaskValue;

        public static bool operator==(BitMaskBoilerplate left, BitMaskBoilerplate right) => left.MaskValue == right.MaskValue;
        public static bool operator!=(BitMaskBoilerplate left, BitMaskBoilerplate right) => left.MaskValue != right.MaskValue;

        public bool Equals(BitMaskBoilerplate other) => MaskValue == other.MaskValue;
        public override bool Equals(object obj) => obj is BitMaskBoilerplate other && Equals(other);
        public override int GetHashCode() => MaskValue;
    }
}