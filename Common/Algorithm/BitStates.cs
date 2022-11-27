// Copyright (c) 2022, Findstr, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This source file is adapted from the 32-bit rightmost index algorithm from MIT.
// Original license below:

/* Copyright (c) 2007 Massachusetts Institute of Technology
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

namespace MaTech.Common.Algorithm {
    /// 一个允许最高32位的按位读写二进制压缩bool状态
    /// 从 Findstr 的版本整理而来
    public struct BitStates {
        public uint state;

        public bool this[int index] {
            get => (index & ~0x1f) == 0 && (state & (1u << index)) != 0;
            set {
                uint mask = (index & ~0x1f) == 0 ? (1u << index) : 0;
                state = value ? (state | mask) : (state & ~mask);
            }
        }

        /// 从LSB开始，第一位为1的下标
        public int FirstValid => Private.FirstValid(state);

        public static BitStates False => new BitStates() { state = 0u };
        public static BitStates True => new BitStates() { state = ~0u };

        public BitEnumerator GetEnumerator() => new BitEnumerator(state);

        public override string ToString() => state.ToString("X");

        /// 从LSB到MSB遍历所有为1的位，输出下标
        public struct BitEnumerator {
            private uint state;

            public BitEnumerator(uint state) {
                this.state = state;
                Current = -1;
            }

            public int Current { get; private set; }

            public bool MoveNext() {
                if (state == 0) return false;
                uint lowbit = Private.LowBit(state);
                Current = Private.BitIndex(lowbit);
                state ^= lowbit;
                return true;
            }
        }

        private static class Private {
            /// Magic Number，用于计算为1的LSB的index
            /// https://github.com/stevengj/nlopt/blob/004f415c5d04dd1f616a953eb8ad078a20b72c58/src/util/sobolseq.c#L77-L104
            private static readonly int[] DECODE = new int[32] { 0, 1, 2, 26, 23, 3, 15, 27, 24, 21, 19, 4, 12, 16, 28, 6, 31, 25, 22, 14, 20, 18, 11, 5, 30, 13, 17, 10, 29, 9, 8, 7 };
            private const uint MAGIC_NUMBER = 0x05f66a47;
            
            public static uint LowBit(uint state) => state & (uint)-(int)state;
            public static int BitIndex(uint bit) => DECODE[(MAGIC_NUMBER * bit) >> 27];
            public static int FirstValid(uint state) => state == 0 ? -1 : BitIndex(LowBit(state));
        }
    }
}