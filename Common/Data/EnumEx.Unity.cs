// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace MaTech.Common.Data {
    public class InitializeEnumExMethodAttribute
#if UNITY_EDITOR
        : InitializeOnLoadMethodAttribute 
#else
        : RuntimeInitializeOnLoadMethodAttribute
#endif
    {
#if !UNITY_EDITOR
        public InitializeEnumExMethodAttribute() : base(RuntimeInitializeLoadType.SubsystemRegistration) { }
#endif
    }
}