// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Gameplay.Input;
using MaTech.Gameplay.Scoring;
using MaTech.Gameplay.Time;
using UnityEngine;

namespace MaTech.Gameplay.Display {
    public partial class PlayBehavior {
        /// 比正常IKeyInput回调更早触发的回调，调用时可能不在UI线程，但是保证调用过程互斥。
        /// 适合给音频使用，不要在这个回调中用Unity API。
        public interface IKeyInputEarly {
            void OnKeyInputEarly(KeyCode keyCode, bool isDown, TimeUnit judgeTime);
        }
        
        /// 比正常ITouchInput回调更早触发的回调，调用时可能不在UI线程，但是保证调用过程互斥。
        /// 适合给音频使用，不要在这个回调中用Unity API。
        public interface ITouchInputEarly {
            void OnTouchInputEarly(PlayInput.Finger finger, TimeUnit judgeTime);
        }
        
        /// 比正常IIndexedInput回调更早触发的回调，调用时可能不在UI线程，但是保证调用过程互斥。
        /// 适合给音频使用，不要在这个回调中用Unity API。
        public interface IIndexedInputEarly {
            void OnIndexedInputEarly(int keyIndex, bool isDown, TimeUnit judgeTime);
        }

        /// 输入回调，在每帧的开始时被调用。
        public interface IKeyInput {
            void OnKeyInput(KeyCode index, bool isDown, TimeUnit judgeTime);
        }

        /// 输入回调，在每帧的开始时被调用。
        public interface ITouchInput {
            void OnTouchInput(PlayInput.Finger finger, TimeUnit judgeTime);
        }
    
        /// 输入回调，在每帧的开始时被调用。
        public interface IIndexedInput {
            void OnIndexedInput(int index, bool isDown, TimeUnit judgeTime);
        }
        
        /// 输入信息被JudgeLogic分发至某个音符后被调用；NoteBehavior上也会接收到类似的信息。
        public interface INoteHitResult {
            void OnHitNote(ChartPlayer.NoteCarrier carrier, JudgeLogicBase.NoteHitAction action, TimeUnit judgeTime, HitResult result);
            void OnHitEmpty(JudgeLogicBase.EmptyHitAction action, TimeUnit judgeTime);
        }
    
        /// 音符成绩计算结束后调用。
        public interface IScoreUpdate {
            void OnUpdateScore(MetaTable<ScoreType> scoreSnapshot);
        }
        
        /// 在即将触发PlayBehavior.OnFinish前调用。
        public interface IScoreResult {
            void OnFinishWithScore(bool isFailed, MetaTable<ScoreType> scoreSnapshot);
        }
    }
}