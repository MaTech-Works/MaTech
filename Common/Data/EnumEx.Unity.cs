// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System;
#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace MaTech.Common.Data {
    [AttributeUsage(AttributeTargets.Method)]
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
    
    [AttributeUsage(AttributeTargets.Class)]
    public class InitializeEnumExForEditorAttribute
#if UNITY_EDITOR
        : InitializeOnLoadAttribute 
#else
        : System.Attribute
#endif
    {
    }
}