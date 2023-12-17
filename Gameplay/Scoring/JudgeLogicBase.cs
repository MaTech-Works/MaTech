// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Data;
using MaTech.Common.Tools;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Input;
using MaTech.Gameplay.Time;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaTech.Gameplay.Scoring {
    public abstract partial class JudgeLogicBase : MonoBehaviour {
        // 分发输入操作到note的处理逻辑。
        // 全部的回调都发生在主线程。
        
        #region Public Methods
    
        public enum NoteHitAction {
            Unknown = -1, // 无法判别交互形式时使用
            Auto = 0, // 无对应输入操作（如超时自动判miss）或无法判别交互形式时使用
            Hit, Hold, Release, Flick,
            Linked, // 被其他音符连锁触发
        };

        public enum EmptyHitAction {
            Unknown = -1, // 无法判别交互形式（如不支持的input操作）时使用
            Auto = 0, // 无对应输入操作时使用
            Down, Move, Up, Flick,
        }

        // 以下组件getter在OnLoadChart后应当指向一个有效的实例
        // todo: 有没有更好的方法给这些组件添加工厂函数？是否将这些全部移动到一个ModeRule类，全部从外部传入这里？
        public IJudgeTiming Timing { get; protected set; }
        public IScore Score { get; protected set; }
        public AutoPlayControllerBase AutoPlayController { get; protected set; }

        // 以下组件从外部传入
        public ChartPlayer.IReplayRecordJudgeScore Recorder { get; set; }

        public MetaTable<ScoreType> LastScoreSnapshot { get; } = new();
        public MetaTable<ScoreType> UpdateScoreSnapshot() {
            Meta.ShallowCopy(Score, LastScoreSnapshot);
            return LastScoreSnapshot;
        }
        
        #endregion
        
        #region Abstract Methods

        /// <summary> 游玩是否结束，告诉业务代码是否已经无note可打。如果始终未返回true，游戏将无法结束。 </summary>
        public abstract bool IsFinished { get; }
        
        public abstract bool IsFailed { get; }
        
        /// <summary>
        /// 当 ChartPlayer 加载并处理谱面后这个方法会被调用。
        /// </summary>
        /// <param name="playInfo">目前正在处理的游玩信息，包含已经加载完成的谱面</param>
        /// <param name="processor">已经完成处理的processor，可以取得处理结果</param>
        public abstract void OnLoadChart(IPlayInfo playInfo, Processor.Processor processor);

        /// <summary>
        /// 每帧游戏逻辑更新时被调用，与帧率关联的回调。
        /// </summary>
        public virtual void OnUpdateLogicBeforeInput(TimeUnit judgeTimeBeforeInput, TimeUnit judgeTimeAfterInput) {}
        public virtual void OnUpdateLogicAfterInput(TimeUnit judgeTimeBeforeInput, TimeUnit judgeTimeAfterInput) {}

        /// <summary>
        /// 处理按键或触控输入，详见 PlayInput 类。
        /// </summary>
        public abstract void OnIndexedInput(int index, bool isDown, TimeUnit judgeTime);
        public abstract void OnKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime);
        public abstract void OnTouchInput(PlayInput.Finger finger, TimeUnit judgeTime);
        
        #endregion

        #region Protected Utility Methods

        /// <summary>
        /// 向判定数值Timing查询给定音符的判定结果。
        /// </summary>
        protected HitResult JudgeNoteHit(ChartPlayer.NoteCarrier carrier, NoteHitAction action, TimeUnit judgeTime) {
            return Timing?.JudgeNoteHit(carrier, action, judgeTime) ?? HitResult.None;
        }
        
        /// <summary>
        /// 记录判定结果，并将判定发送至图形。
        /// </summary>
        protected void HandleNoteHit(ChartPlayer.NoteCarrier carrier, NoteHitAction action, TimeUnit judgeTime, HitResult result) {
            DebugLogHistory.PushHistory("play", $"{judgeTime}: note hit, action {action}, result {result}");
            
            // Note and Behavior callbacks
            Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): NoteBehavior.OnHit", this);
            carrier.NoteVisual?.OnHit(action, result);
            Profiler.EndSample();
            
            Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): PlayBehavior.OnHitNote", this);
            foreach (var behavior in PlayBehavior.ListNoteHitResult) {
                behavior.OnHitNote(carrier, action, judgeTime, result);
            }
            Profiler.EndSample();

            // Update score & Broadcast score change
            if (Score != null) {
                Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): Score.HandleScoreResult", this);
                Score.HandleScoreResult(result, judgeTime);
                Profiler.EndSample();

                Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): ScoreSnapshot", this);
                using var listLock = PlayBehavior.ListScoreUpdate.LockRAII();
                if (!PlayBehavior.ListScoreUpdate.IsEmpty) {
                    var scoreSnapshot = UpdateScoreSnapshot();
                    foreach (var behavior in PlayBehavior.ListScoreUpdate) {
                        behavior.OnUpdateScore(scoreSnapshot);
                    }
                }
                Profiler.EndSample();
            }

            // Record replay
            if (Recorder != null) {
                Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): Recorder", this);
                Recorder.RecordJudgeNoteHit(action, judgeTime, result);
                if (Score != null) {
                    Recorder.RecordScoreUpdate(Score, judgeTime);
                }
                Profiler.EndSample();
            }
        }

        /// <summary>
        /// 将空击消息传递给所有子元件上的JudgeResultBehaviour
        /// </summary>
        protected void HandleEmptyHit(EmptyHitAction action, TimeUnit judgeTime) {
            DebugLogHistory.PushHistory("play", $"{judgeTime}: empty hit, result {action}");
            
            Profiler.BeginSample("JudgeLogicBase.HandleEmptyHit(): PlayBehavior", this);
            foreach (var behavior in PlayBehavior.ListNoteHitResult) {
                behavior.OnHitEmpty(action, judgeTime);
            }
            Profiler.EndSample();
            
            if (Recorder != null) {
                Profiler.BeginSample("JudgeLogicBase.HandleEmptyHit(): Recorder", this);
                Recorder.RecordJudgeEmptyHit(action, judgeTime);
                Profiler.EndSample();
            }
        }
        
        #endregion

    }
}
