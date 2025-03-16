// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Common.Data;
using MaTech.Gameplay.Display;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        public enum ObjectType {
            None, // 可以用 DataEnum 扩展定义
        }

        // todo: 把这个基类与模板参数干掉
        // todo: 重命名成VisualData或类似名称
        // todo: 与Processor和ObjectLayer解耦
        public class ObjectCarrier<TCarrier, TLayer> : Carrier
            where TCarrier : ObjectCarrier<TCarrier, TLayer>
            where TLayer : ObjectLayer<TCarrier, TLayer> {
            
            /// <summary> 用于在ObjectLayer分选Carrier，只有类型匹配的Carrier才会在ObjectLayer实例化 </summary>
            public DataEnum<ObjectType> type;

            /// <summary> 用来筛选effect的关键词 </summary>
            public Variant keyword;
            
            /// <summary> 物体的移动速度相对卷轴速度的倍率 </summary>
            public double scale = 1;
            
            // todo: support Range<RollUnit>

            public double DeltaRoll(double targetRoll, double noteRoll) => (noteRoll - targetRoll) * scale;
            public double TargetRoll(double deltaRoll, double noteRoll) => noteRoll - deltaRoll / scale;
            public double NoteRoll(double targetRoll, double deltaRoll) => targetRoll + deltaRoll / scale;
            
            public double DeltaRoll(double targetRoll, bool byStart) => DeltaRoll(targetRoll, byStart ? StartRoll : EndRoll);
            public double TargetRoll(double deltaRoll, bool byStart) => TargetRoll(deltaRoll, byStart ? StartRoll : EndRoll);
        }

        /// <summary> note载体: 用于将逻辑note和图形note关联起来的中间桥梁，同时承载与显示相关的计算得到的note数据 </summary>
        /// todo: 把NoteCarrier和BarCarrier合并到ObjectCarrier，不再在核心管线层面用类型区分Note和Bar等视觉元素
        /// todo: 增加一种重载ToString方法的IUnit
        public partial class NoteCarrier : ObjectCarrier<NoteCarrier, NoteLayer> {
            /// <summary> Carrier对应的实际游戏功能。多个Carrier上相同的unit对象会被视为同一个单元，在诸多游戏逻辑中会以具体的unit为准更新逻辑。 </summary>
            public IUnit[]? units = null;
        };

        /// <summary> 小节线载体: 用于承载小节线信息，没有直接对应的逻辑数据 </summary>
        /// todo: 把NoteCarrier和BarCarrier合并到ObjectCarrier，不再在核心管线层面用类型区分Note和Bar等视觉元素
        public class BarCarrier : ObjectCarrier<BarCarrier, BarLayer> {};
    }
}