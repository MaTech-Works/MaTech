// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Logic;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        // todo: 支持同个prefab上存在多个behavior，在prefab中任意位置接受回调，并且分开事件、状态和Hit接口
        public interface IObjectVisual<in TCarrier, in TLayer>
            where TCarrier : ObjectCarrier<TCarrier, TLayer>
            where TLayer : ObjectLayer<TCarrier, TLayer> {
            /// <summary> 元件作为新音符被显示时调用 </summary>
            void StartVisual(TCarrier carrier, TLayer layer);
            /// <summary> 元件所代表的音符结束显示时调用 </summary>
            void FinishVisual();
            /// <summary> 元件所代表的音符仍在显示时的每帧被调用 </summary>
            void UpdateVisual();
            /// <summary> 是否应当尽快结束音符显示（无论音符是否超出显示范围）；若指定IgnoreDisplayWindow为true忽视显示范围，这个属性会成为结束显示的唯一标准 </summary>
            bool IsVisualFinished { get; }
            /// <summary> 是否在超出显示范围时自动移除音符（无论音符是否标记了IsVisualFinished）；比如音符被击打后可以设置为false，以便让击打动画完整播放，完成后指定IsFinished为true结束显示 </summary>
            bool IgnoreDisplayWindow { get; }
        }

        public interface INoteVisual : IObjectVisual<NoteCarrier, NoteLayer> {
            /// <summary> 接受JudgeLogic传递的判定消息 </summary>
            void OnHit(IJudgeUnit judgeUnit, JudgeLogicBase.NoteHitAction action, in TimeUnit time, HitResult result);
        }

        public interface IBarVisual : IObjectVisual<BarCarrier, BarLayer> {}
        
    }
}