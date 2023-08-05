// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using MaTech.Common.Algorithm;
using MaTech.Gameplay.Input;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        private const int fingerBufferInitCapacity = 3 * PlayInput.fingerCapacity; // assumption: max 3 input events per frame per finger
        
        private readonly PlayInput.FingerDictionary fingersMainThread = new PlayInput.FingerDictionary();

        private readonly List<PlayInput.FingerClone> fingerBuffer = new List<PlayInput.FingerClone>(fingerBufferInitCapacity);
        private readonly StackList<int> fingerBufferIndexPool = new StackList<int>(fingerBufferInitCapacity);

        private int AllocateTouchIndex(PlayInput.Finger finger) {
            lock (fingerBuffer) {
                if (fingerBufferIndexPool.TryPop(out int touchIndex)) {
                    fingerBuffer[touchIndex].CopyFrom(finger);
                } else {
                    touchIndex = fingerBuffer.Count;
                    var fingerClone = new PlayInput.FingerClone();
                    fingerClone.CopyFrom(finger);
                    fingerBuffer.Add(fingerClone);
                }
                return touchIndex;
            }
        }
        
        private PlayInput.Finger DeallocateTouchIndex(int touchIndex) {
            lock (fingerBuffer) {
                var finger = fingerBuffer[touchIndex];
                fingerBufferIndexPool.Add(touchIndex);
                return fingersMainThread.CopyFinger(finger);
            }
        }
        
    }
}