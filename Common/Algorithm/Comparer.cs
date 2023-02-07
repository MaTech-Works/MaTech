// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public class MappedComparer<TCompare, TSource> : IComparer<TCompare> {
        private readonly IComparer<TSource> comparer;
        private readonly Func<TCompare, TSource> map;

        public MappedComparer(IComparer<TSource> sourceComparer, Func<TCompare, TSource> mapToSource) {
            comparer = sourceComparer;
            map = mapToSource;
        }

        public int Compare(TCompare x, TCompare y) => comparer.Compare(map(x), map(y));
    }
}