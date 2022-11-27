// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;

namespace MaTech.Control {
    // 把touch转换为可追踪数据的finger
    // todo: 在PlayInput内部维护一份Dictionary实例，并且对外提供这个实例供查询
    public partial class PlayInput {
        public abstract class Finger {
            /// <summary> Finger对应的多指触控id </summary>
            public abstract int ID { get; }
            /// <summary> 世界坐标系下的投射光线 </summary>
            public abstract Ray Ray { get; }
            /// <summary> 根据模式/游戏设置/谱面定义的映射得到的二维整型坐标，作为独立于画面的判定依据，例如ouen的舞台坐标或slide的横向位置（建议用定点小数提高精度） </summary>
            public abstract Vector2Int Coord { get; }
            /// <summary> 输入状态，在Up并触发回调后，会设置为Idle并回归对象池重用 </summary>
            public abstract FingerPhase Phase { get; }
            /// <summary> 输入位置对应的键位index，根据触控区域而判断；可以有多个Finger对应同一个index（无视穿透），但是同一个index只会交替触发isDown </summary>
            public abstract BitStates KeyState { get; }
        }

        /// <summary> 在重构Finger为非持久struct类型前临时使用，用于存储一份finger状态的拷贝 </summary>
        public class FingerClone : Finger {
            public override int ID => id;
            public override Ray Ray => ray;
            public override Vector2Int Coord => coord;
            public override FingerPhase Phase => phase;
            public override BitStates KeyState => state;

            private int id;
            private Ray ray;
            private Vector2Int coord;
            private FingerPhase phase;
            private BitStates state;

            public FingerClone() => Reset();
            public FingerClone(Finger other) => CopyFrom(other);

            public void Reset() {
                id = -1;
                phase = FingerPhase.Idle;
            }

            public void CopyFrom(Finger other) {
                id = other.ID;
                ray = other.Ray;
                coord = other.Coord;
                phase = other.Phase;
                state = other.KeyState;
            }
        }

        public class FingerDictionary {
            public const int defaultFingerCapacity = PlayInput.fingerCapacity;
        
            public abstract class FingerRecord : Finger {
                public override Ray Ray => ray;
                public override Vector2Int Coord => coord;
                public override BitStates KeyState => state;
                public Ray ray;
                public Vector2Int coord;
                public BitStates state = BitStates.False;
            }

            private class FingerRecordPrivate : FingerRecord {
                public override int ID => id;
                public override FingerPhase Phase => phase;
                public int id;
                public FingerPhase phase = FingerPhase.Idle;
            }
            
            private readonly Dictionary<int, FingerRecordPrivate> dict;

            public FingerDictionary(int initCapacity = defaultFingerCapacity) {
                dict = new Dictionary<int, FingerRecordPrivate>(initCapacity);
            }

            public FingerRecord CopyFinger(Finger other) {
                var result = GetFinger(other.ID, other.Phase);
                result.ray = other.Ray;
                result.coord = other.Coord;
                result.state = other.KeyState;
                return result;
            }

            public FingerRecord GetFinger(int id, FingerPhase phase) {
                FingerRecordPrivate finger = null;

                lock (dict) {
                    if (phase != FingerPhase.Down) {
                        if (dict.TryGetValue(id, out finger)) {
                            if (phase == FingerPhase.Up) {
                                dict.Remove(id);
                            }
                        }
                    }
                    if (finger == null) {
                        finger = new FingerRecordPrivate() { id = id };
                        if (phase != FingerPhase.Up) {
                            dict[id] = finger;
                        }
                    }
                }
                
                finger.phase = phase;
                return finger;
            }
            
        }

        public const int fingerCapacity = 10;

        private readonly FingerDictionary fingers = new FingerDictionary();
        private readonly List<int> keyFingerCount = new List<int>(20);
        
        private void SetFingerState(int id, Vector2 screenPosition, FingerPhase phase) {
            if (!inputEnabled) return;
            if (UnityUtil.IsNull(touchReferenceCamera)) return;
            var ray = touchReferenceCamera.ScreenPointToRay(screenPosition);

            var surface = TouchSurfaceBase.ActiveTouchSurface;
            var coord = surface?.GetInputCoordFromRay(ray) ?? TouchSurfaceBase.invalidCoord;
            
            var finger = fingers.GetFinger(id, phase);
            finger.ray = ray;
            finger.coord = coord;
            
            if (phase == FingerPhase.Down) {
                OnTouchDown(finger);
            } else if (phase == FingerPhase.Move) {
                OnTouchMove(finger);
            } else if (phase == FingerPhase.Up) {
                OnTouchUp(finger);
            }
        }

        private void ForceReleaseFinger(int id) {
            OnTouchUp(fingers.GetFinger(id, FingerPhase.Up));
        }
        
    }
}
