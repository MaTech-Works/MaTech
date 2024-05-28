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
                    double k0 = i * delta;
                    double k1 = (i + 1) * delta;
                    double f0 = interpolator.Map(k0);
                    double f1 = interpolator.Map(k1);
                    if (Math.Abs(f1 - f0) > maxDelta) continue;
                    double df0 = interpolator.Derivative(k0);
                    double df1 = interpolator.Derivative(k1);
                    double dfdk = (f1 - f0) / delta;
                    if (Math.Abs(dfdk - df0) > tolerance) {
                        Debug.LogError($"[Interpolator] derivative of {interpolator.GetType()} has error > {tolerance} at k0 = {k0}, k1 = {k1}:\nf0 = {df0}, dFdk = {dfdk}\nF0 = {f0}, F1 = {f1}");
                        break;
                    }
                    if (Math.Abs(dfdk - df1) > tolerance) {
                        Debug.LogError($"[Interpolator] derivative of {interpolator.GetType()} has error > {tolerance} at k0 = {k0}, k1 = {k1}:\nf1 = {df1}, dFdk = {dfdk}\nF0 = {f0}, F1 = {f1}");
                        break;
                    }
                }
                for (int i = 0; i < n; ++i) {
                    double k0 = i * delta;
                    double k1 = (i + 1) * delta;
                    double f0 = interpolator.Map(k0);
                    double f1 = interpolator.Map(k1);
                    if (Math.Abs(f1 - f0) > maxDelta) continue;
                    double sfdk = (f0 + f1) * delta / 2;
                    double sf01 = interpolator.Integral(k1) - interpolator.Integral(k0);
                    if (Math.Abs(sfdk - sf01) > tolerance) {
                        Debug.LogError($"[Interpolator] integral of {interpolator.GetType()} has error > {tolerance} at k0 = {k0}, k1 = {k1}:\nFI = {sf01}, iFdk = {sfdk}\nF0 = {f0}, F1 = {f1}");
                        break;
                    }
                }
            }

            CheckInterpolator(linear);
            
            CheckInterpolator(holdAndChange);
            CheckInterpolator(changeAndHold);
            
            CheckInterpolator(easeInQuad);
            CheckInterpolator(easeOutQuad);
            CheckInterpolator(easeInOutQuad);
            
            CheckInterpolator(easeInSine);
            CheckInterpolator(easeOutSine);
            CheckInterpolator(easeInOutSine);
        }
        
    }
}