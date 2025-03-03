// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Common.Utils;
using Optional.Unsafe;

namespace MaTech.Gameplay.Data {
    public enum EffectType {
        None = 0,

        /// <summary>
        /// 卷轴滚动速度：float（兼容 int，Fraction）
        /// 记作比例值。在不改变 BPM 的情况下让局部谱面突然整体改变滚动速度，用于制作类似与 DDR 的软着陆效果，类似于 osu 谱面绿线在 mania 模式下的表现。
        /// </summary>
        ScrollSpeed,
        /// <summary>
        /// 音符移动速度：float（兼容 int，Fraction）
        /// 记作比例值。令区间内的音符单独改变个体的移动速度，不影响区域外音符的移动速度，也不会突然加减速，类似于 osu 谱面在 taiko 模式下的表现。
        /// </summary>
        NoteSpeed,
        /// <summary>
        /// 卷轴瞬间跳跃：float（兼容 int，Fraction）
        /// 记作时间单位。在无视卷轴速度的情况下，将卷轴（roll值）偏移一定距离；若开头结尾非0，则在开头结尾瞬移。单位与roll相同。
        /// </summary>
        ScrollOffset,

        /// <summary>
        /// 小节线间隔：Fraction（兼容 int，float；当为 float 时，用连分数算法，替代为底数 1000 以内的最接近的分数）
        /// 以 beat 单位指定小节线之间的间隔；在指令出现时，以指令开始的 beat 位置重新开始放置新小节线，无需范围。
        /// 当数值为 float 时，使用连分数算法寻找底数 1000 以内的最接近的分数作为实际使用值。
        /// </summary>
        Signature,
        /// <summary>
        /// 是否展示小节线：bool（兼容 int，float，Fraction；非零值为 true）
        /// 在区间内改变小节线显示与否。开头和结束都是包含关系，在开头和结束位置上的小节线均算作受到影响。
        /// </summary>
        ShowBar,
        
        /// <summary>
        /// 是否为副歌（高潮段）：bool（兼容int，float，Fraction；非零值为 true）
        /// 在值为 true 的期间开启一些画面特效，表示进入了歌曲或谱面的引人注意的段落。与 osu 谱面的 kiai time 同理。
        /// </summary>
        Chorus,
        
        // Tips: 可以使用 DataEnum 来扩展额外的 EffectType
        ExtensionStart = 100
    }
    
    public class Effect : TimedObject {
        public readonly DataEnum<EffectType> type;
        public readonly (Variant start, Variant end) value;
        public readonly (ITimePoint? start, ITimePoint? end) range;
        public readonly IInterpolator? interpolator;
        public readonly Variant keyword;

        /// <summary> Fallbacks when no effect exists or resolves to None on sampling effects </summary>
        public static Dictionary<DataEnum<EffectType>, Effect> GlobalFallbacks { get; } = new();
        public static void AddGlobalFallback(in DataEnum<EffectType> type, in Variant value) {
            GlobalFallbacks[type] = new(type, value, (TimePoint.MinValue, TimePoint.MaxValue));
        }

        static Effect() {
            AddGlobalFallback(EffectType.ScrollSpeed, 1.0);
            AddGlobalFallback(EffectType.NoteSpeed, 1.0);
            AddGlobalFallback(EffectType.ScrollOffset, 0.0);
            AddGlobalFallback(EffectType.Signature, 4);
            AddGlobalFallback(EffectType.ShowBar, true);
        }

        public delegate Variant Sampler<T>(in Effect effect, in T value) where T : struct;
        public static Sampler<T> SpeedSampler<T>() where T : struct, ITimeUnit<T> => (in Effect effect, in T t) => effect.SpeedAt(t);
        public static Sampler<T> ValueSampler<T>() where T : struct, ITimeUnit<T> => (in Effect effect, in T t) => effect.ValueAt(t);
        public static Sampler<Range<T>> DeltaSampler<T>() where T : struct, ITimeUnit<T> => (in Effect effect, in Range<T> range) => effect.Delta(range);
        public static Sampler<Range<T>> IntegralSampler<T>() where T : struct, ITimeUnit<T> => (in Effect effect, in Range<T> range) => effect.Integrate(range);
        
        public Variant Sample<T>(Sampler<T> sampler, in T t) where T : struct => sampler(this, t);
        
        // todo: accumulated effects, e.g. additive offset and multiplicative scales (!!! and be warned of multiplicative integral !!!)
        //public readonly IAccumulator<Variant> accumulator;
        //public static readonly FuncAccumulator<Variant> replace = new((in Variant a, in Variant b) => b);
        //public static readonly FuncAccumulator<Variant> add = new((in Variant a, in Variant b) => a.Double + b.Double);
        //public static readonly FuncAccumulator<Variant> multiply = new((in Variant a, in Variant b) => a.Double * b.Double);

        public bool Match(in Variant keyword) => this.keyword == keyword;
        public bool Match(in Variant keyword, Func<Variant, Variant, bool> match) => match(this.keyword, keyword);
        
        public Variant SpeedAt<T>(in T t) where T : struct, ITimeUnit<T> => SpeedAt(ClampedRatioOf(t), BeatRange.Length());
        public Variant SpeedAt(double t, double? w = null) {
            if (!value.start.IsNumeral || !value.end.IsNumeral) return Variant.None;
            double k = interpolator?.Derivative(t) ?? 0.0f;
            double v0 = value.start.Double, v1 = value.end.Double;
            return k * (v1 - v0) / (w ?? 1);
        }

        public Variant ValueAt<T>(in T t) where T : struct, ITimeUnit<T> => ValueAt(ClampedRatioOf(t));
        public Variant ValueAt(double t) {
            double k = interpolator?.Map(t) ?? 0.0f;
            if (value.start.IsNumeral && value.end.IsNumeral) {
                return MathUtil.Lerp(value.start.Double, value.end.Double, k);
            }
            return k >= 0.5 ? value.end : value.start;
        }
        
        public Variant Delta<T>(in Range<T> range) where T : struct, ITimeUnit<T> => Delta(range.start, range.end);
        public Variant Delta<T>(in T start, in T end) where T : struct, ITimeUnit<T> => DeltaBetween(ValueAt(end), ValueAt(start));
        public static Variant DeltaBetween(in Variant start, in Variant end) => start.IsNumeral && end.IsNumeral ? end.Double - start.Double : Variant.None;
        
        public Variant Integrate<T>(in Range<T> range) where T : struct, ITimeUnit<T> => Integrate(range.start, range.end);
        public Variant Integrate<T>(in T start, in T end) where T : struct, ITimeUnit<T> => Integrate(ClampedRatioOf(start), ClampedRatioOf(end), ClampedLength<T>((start, end)));
        public Variant Integrate(double t0, double t1, double? tw = null) {
            if (!value.start.IsNumeral || !value.end.IsNumeral) return Variant.None;
            double v0 = value.start.Double, v1 = value.end.Double;
            return (v0 + (v1 - v0) * (interpolator?.Average(t0, t1) ?? 0.0)) * (tw ?? t1 - t0); // use Average since it handles divide by 0
        }
        
        private Range<T> Range<T>() where T : struct, ITimeUnit<T> => rangeMap.Get<Range<T>>(this).ValueOrDefault();
        private double ClampedRatioOf<T>(in T t) where T : struct, ITimeUnit<T> => Range<T>().RatioOf(t, clamped: true);
        private double ClampedLength<T>(in Range<T> range) where T : struct, ITimeUnit<T> => Range<T>().Clamp(range).Length().Value;

        private static readonly GenericMap<Effect> rangeMap = new(map => map.Add(effect => effect.TimeRange).Add(effect => effect.BeatRange));
        
        public Effect(DataEnum<EffectType> type, in Variant value, ITimePoint? time, IInterpolator? interpolator = null, Variant keyword = default) {
            this.type = type;
            this.range = (time, time);
            this.value = (value, value);
            this.interpolator = interpolator;
            this.keyword = keyword;
        }
        
        public Effect(DataEnum<EffectType> type, in Variant value, (ITimePoint?, ITimePoint?) range, IInterpolator? interpolator = null, Variant keyword = default) {
            this.type = type;
            this.range = range;
            this.value = (value, value);
            this.interpolator = interpolator;
            this.keyword = keyword;
        }
        
        public Effect(DataEnum<EffectType> type, in (Variant, Variant) value, (ITimePoint?, ITimePoint?) range, IInterpolator? interpolator = null, Variant keyword = default) {
            this.type = type;
            this.range = range;
            this.value = value;
            this.interpolator = interpolator;
            this.keyword = keyword;
        }
        
        public Effect(Effect other) {
            range = other.range;
            type = other.type;
            value = other.value;
            interpolator = other.interpolator;
            keyword = other.keyword;
        }
        
        public override ITimePoint? Start => range.start;
        public override ITimePoint? End => range.end;
    }
}
