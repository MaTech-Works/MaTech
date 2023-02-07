// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Utils {
    public static class CompareUtil {
        public static bool TryCompareTo<T>(in T left, in T right, out int result) where T : IComparable<T> {
            result = left.CompareTo(right);
            return result != 0;
        }
    }
}