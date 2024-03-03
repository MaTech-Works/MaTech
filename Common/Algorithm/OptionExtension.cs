// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Optional;

namespace MaTech.Common.Algorithm {
    public static class OptionExtension {
        public static bool TryGet<T>(in this Option<T> self, out T result) {
            result = self.ValueOr(default(T));
            return self.HasValue;
        }

        public static bool TryAssign<T>(in this Option<T> self, ref T result) {
            if (self.HasValue) {
                result = self.Value;
                return true;
            }
            return false;
        }
    }
}