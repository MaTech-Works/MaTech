// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MaTech.Play {
    /// <summary>
    /// 继承这个类的组件，可以实现以上接口以接受Play广播信息。
    /// 
    /// 接口调用流程：
    /// OnLoad --> OnStart --(完成关卡)--> OnExit(true)
    ///                    --(暂停)--> OnPause --(继续)--> OnResume
    ///                                       --(重开)--> OnStart
    ///                                       --(退出)--> OnExit(false)
    ///
    /// 其他可选接口请参考interface注释。
    /// </summary>
    public abstract partial class PlayBehavior : MonoBehaviour {
        public virtual async UniTask OnLoad(PlayInfo playInfo) { }
        public virtual async UniTask OnStart(bool isRetry) { }
        public virtual async UniTask OnExit(bool isPlayFinished) { }
        public virtual async UniTask OnPause() { }
        public virtual async UniTask OnResume() { }
    }
}