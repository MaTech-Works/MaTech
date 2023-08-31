// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
#if UNITY_EDITOR && MATECH_TEST
using UnityEditor;
#endif

namespace MaTech.Common.Tools {
    // TODO: 换成任意Unit Test框架
    [AttributeUsage(AttributeTargets.Method)]
    public class TestInitializeOnLoadMethodAttribute
    #if UNITY_EDITOR && MATECH_TEST
        : InitializeOnLoadMethodAttribute
    #else
        : Attribute
    #endif
    {
        
    }
}