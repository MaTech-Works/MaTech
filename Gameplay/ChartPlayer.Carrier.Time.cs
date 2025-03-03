// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;

namespace MaTech.Gameplay {
    using EffectLookup = Dictionary<DataEnum<EffectType>, Effect[]>;
    
    public partial class ChartPlayer {
        /// <summary> 时间载体: 用于承载与显示相关的时间轴相关数据，同时对应Tempo和Effect数据，所有数据自计算结束后应当保持不再改变 </summary>
        public class TimeCarrier : Carrier {
            /// <summary> 当前生效的Tempo </summary>
            public TempoChange? tempo = null!;
            /// <summary> 当前生效的Effect </summary>
            public EffectLookup? effects = null;
            /// <summary> 根据ReferenceBeatLength调整后的速度缩放 </summary>
            public (double roll, double note) scale = (1.0, 1.0);
            
            public Variant SampleEffect<T>(in T t, DataEnum<EffectType> type, in Variant keyword = default) where T : struct, ITimeUnit<T>
                => SampleEffect(in t, type, Effect.ValueSampler<T>(), in keyword);
            public Variant SampleEffect<T>(in T t, DataEnum<EffectType> type, Effect.Sampler<T> sampler, in Variant keyword = default) where T : struct {
                var result = Variant.None;
                if (effects?.GetValueOrDefault(type) is { } effectsOfType) {
                    foreach (var effect in effectsOfType) {
                        if (!effect.Match(keyword)) continue;
                        result = effect.Sample(sampler, t);
                    }
                }
                if (result.IsNone) {
                    if (Effect.GlobalFallbacks.TryGetValue(type, out var effect))
                        result = effect.Sample(sampler, t);
                }
                return result;
            }

            public double SampleRoll(in TimeUnit time, in Variant keyword = default) => StartRoll + SampleDeltaRoll((StartTime, time), keyword);
            public double SampleDeltaRoll(in Range<TimeUnit> range, in Variant keyword = default) {
                var distance = SampleEffect(range, EffectType.ScrollSpeed, Effect.IntegralSampler<TimeUnit>(), keyword).Double;
                var offset = SampleEffect(range, EffectType.ScrollOffset, Effect.DeltaSampler<TimeUnit>(), keyword).Double;
                return distance * scale.roll + offset;
            }
            
            public CarrierTiming SampleTiming(ITimePoint timePoint, in Variant keyword = default) => CarrierTiming.FromTimePoint(timePoint, SampleRoll(timePoint.Time));
        }
    }

    public static class TimeCarrierExtensions {
        public static EffectLookup ToLookup(this IEnumerable<Effect> effects) => effects.GroupBy(effect => effect.type).ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());
    }
}