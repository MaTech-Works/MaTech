// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using Optional.Unsafe;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Processor {
    public partial class ProcessorBasic {
        // 以下全部非static方法都需要有完整的TimeCarrier列表，要至少在OnPreProcess之后调用。

        #region Find Time

        private const double ToleranceFindTimeOffset = -0.0005;

        private delegate bool FuncAfterCarrier<T>(Carrier carrier, in T t);
        
        /// <summary> 二分查找TimeCarrier以便计算Y值或者speed。 </summary>
        private TimeCarrier FindTimeCarrierByFunc<T>(FuncAfterCarrier<T> funcAfterCarrier, in T t, int minIndex = 0) {
            if (timeList is null) return null;
            // TODO: 封装到Timeline类，并在内部处理时根据进度决定二分查找的开始位置
            int maxIndex = timeList.Count, i = minIndex + 1;
            // 先看一眼offset是不是在minIndex的同区间，很多时候长音符结尾都不会跨区间，节省掉二分操作
            if (i >= maxIndex || funcAfterCarrier(timeList[i], t)) return timeList[minIndex];
            // C#的BinarySearch不支持不同类型的查找，而且返回值不好处理，自己实现一个，正好也好控制边界条件
            while (minIndex + 1 < maxIndex) {
                i = (minIndex + maxIndex) >> 1; // 向下取整
                if (funcAfterCarrier(timeList[i], t)) maxIndex = i;
                else minIndex = i; // 取最后一个小于等于offset的time
            }

            return timeList[minIndex];
        }
        
        private FuncAfterCarrier<TimeUnit> funcAfterCarrierByTime = (Carrier carrier, in TimeUnit time) => carrier.StartTime + ToleranceFindTimeOffset > time;
        private FuncAfterCarrier<BeatUnit> funcAfterCarrierByBeat = (Carrier carrier, in BeatUnit beat) => carrier.StartBeat > beat;
        private FuncAfterCarrier<double> funcAfterCarrierByRoll = (Carrier carrier, in double roll) => carrier.StartRoll > roll;
        
        private static readonly GenericMap<ProcessorBasic, TimeCarrier> mapFindTimeCarrier = new(map => map
            .Add<TimeUnit>(self => time => self.FindTimeCarrierByFunc(self.funcAfterCarrierByTime, time))
            .Add<BeatUnit>(self => beat => self.FindTimeCarrierByFunc(self.funcAfterCarrierByBeat, beat))
            .Add<double>(self => roll => self.FindTimeCarrierByFunc(self.funcAfterCarrierByRoll, roll))
        );
        
        public TimeCarrier FindTimeCarrierByTime(in TimeUnit time) => FindTimeCarrierByFunc(funcAfterCarrierByTime, time);
        public TimeCarrier FindTimeCarrierByBeat(in BeatUnit beat) => FindTimeCarrierByFunc(funcAfterCarrierByBeat, beat);
        public TimeCarrier FindTimeCarrierByRoll(in double roll) => FindTimeCarrierByFunc(funcAfterCarrierByRoll, roll);

        public TimeCarrier FindTimeCarrier<T>(in T t) where T : struct, ITimeUnit<T> => mapFindTimeCarrier.Map(this, t).ValueOrFailure();

        public TimeCarrier FindTimeCarrier(ITimePoint timePoint, bool findByBeat = true) => findByBeat ? FindTimeCarrierByBeat(timePoint.Beat) : FindTimeCarrierByTime(timePoint.Time);
        
        #endregion
        
        #region Effect Sampling
        
        public CarrierTiming SampleTiming(ITimePoint timePoint, bool findByBeat = true, in Variant keyword = default) => FindTimeCarrier(timePoint, findByBeat).SampleTiming(timePoint, keyword);
        
        public double SampleRoll(ITimePoint timePoint, bool findByBeat = true, in Variant keyword = default) => FindTimeCarrier(timePoint, findByBeat).SampleRoll(timePoint.Time, keyword); 
        public double SampleRoll(in TimeUnit time, in Variant keyword = default) => FindTimeCarrierByTime(time).SampleRoll(time, keyword);

        public override double CurrentRoll => SampleRoll(PlayTime.DisplayTime);
        
        public double SampleNoteSpeed(ITimePoint timePoint, bool findByBeat = true, in Variant keyword = default)
            => FindTimeCarrier(timePoint, findByBeat).SampleEffect(timePoint.Time, EffectType.NoteSpeed, keyword).TryDouble.ValueOr(1.0);
        
        public Variant SampleEffect<T>(in T t, DataEnum<EffectType> type, in Variant keyword = default) where T : struct, ITimeUnit<T>
            => FindTimeCarrier(t).SampleEffect(t, type, Effect.ValueSampler<T>(), keyword);
        
        // todo: roll is current always based on time; probably there can be an effect as mapping from beat to roll
        // todo: SampleEffect with range t (need query on all TimeCarriers in between)
        
        #endregion
    }
}