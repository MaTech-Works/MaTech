// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Algorithm {
    public static class CMath {
        /// <summary>
        /// Bit-mask used for extracting the exponent bits of a <see cref="double"/> (<c>0x7ff0000000000000</c>).
        /// </summary>
        public const long DBL_EXP_MASK = 0x7ff0000000000000L;
        /// <summary>
        /// The number of bits in the mantissa of a <see cref="double"/>, excludes the implicit leading <c>1</c> bit (<c>52</c>).
        /// </summary>
        public const int DBL_MANT_BITS = 52;
        /// <summary>
        /// Bit-mask used for extracting the sign bit of a <see cref="double"/> (<c>0x8000000000000000</c>).
        /// </summary>
        public const long DBL_SGN_MASK = -1 - 0x7fffffffffffffffL;
        /// <summary>
        /// Bit-mask used for extracting the mantissa bits of a <see cref="double"/> (<c>0x000fffffffffffff</c>).
        /// </summary>
        public const long DBL_MANT_MASK = 0x000fffffffffffffL;
        /// <summary>
        /// Bit-mask used for clearing the exponent bits of a <see cref="double"/> (<c>0x800fffffffffffff</c>).
        /// </summary>
        public const long DBL_EXP_CLR_MASK = DBL_SGN_MASK | DBL_MANT_MASK;
        /// <summary>
        /// Bit-mask used for clearing the sign bit of a <see cref="double"/> (<c>0x7fffffffffffffff</c>).
        /// </summary>
        public const long DBL_SGN_CLR_MASK = 0x7fffffffffffffffL;

        public static double frexp(double number, ref int exponent) {
            long bits = System.BitConverter.DoubleToInt64Bits(number);
            int exp = (int)((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
            exponent = 0;

            if (exp == 0x7ff || number == 0D)
                number += number;
            else {
                // Not zero and finite.
                exponent = exp - 1022;
                if (exp == 0) {
                    // Subnormal, scale number so that it is in [1, 2).
                    number *= System.BitConverter.Int64BitsToDouble(0x4350000000000000L); // 2^54
                    bits = System.BitConverter.DoubleToInt64Bits(number);
                    exp = (int)((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
                    exponent = exp - 1022 - 54;
                }
                // Set exponent to -1 so that number is in [0.5, 1).
                number = System.BitConverter.Int64BitsToDouble((bits & DBL_EXP_CLR_MASK) | 0x3fe0000000000000L);
            }

            return number;
        }

        public static double ldexp(double number, int exponent) {
            return scalbln(number, exponent);
        }

        public static double scalbln(double number, long exponent) {
            long bits = System.BitConverter.DoubleToInt64Bits(number);
            int exp = (int)((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
            // Check for infinity or NaN.
            if (exp == 0x7ff)
                return number;
            // Check for 0 or subnormal.
            if (exp == 0) {
                // Check for 0.
                if ((bits & DBL_MANT_MASK) == 0)
                    return number;
                // Subnormal, scale number so that it is in [1, 2).
                number *= System.BitConverter.Int64BitsToDouble(0x4350000000000000L); // 2^54
                bits = System.BitConverter.DoubleToInt64Bits(number);
                exp = (int)((bits & DBL_EXP_MASK) >> DBL_MANT_BITS) - 54;
            }
            // Check for underflow.
            if (exponent < -50000)
                return copysign(0D, number);
            // Check for overflow.
            if (exponent > 50000 || (long)exp + exponent > 0x7feL)
                return copysign(System.Double.PositiveInfinity, number);
            exp += (int)exponent;
            // Check for normal.
            if (exp > 0)
                return System.BitConverter.Int64BitsToDouble((bits & DBL_EXP_CLR_MASK) | ((long)exp << DBL_MANT_BITS));
            // Check for underflow.
            if (exp <= -54)
                return copysign(0D, number);
            // Subnormal.
            exp += 54;
            number = System.BitConverter.Int64BitsToDouble((bits & DBL_EXP_CLR_MASK) | ((long)exp << DBL_MANT_BITS));
            return number * System.BitConverter.Int64BitsToDouble(0x3c90000000000000L); // 2^-54
        }

        public static double copysign(double number1, double number2) {
            // If number1 is NaN, we have to store in it the opposite of the sign bit.
            long sign = (signbit(number2) == 1 ? DBL_SGN_MASK : 0L) ^ (System.Double.IsNaN(number1) ? DBL_SGN_MASK : 0L);
            return System.BitConverter.Int64BitsToDouble((System.BitConverter.DoubleToInt64Bits(number1) & DBL_SGN_CLR_MASK) | sign);
        }

        public static int signbit(double number) {
            if (System.Double.IsNaN(number))
                return ((System.BitConverter.DoubleToInt64Bits(number) & DBL_SGN_MASK) != 0) ? 0 : 1;
            else
                return ((System.BitConverter.DoubleToInt64Bits(number) & DBL_SGN_MASK) != 0) ? 1 : 0;
        }
    }
}