// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Tools;
using UnityEngine;

namespace MaTech.Common.Algorithm {
    public static partial class Interpolators {
        // TODO: 换成任意Unit Test框架
        [TestInitializeOnLoadMethod]
        internal static void TestCalculus() {
            void CheckInterpolator(IInterpolator interpolator, int n = 1000, double tolerance = 1e-2, double maxDelta = 0.5) {
                double delta = 1.0f / n;
                for (int i = 0; i < n; ++i) {
                    double t0 = i * delta;
                    double t1 = (i + 1) * delta;
                    double f0 = interpolator.Map(t0);
                    double f1 = interpolator.Map(t1);
                    if (Math.Abs(f1 - f0) > maxDelta) continue;
                    double df0 = interpolator.Derivative(t0);
                    double df1 = interpolator.Derivative(t1);
                    double dfdt = (f1 - f0) / delta;
                    if (double.IsInfinity(dfdt)) continue;
                    if (Math.Abs(dfdt - df0) > tolerance) {
                        Debug.LogError($"[Interpolator] derivative of {interpolator.GetType()} has error > {tolerance} at t0 = {t0}, t1 = {t1}:\nf0 = {df0}, dFdt = {dfdt}\nF0 = {f0}, F1 = {f1}");
                        break;
                    }
                    if (Math.Abs(dfdt - df1) > tolerance) {
                        Debug.LogError($"[Interpolator] derivative of {interpolator.GetType()} has error > {tolerance} at t0 = {t0}, t1 = {t1}:\nf1 = {df1}, dFdt = {dfdt}\nF0 = {f0}, F1 = {f1}");
                        break;
                    }
                }
                for (int i = 0; i < n; ++i) {
                    double t0 = i * delta;
                    double t1 = (i + 1) * delta;
                    double f0 = interpolator.Map(t0);
                    double f1 = interpolator.Map(t1);
                    if (Math.Abs(f1 - f0) > maxDelta) continue;
                    double sfdt = (f0 + f1) * delta / 2;
                    double dF01 = interpolator.Integral(t1) - interpolator.Integral(t0);
                    if (Math.Abs(sfdt - dF01) > tolerance) {
                        Debug.LogError($"[Interpolator] integral of {interpolator.GetType()} has error > {tolerance} at t0 = {t0}, t1 = {t1}:\ndF01 = {dF01}, ifdt = {sfdt}\nf0 = {f0}, f1 = {f1}");
                        break;
                    }
                }
            }

            CheckInterpolator(linear);
            
            CheckInterpolator(changeAtStart);
            CheckInterpolator(changeAtEnd);
            
            CheckInterpolator(easeInQuad);
            CheckInterpolator(easeOutQuad);
            CheckInterpolator(easeInOutQuad);
            
            CheckInterpolator(easeInSine);
            CheckInterpolator(easeOutSine);
            CheckInterpolator(easeInOutSine);
        }
        
    }
}