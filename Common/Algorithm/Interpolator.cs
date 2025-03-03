// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Utils;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 将标准化至[0,1]的插值进度k映射到[0,1]的新曲线上的映射定义，与映射函数的导数与积分定义
    /// </summary>
    public interface IInterpolator {
        /// <summary> 将插值进度k映射到新值，不需要对超出定义域的范围进行clamp或saturate </summary>
        double Map(double t);
        /// <summary> map函数在k处的导数，仅在[0,1]范围内保证正确 </summary>
        double Derivative(double t);
        /// <summary> map函数的<b>不定积分</b>，仅在[0,1]范围内保证正确，常数任意，k=0时不必返回0，需自行用减法计算定积分 </summary>
        double Integral(double t);
    }
    
    public static partial class Interpolators {
        public static readonly IInterpolator linear = new Presets.Linear();
        
        public static readonly IInterpolator changeAtStart = new Presets.ChangeAtStart();
        public static readonly IInterpolator changeAtEnd = new Presets.ChangeAtEnd();
        
        public static readonly IInterpolator easeInQuad = new Presets.EaseInQuad();
        public static readonly IInterpolator easeOutQuad = new Presets.EaseOutQuad();
        public static readonly IInterpolator easeInOutQuad = new Presets.EaseInOutQuad();
        
        public static readonly IInterpolator easeInSine = new Presets.EaseInSine();
        public static readonly IInterpolator easeOutSine = new Presets.EaseOutSine();
        public static readonly IInterpolator easeInOutSine = new Presets.EaseInOutSine();
        
        // todo: exponent curve & factory methods from ratio
    }

    public static class InterpolatorExtensions {
        public static double Average(this IInterpolator self, double t0, double t1)
            => t0.Near(t1, 1e-6) ? (self.Map(t1) - self.Map(t0)) / 2 : (self.Integral(t1) - self.Integral(t0)) / (t1 - t0);
    }
}