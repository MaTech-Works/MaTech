// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Input;
using MaTech.Gameplay.Scoring;
using UnityEngine;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        [StructLayout(LayoutKind.Explicit, Pack = 4)]
        private struct InputData {
            public enum Type { Key, Touch, Indexed };
            [FieldOffset(0)] public Type type;
            [FieldOffset(4)] public int index;
            [FieldOffset(4)] public KeyCode keyCode;
            [FieldOffset(8)] public bool isDown;
            [FieldOffset(4)] public int touchIndex;
            [FieldOffset(12)] public TimeUnit judgeTime;
        }

        private readonly List<InputData> listPendingInput = new List<InputData>(32);
        private readonly List<InputData> listPendingInputDumped = new List<InputData>(32);
        
        private MetaTable<ScoreType> pendingScoreSnapshot;
        
        private void OnKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime) {
            if (judgeLogic == null) return;
            foreach (var behavior in PlayBehavior.ListKeyInputEarly) {
                behavior.OnKeyInputEarly(keyCode, isDown, judgeTime);
            }
            lock (listPendingInput) {
                listPendingInput.Add(new InputData {
                    type = InputData.Type.Key,
                    keyCode = keyCode,
                    isDown = isDown,
                    judgeTime = judgeTime,
                });
            }
        }

        private void OnTouchInput(PlayInput.Finger finger, TimeUnit judgeTime) {
            if (judgeLogic == null) return;
            foreach (var behavior in PlayBehavior.ListTouchInputEarly) {
                behavior.OnTouchInputEarly(finger, judgeTime);
            }
            lock (listPendingInput) {
                listPendingInput.Add(new InputData {
                    type = InputData.Type.Touch,
                    touchIndex = AllocateTouchIndex(finger),
                    judgeTime = judgeTime,
                });
            }
        }

        private void OnIndexedInput(int index, bool isDown, TimeUnit judgeTime) {
            if (judgeLogic == null) return;
            foreach (var behavior in PlayBehavior.ListIndexedInputEarly) {
                behavior.OnIndexedInputEarly(index, isDown, judgeTime);
            }
            lock (listPendingInput) {
                listPendingInput.Add(new InputData {
                    type = InputData.Type.Indexed,
                    index = index,
                    isDown = isDown,
                    judgeTime = judgeTime,
                });
            }
        }

        private void OnScoreUpdate(MetaTable<ScoreType> scoreSnapshot, TimeUnit judgeTime) {
            if (judgeLogic == null) return;
            lock (listPendingInput) {
                pendingScoreSnapshot = scoreSnapshot;
            }
        }
        
        private void FlushPendingInput() {
            listPendingInputDumped.Clear();
            lock (listPendingInput) {
                listPendingInputDumped.AddRange(listPendingInput);
                listPendingInput.Clear();
            }
            
            // Replay录制简则
            // - 对于每个Input，录制先于JudgeLogic发生，其后JudgeLogic内可后附额外录制信息
            // - FlushPendingInput的开头和结尾都flush一次record，将这部分的处理内容始终分发到一组独立的record
            // - 三种Input与JudgeLogic内不进行flush，产生的任何判定结果和输入根据judgeTime自动匹配record
            inputRecorder?.FlushRecords();
            
            foreach (var i in listPendingInputDumped) {
                switch (i.type) {
                case InputData.Type.Key: {
                    foreach (var behavior in PlayBehavior.ListKeyInput) {
                        behavior.OnKeyInput(i.keyCode, i.isDown, i.judgeTime);
                    }
                    inputRecorder?.RecordKeyInput(i.keyCode, i.isDown, i.judgeTime);
                    judgeLogic.OnKeyInput(i.keyCode, i.isDown, i.judgeTime);
                    break;
                }
                case InputData.Type.Touch: {
                    var finger = DeallocateTouchIndex(i.touchIndex);
                    foreach (var behavior in PlayBehavior.ListTouchInput) {
                        behavior.OnTouchInput(finger, i.judgeTime);
                    }
                    inputRecorder?.RecordTouchInput(finger, i.judgeTime);
                    judgeLogic.OnTouchInput(finger, i.judgeTime);
                    break;
                }
                case InputData.Type.Indexed: {
                    foreach (var behavior in PlayBehavior.ListIndexedInput) {
                        behavior.OnIndexedInput(i.index, i.isDown, i.judgeTime);
                    }
                    inputRecorder?.RecordIndexedInput(i.index, i.isDown, i.judgeTime);
                    judgeLogic.OnIndexedInput(i.index, i.isDown, i.judgeTime);
                    break;
                }
                }
            }

            // todo: 移除开源哥的todo，并且为他没有考虑操作顺序的tech debt买单
            // 以下是开源哥的注释
            //TODO 放这里合适吗？
            if (pendingScoreSnapshot != null) {
                foreach (var behavior in PlayBehavior.ListScoreUpdate) {
                    behavior.OnUpdateScore(pendingScoreSnapshot);
                }
                pendingScoreSnapshot = null;
            }
            inputRecorder?.FlushRecords();
        }

    }
}