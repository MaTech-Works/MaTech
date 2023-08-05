// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Common.Data;
using MaTech.Gameplay.Time;

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
        
        /// <summary>
        /// 是否为副歌（高潮段）：bool（兼容int，float，Fraction；非零值为 true）
        /// 在值为 true 的期间开启一些画面特效，表示进入了歌曲或谱面的引人注意的段落。与 osu 谱面的 kiai time 同理。
        /// </summary>
        Chorus,
        
        // Tips: 可以使用 EnumEx 来扩展额外的 EffectType
    }
    
    public class Effect : TimedObjectRanged {
        public readonly EnumEx<EffectType> type;
        public readonly Variant value;
        
        // TODO: 实现插值方法
        // TODO: 将value改为valueStart和valueEnd来对应连续变化的值
        //public readonly Func<> interpolation;

        // TODO: 如何对非浮点数据插值？
        // TODO: 将Variant类重命名成Number类
        public Variant ValueAt(TimeUnit time) => TimeUnit.IsInRangeByValue(time, Start.Time, End.Time) ? value : Variant.None;
        public Variant ValueAt(BeatUnit beat) => BeatUnit.IsInRangeByFraction(beat, Start.Beat, End.Beat) ? value : Variant.None;

        public Effect(EnumEx<EffectType> type, in Variant value, ITimePoint? injectedStart = null, ITimePoint? injectedEnd = null)
            : base(injectedStart, injectedEnd) {
            this.type = type;
            this.value = value;
        }
        
        public Effect(Effect other) : base(other.Start, other.End) {
            this.type = other.type;
            this.value = other.value;
        }
    }
}
