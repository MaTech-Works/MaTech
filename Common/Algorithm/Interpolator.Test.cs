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
                    double F0 = interpolator.Map(k0);
                    double F1 = interpolator.Map(k1);
                    if (Math.Abs(F1 - F0) > maxDelta) continue;
                    double f0 = interpolator.Derivative(k0);
                    double f1 = interpolator.Derivative(k1);
                    double dFdk = (F1 - F0) / delta;
                    if (Math.Abs(dFdk - f0) > tolerance) {
                        Debug.LogError($"[Interpolator] derivative of {interpolator.GetType()} has error > {tolerance} at k0 = {k0}, k1 = {k1}:\nf0 = {f0}, dFdk = {dFdk}\nF0 = {F0}, F1 = {F1}");
                        break;
                    }
                    if (Math.Abs(dFdk - f1) > tolerance) {
                        Debug.LogError($"[Interpolator] derivative of {interpolator.GetType()} has error > {tolerance} at k0 = {k0}, k1 = {k1}:\nf1 = {f1}, dFdk = {dFdk}\nF0 = {F0}, F1 = {F1}");
                        break;
                    }
                }
                for (int i = 0; i < n; ++i) {
                    double k0 = i * delta;
                    double k1 = (i + 1) * delta;
                    double F0 = interpolator.Map(k0);
                    double F1 = interpolator.Map(k1);
                    if (Math.Abs(F1 - F0) > maxDelta) continue;
                    double iFdk = (F0 + F1) * delta / 2;
                    double FI = interpolator.Integral(k1) - interpolator.Integral(k0);
                    if (Math.Abs(iFdk - FI) > tolerance) {
                        Debug.LogError($"[Interpolator] integral of {interpolator.GetType()} has error > {tolerance} at k0 = {k0}, k1 = {k1}:\nFI = {FI}, iFdk = {iFdk}\nF0 = {F0}, F1 = {F1}");
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