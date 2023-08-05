// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;

namespace MaTech.Common.Utils {
    public static class TimeUtil {
        public static TimeSpan Now => sw.Elapsed;
        public static int NowS => Now.Seconds;
        public static int NowMS => Now.Milliseconds;
        public static long NowTicks => Now.Ticks;

        public static long NowUTCSeconds => (DateTime.UtcNow.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

        private static readonly Stopwatch sw = new();
        static TimeUtil() {
            sw.Start();
        }
    }
}