// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个允许最高32位的按位读写二进制压缩bool状态
    /// </summary>
    public struct BitStates {
        public int state;
        public bool this[int index] {
            get => index >= 0 && index < 32 && (state & (1 << index)) != 0;
            set {
                if (index < 0 && index >= 32) return;
                if (value) state |= (1 << index);
                else state &= ~(1 << index);
            }
        }

        /// <summary>
        /// 首位不为0的序号
        /// </summary>
        public int FirstValid {
            get {
                for (var i = 0; i < 32; i++) {
                    if (this[i]) return i;
                }

                return -1;
            }
        }

        public static BitStates False => new BitStates() { state = 0 };
        public static BitStates True => new BitStates() { state = ~0 };

        public BitEnumerator GetEnumerator() {
            return new BitEnumerator(state);
        }

        public override string ToString() {
            return state.ToString("X");
        }

        /// <summary>
        /// 用于遍历一个state所有为1的位的下标，从LSB到MSB
        /// </summary>
        public struct BitEnumerator {
            /// Magic Number用于计算为1的LSB的index
            /// https://github.com/stevengj/nlopt/blob/004f415c5d04dd1f616a953eb8ad078a20b72c58/src/util/sobolseq.c#L77-L104
            private static readonly int[] DECODE = new int[32] { 0, 1, 2, 26, 23, 3, 15, 27, 24, 21, 19, 4, 12, 16, 28, 6, 31, 25, 22, 14, 20, 18, 11, 5, 30, 13, 17, 10, 29, 9, 8, 7 };
            private const uint MAGIC_NUMBER = 0x05f66a47;

            /// <summary>
            /// 当前的state
            /// </summary>
            private int state;

            public BitEnumerator(int state) {
                this.state = state;
                Current = -1;
            }

            public int Current { get; private set; }

            public bool MoveNext() {
                if (state == 0) return false;
                int lowbit = state & (-state);
                Current = DECODE[(MAGIC_NUMBER * (uint)lowbit) >> 27];
                state ^= lowbit;
                return true;
            }
        }
    }
}