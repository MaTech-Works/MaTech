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