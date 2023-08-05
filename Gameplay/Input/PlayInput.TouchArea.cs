// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using MaTech.Common.Data;

namespace MaTech.Gameplay.Input {
    // 把finger转换为index的部分
    public partial class PlayInput {
        private readonly Dictionary<Finger, BitStates> activeHold = new Dictionary<Finger, BitStates>(fingerCapacity);
        private readonly Dictionary<Finger, BitStates> activeSlide = new Dictionary<Finger, BitStates>(fingerCapacity);

        private readonly List<TouchAreaBase> touchAreasCached = new List<TouchAreaBase>(20);
        private readonly object mutexTouchAreas = new object();

        private void UpdateCachedTouchAreas() {
            lock (mutexTouchAreas) {
                touchAreasCached.Clear();
                touchAreasCached.AddRange(TouchAreaBase.activeAreasOrdered);
            }
        }

        /// <summary>
        /// 根据finger新的位置更新finger.state
        /// </summary>
        private void UpdateFingerState(Finger finger, out BitStates slideAreas, out BitStates holdAreas) {
            slideAreas = BitStates.False;
            holdAreas = BitStates.False;

            if (!(finger is FingerDictionary.FingerRecord fingerRecord)) return;
            
            fingerRecord.state = BitStates.False;
            foreach (var area in touchAreasCached) {
                if (!area.RayCast(fingerRecord.ray, out _))
                    continue;

                fingerRecord.state[area.KeyIndex] = true;
                if (area.allowSlide) {
                    slideAreas[area.KeyIndex] = true;
                } else {
                    holdAreas[area.KeyIndex] = true;
                }
                
                if (!area.penetrate) {
                    break;
                }
            }
        }

        private void OnTouchDown(Finger finger) {
            lock (mutexTouchAreas) {
                UpdateFingerState(finger, out var slideAreas, out var holdAreas);
                
                foreach (int keyIndex in holdAreas) {
                    IncreaseFingerCountAtIndex(keyIndex);
                }
                foreach (int keyIndex in slideAreas) {
                    IncreaseFingerCountAtIndex(keyIndex);
                }

                SendTouchInput(finger);

                activeHold.Add(finger, holdAreas);
                activeSlide.Add(finger, slideAreas);
            }
        }

        private void OnTouchUp(Finger finger) {
            lock (mutexTouchAreas) {
                var holdAreas = activeHold[finger];
                var slideAreas = activeSlide[finger];

                UpdateFingerState(finger, out _, out _);

                foreach (int keyIndex in holdAreas) {
                    DecreaseFingerCountAtIndex(keyIndex);
                }
                foreach (int keyIndex in slideAreas) {
                    DecreaseFingerCountAtIndex(keyIndex);
                }

                SendTouchInput(finger);

                activeHold.Remove(finger);
                activeSlide.Remove(finger);
            }
        }

        private void OnTouchMove(Finger finger) {
            lock (mutexTouchAreas) {
                UpdateFingerState(finger, out var currentSlideAreas, out _);

                var previousSlideAreas = activeSlide[finger];
                var appearingSlideAreas = new BitStates { state = currentSlideAreas.state & ~previousSlideAreas.state };
                var disappearingSlideAreas = new BitStates { state = previousSlideAreas.state & ~currentSlideAreas.state };

                foreach (int keyIndex in appearingSlideAreas) {
                    IncreaseFingerCountAtIndex(keyIndex);
                }

                foreach (int keyIndex in disappearingSlideAreas) {
                    // 这里正常情况不会触发前个循环中增加的area对应的index，
                    // 因为将keyFingerCount增加到1前不可能有同样的index的area被按下
                    DecreaseFingerCountAtIndex(keyIndex);
                }

                SendTouchInput(finger);

                activeSlide[finger] = currentSlideAreas;
            }
        }

    }
}
