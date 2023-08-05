// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public class ListPool<T> : ObjectPool<List<T>> {
        public ListPool(int capacityPool, int initCapacityList)
            : base(() => new List<T>(initCapacityList), list => list.Clear(), null, capacityPool) { }
    }
}