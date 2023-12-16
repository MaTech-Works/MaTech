// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Common.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Time;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        public enum ObjectType {
            Generic,
            // 可以用 DataEnum 扩展定义
        }
        
        // TODO: 根据需求整理接口操作，目前留空仅作类型检查用
        public interface INoteVisualState {}
        public interface INoteJudgeState {}
        
        public class ObjectCarrier<TCarrier, TLayer> : CarrierRanged
            where TCarrier : ObjectCarrier<TCarrier, TLayer>
            where TLayer : ObjectLayer<TCarrier, TLayer> {
            /// <summary> Carrier对应的图形 </summary>
            public IObjectVisual<TCarrier, TLayer>? visual = null;

            /// <summary> 用于在ObjectLayer分选Carrier，只有类型匹配的Carrier才会在ObjectLayer实例化 </summary>
            public DataEnum<ObjectType> type;
            
            /// <summary> 物体的移动速度相对TimeCarrier.scroll的倍率 </summary>
            public double scaleY = 1;

            public double LengthOffset => EndTime - StartTime;
            public double LengthY => EndY - StartY;

            public double DeltaYStart(double displayY) => (StartY - displayY) * scaleY;
            public double DeltaYEnd(double displayY) => (EndY - displayY) * scaleY;
            public double DisplayYStart(double deltaY) => StartY - deltaY / scaleY;
            public double DisplayYEnd(double deltaY) => EndY - deltaY / scaleY;
        }

        /// <summary> note载体: 用于将逻辑note和图形note关联起来的中间桥梁，同时承载与显示相关的计算得到的note数据 </summary>
        /// TODO: 把NoteCarrier和BarCarrier合并到ObjectCarrier，不再在核心管线层面用类型区分Note和Bar等视觉元素
        public class NoteCarrier : ObjectCarrier<NoteCarrier, NoteLayer> {
            /// <summary> 原note数据，可以为null，也可以多对一，用法取决于业务代码中Processor和INoteVisual的具体实现 </summary>
            public TimedObject? note = null;
            /// <summary> note对应的图形 </summary>
            public INoteVisual? NoteVisual => visual as INoteVisual;
            
            // TODO: 移除NoteVisual和visual成员，将图形关联记录在ObjectLayer里

            /// <summary> 显示相关的额外数据，可以承载任意内容 </summary>
            public INoteVisualState? visualState = null;

            /// <summary> 音符在判定逻辑中的状态，记录所有标准或者非标准判定的状态量与结果等等。若为null则本carrier不参与判定。 </summary>
            public INoteJudgeState? judgeState = null;
        };

        /// <summary> 小节线载体: 用于承载小节线信息，没有直接对应的逻辑数据 </summary>
        /// TODO: 把NoteCarrier和BarCarrier合并到ObjectCarrier，不再在核心管线层面用类型区分Note和Bar等视觉元素
        public class BarCarrier : ObjectCarrier<BarCarrier, BarLayer> {};

    }
}