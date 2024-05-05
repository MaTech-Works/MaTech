// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using MaTech.Gameplay.Data;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        /// <summary> 时间载体: 用于承载与显示相关的时间轴相关数据，同时对应TimePoint和Effect数据，所有数据自计算结束后应当保持不再改变 </summary>
        public class TimeCarrier : Carrier {
            // TODO: 将effect数值封装到一个时间轴类里，隐藏本类的实现细节
            // TODO: 不再使用Carrier盛放这一时间轴信息，考虑重用CarrierTiming结构

            /// <summary> 本TimeCarrier至下一个TimeCarrier之间的谱面滚动速度，对时间积分即为Y值 </summary>
            public double speed = 1;

            /// <summary> speed受到effect影响，相对无effect时的倍数；已经计入speed中，使用speed值时无需再度乘上本值 </summary>
            public double scrollVelocity = 1;
            /// <summary> 音符移动速度相对speed的倍数，不参与speed的运算，但是会与noteVelocityScale相乘并生效于区间内单个音符的 </summary>
            public double noteVelocity = 1;
            /// <summary> 本TimeCarrier生效瞬间的卷轴位置变化，单位秒，与当前的speed相乘并生效到Y值上 </summary>
            public double jumpTime = 0;
            /// <summary> 谱面是否为处于副歌段或者类似片段，用于模仿某些模式的段落特效 </summary>
            public bool chorus = false;

            /// <summary> 经过adjust hs处理的hs值缩放比例 </summary>
            public double noteVelocityScale = 1;
            /// <summary> 实际传递给区间内的所有的ObjectCarrier的hs值 </summary>
            public double EffectiveNoteVelocity => noteVelocity * noteVelocityScale;

            /// <summary> 当前生效的Tempo </summary>
            public TempoChange tempo = null!;

            /// <summary> 当前生效的Effect </summary>
            public Effect[] effects = Array.Empty<Effect>();
            /// <summary> 本TimeCarrier开始生效的effect，不存在则为null </summary>
            public Effect? effectActivated = null;
            /// <summary> 本TimeCarrier取消生效的effect，不存在则为null </summary>
            public Effect? effectDeactivated = null;

            /// <summary> 从另一个TimeCarrier抄来所有可能受到effect影响的值，包括effects容器本身（因为是静态的数据，故以引用形式复制） </summary>
            public void CopyEffectFrom(TimeCarrier other) {
                speed = other.speed;
                scrollVelocity = other.scrollVelocity;
                noteVelocity = other.noteVelocity;
                jumpTime = 0; // 始终清空
                chorus = other.chorus;
                effects = other.effects;
                effectActivated = null;
                effectDeactivated = null;
            }
        }
    }
}