// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Data;
using MaTech.Gameplay.Time;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Processor {
    public partial class ProcessorBasic {
        // 以下全部非static方法都需要有完整的TimeCarrier列表，要至少在OnPreProcess之后调用。

        #region Comparisons

        private const float toleranceFindTimeOffset = -0.5f;

        /// <summary> 二分查找的标准：carrier是否比beat更晚，绝对无法用来计算数值；会查找列表里最后一个得到false的carrier </summary>
        private bool IsAfterTimeCarrierByBeat(TimeCarrier carrier, Fraction beat) {
            return carrier.StartBeat > beat;
        }
        
        /// <summary> 二分查找的标准：carrier是否比offset更晚，绝对无法用来计算数值；会查找列表里最后一个得到false的carrier </summary>
        private bool IsAfterTimeCarrierByTime(TimeCarrier carrier, double offset) {
            return carrier.StartTime + toleranceFindTimeOffset > offset;
        }
        
        #endregion
        
        #region Find Time
        
        /// <summary>
        /// 二分查找TimeCarrier以便计算Y值或者speed。
        /// </summary>
        private TimeCarrier FindTimeCarrierGeneric<T>(Func<TimeCarrier, T, bool> funcIsAfterTimeCarrier, T t, int minIndex = 0) {
            // TODO: 指数搜索，根据ProcessNote时的context决定二分查找的开始位置
            // TODO: 封装到Timeline类
            int maxIndex = timeList.Count, i = minIndex + 1;
            // 先看一眼offset是不是在minIndex的同区间，很多时候长音符结尾都不会跨区间，节省掉二分操作
            if (i >= maxIndex || funcIsAfterTimeCarrier(timeList[i], t)) return timeList[minIndex];
            // C#的BinarySearch不支持不同类型的查找，而且返回值不好处理，自己实现一个，正好也好控制边界条件
            while (minIndex + 1 < maxIndex) {
                i = (minIndex + maxIndex) >> 1; // 向下取整
                if (funcIsAfterTimeCarrier(timeList[i], t)) maxIndex = i;
                else minIndex = i; // 取最后一个小于等于offset的time
            }

            return timeList[minIndex];
        }
        
        // https://github.com/dotnet/csharplang/issues/2404
        // MS says we need to cache the func objects by ourselves :(
        private Func<TimeCarrier, Fraction, bool> funcIsAfterTimeCarrierByBeat;
        private Func<TimeCarrier, double, bool> funcIsAfterTimeCarrierByTime;
        private Func<TimeCarrier, Fraction, bool> FuncIsAfterTimeCarrierByBeat => funcIsAfterTimeCarrierByBeat ??= IsAfterTimeCarrierByBeat;
        private Func<TimeCarrier, double, bool> FuncIsAfterTimeCarrierByTime => funcIsAfterTimeCarrierByTime ??= IsAfterTimeCarrierByTime;

        public TimeCarrier FindTimeCarrierByBeat(in Fraction beat) => FindTimeCarrierGeneric(FuncIsAfterTimeCarrierByBeat, beat);
        public TimeCarrier FindTimeCarrierByOffset(double offset) => FindTimeCarrierGeneric(FuncIsAfterTimeCarrierByTime, offset);
        public TimeCarrier FindTimeCarrier(ITimePoint timePoint, bool findByBeat = true) => findByBeat ?
            FindTimeCarrierGeneric(FuncIsAfterTimeCarrierByBeat, timePoint.Beat) : FindTimeCarrierGeneric(FuncIsAfterTimeCarrierByTime, timePoint.Time.Seconds);
        
        #endregion
        
        #region Y Calculation
        
        /// <summary> Y值的基础计算规则，需要指定作为参考的TimeCarrier </summary>
        public static double CalculateYFromTime(double time, TimeCarrier reference) {
            return reference.StartY + (time - reference.StartTime) * reference.speed;
        }
        
        /// <summary> 自动查找TimeCarrier计算offset对应的Y值。见<see cref="FindTimeCarrierByOffset" /> </summary>
        public double CalculateYFromTime(double time) {
            return CalculateYFromTime(time, FindTimeCarrierByOffset(time));
        }
        
        /// <summary> 自动查找TimeCarrier计算offset对应的Y值。见<see cref="FindTimeCarrier" /> </summary>
        public double CalculateY(ITimePoint timePoint, bool findTimeCarrierByBeat = true) {
            return CalculateYFromTime(timePoint.Time.Seconds, FindTimeCarrier(timePoint));
        }
        
        #endregion

        #region CarrierTiming Factory
        
        /// <summary> 从TimePoint创建一个CarrierTiming，通常传入note.start或者note.end </summary>
        protected CarrierTiming CreateTiming(ITimePoint timePoint) {
            return CarrierTiming.FromTimePoint(timePoint, CalculateY(timePoint));
        }
        
        /// <summary> 仅用offset值创建一个CarrierTiming，填入的beat值不影响计算 </summary>
        protected CarrierTiming CreateTiming(double time, Fraction beat) {
            return new CarrierTiming() {
                time = time,
                displayY = CalculateYFromTime(time),
                beat = beat,
            };
        }
        
        #endregion
        
    }
}