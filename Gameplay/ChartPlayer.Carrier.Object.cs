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

        // TODO: 把这个基类与模板参数干掉
        public class ObjectCarrier<TCarrier, TLayer> : Carrier
            where TCarrier : ObjectCarrier<TCarrier, TLayer>
            where TLayer : ObjectLayer<TCarrier, TLayer> {
            
            /// <summary> 用于在ObjectLayer分选Carrier，只有类型匹配的Carrier才会在ObjectLayer实例化 </summary>
            /// TODO: 移动至unit
            public DataEnum<ObjectType> type;
            
            /// <summary> 物体的移动速度相对TimeCarrier.scroll的倍率 </summary>
            /// TODO: 移动至unit
            public double scaleY = 1;

            public double DeltaYStart(double displayY) => (StartY - displayY) * scaleY;
            public double DeltaYEnd(double displayY) => (EndY - displayY) * scaleY;
            public double DisplayYStart(double deltaY) => StartY - deltaY / scaleY;
            public double DisplayYEnd(double deltaY) => EndY - deltaY / scaleY;
        }

        /// <summary> note载体: 用于将逻辑note和图形note关联起来的中间桥梁，同时承载与显示相关的计算得到的note数据 </summary>
        /// TODO: 把NoteCarrier和BarCarrier合并到ObjectCarrier，不再在核心管线层面用类型区分Note和Bar等视觉元素
        public partial class NoteCarrier : ObjectCarrier<NoteCarrier, NoteLayer> {
            /// <summary> Carrier对应的实际游戏功能。多个Carrier上相同的unit对象会被视为同一个单元，在诸多游戏逻辑中会以具体的unit为准更新逻辑。 </summary>
            public IUnit[]? units = null;
        };

        /// <summary> 小节线载体: 用于承载小节线信息，没有直接对应的逻辑数据 </summary>
        /// TODO: 把NoteCarrier和BarCarrier合并到ObjectCarrier，不再在核心管线层面用类型区分Note和Bar等视觉元素
        public class BarCarrier : ObjectCarrier<BarCarrier, BarLayer> {};

    }
}