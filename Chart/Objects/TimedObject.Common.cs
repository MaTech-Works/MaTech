using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

#nullable enable

namespace MaTech.Chart.Objects {
    // todo: move all classes into separate files with corresponding containers

    public class TempoChange : TimedObjectStartOnly {
        public double beatLength = 500; // milliseconds per beat

        public double BPM {
            get => 60000 / beatLength;
            set => beatLength = 60000 / value;
        }

        public double CalculateTimeFromBeat(double beat) => (beat - t.Beat) * beatLength + t.Time;
        public double CalculateBeatFromTime(double time) => (time - t.Time) / beatLength + t.Beat;
    }

    public static class TempoChangeExtensionMethods {
        public static void UpdateTimeFromBeat(this TimePoint t, TempoChange o) => t.Time = o.CalculateTimeFromBeat(t.Beat);
        public static void UpdateBeatFromTime(this TimePoint t, TempoChange o) => t.Beat = o.CalculateBeatFromTime(t.Time);
    }

    public enum EffectType {
        Invalid = 0,

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
        /// 记作时间单位。以当前的卷轴速度，在区间开头和结尾将卷轴瞬间移动一定距离（让音符在瞬移后的位置继续排列）；开头向时间的正方向瞬移，结尾向时间的负方向瞬移。
        /// 这个的效果比较难理解，建议不要轻易更改代码实现。
        /// </summary>
        ScrollJump, 

        /// <summary>
        /// 小节线间隔：Fraction（兼容 int，float；当为 float 时，用连分数算法，替代为底数 1000 以内的最接近的分数）
        /// 以 beat 单位指定小节线之间的间隔；在指令出现时，以指令的 beat 位置重新开始放置新小节线。
        /// 当数值为 float 时，使用连分数算法寻找底数 1000 以内的最接近的分数作为实际使用值。
        /// </summary>
        Signature,
        /// <summary>
        /// 是否展示小节线：bool（兼容 int，float，Fraction；非零值为 true）
        /// 在区间内改变小节线显示与否。开头和结束都是包含关系，在开头和结束位置上的小节线均算作受到影响。
        /// </summary>
        ShowBar,
    }
    
    /// <summary>
    /// Effect object.
    /// All properties are nullable, meaning whether the property is changed at this timing, or it is effective for the next period of the chart.
    /// </summary>
    public class Effect : TimedObjectRanged {
        public EnumEx<EffectType> type = EffectType.Invalid;
        public Variant value;

        public Effect(EnumEx<EffectType> type, in Variant value) {
            this.type = type;
            this.value = value;
        }
        public Effect(Effect other) : base(other.Start, other.End) {
            this.type = other.type;
            this.value = other.value;
        }
    }
}
