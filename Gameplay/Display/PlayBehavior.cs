// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

// disable warning for async interfaces
// (need to write async there to indicate the usage)
#pragma warning disable 1998

namespace MaTech.Gameplay.Display {
    /// <summary>
    /// 继承这个基类以及额外接口以接受ChartPlayer广播信息。
    /// async的接口将被对应的操作异步等待，比如说ChartPlayer.Load会等待OnLoad，捕获错误后会等待OnError结束后Unload。
    /// 
    /// <code>
    /// 接口调用流程：
    /// 加载关卡 OnLoad
    /// ↑ ↓
    /// | 开始关卡 OnStart(false)
    /// | 重开关卡 OnStart(true)  （可能在无OnFinish的情况下重复回调）
    /// | ↑ ↑                    （暂停状态重开关卡时无OnResume回调）
    /// | | 暂停关卡 OnPause
    /// | | 解除暂停 OnResume
    /// | ↓ ↑ ↓
    /// | (游玩中)
    /// | ↓
    /// | 通关结束 OnFinish(false) （可以靠PlayBehavior.IScoreResult获得详细成绩）
    /// | 死亡结束 OnFinish(true)
    /// ↓ ↓
    /// 卸载关卡 OnUnload
    /// </code>
    /// 
    /// 其他可选接口请参考interface注释。
    /// </summary>
    public abstract partial class PlayBehavior : MonoBehaviour {
        /// <summary> 在ChartPlayer.Load即将加载完成时调用，并等待其结束后结束 </summary>
        public virtual async UniTask OnLoad(IPlayInfo playInfo) { }
        /// <summary> 在ChartPlayer.Unload刚开始时调用，等待其结束后再继续卸载其他资源 </summary>
        public virtual async UniTask OnUnload(IPlayInfo playInfo) { }
        
        /// <summary> 当ChartPlayer捕获异常后调用，等待其结束后再完成Unload等其他善后处理 </summary>
        public virtual async UniTask OnError(Exception exception) { }
        
        public virtual void OnStart(bool isRetry) { }
        public virtual void OnFinish(bool isFailed) { }
        
        public virtual void OnPause() { }
        public virtual void OnResume() { }
    }
}