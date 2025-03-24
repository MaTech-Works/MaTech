// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Input;
using UnityEngine;
using UnityEngine.Profiling;
using static MaTech.Gameplay.ChartPlayer;

namespace MaTech.Gameplay.Logic {
    public abstract partial class JudgeLogicBase : PlayBehavior {
        // 分发输入操作到note的处理逻辑。
        // 全部的回调都发生在主线程。
        
        #region Public Methods
    
        // todo: think about how to adjust HitEvent inspector on extending actions
        public enum NoteHitAction {
            Unknown = -1, // 无法判别交互形式时使用
            Auto = 0, // 无对应输入操作（如超时自动判miss）或无法判别交互形式时使用
            Press, Hold, Release, Flick,
            Linked, // 被其他音符连锁触发
        };

        // todo: merge into NoteHitAction with all callbacks and events
        public enum EmptyHitAction {
            Unknown = -1, // 无法判别交互形式（如不支持的input操作）时使用
            Auto = 0, // 无对应输入操作时使用
            Down, Move, Up, Flick,
        }

        public delegate void ActionHitNote(IJudgeUnit unit, NoteHitAction action, in TimeUnit time, HitResult result);

        // 以下组件getter在OnLoadChart后应当指向一个有效的实例
        // todo: 有没有更好的方法给这些组件添加工厂函数？是否将这些全部移动到一个ModeRule类，全部从外部传入这里？
        public IJudgeTiming Timing { get; protected set; }
        public IScore Score { get; protected set; }
        public AutoPlayControllerBase AutoPlayController { get; protected set; }

        // 以下组件从外部传入
        public IReplayRecordJudgeScore Recorder { get; set; }
        public ActionHitNote OnHitNote { get; set; }
        
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
        public abstract void OnLoadChart(IPlayInfo playInfo, QueueList<NoteCarrier> notes);

        /// <summary>
        /// 每帧游戏逻辑更新时被调用，与帧率关联的回调。
        /// </summary>
        public virtual void OnUpdateLogicBeforeInput(TimeUnit timeBeforeInput, TimeUnit timeAfterInput) {}
        public virtual void OnUpdateLogicAfterInput(TimeUnit timeBeforeInput, TimeUnit timeAfterInput) {}

        /// <summary>
        /// 处理按键或触控输入，详见 PlayInput 类。
        /// </summary>
        public virtual void OnIndexedInput(int index, bool isDown, TimeUnit time) {}
        public virtual void OnKeyInput(KeyCode keyCode, bool isDown, TimeUnit time) {}
        public virtual void OnTouchInput(PlayInput.Finger finger, TimeUnit time) {}

        public override void OnFinish(bool isFailed) => Score.Finish(isFailed);

        #endregion

        #region Protected Utility Methods

        /// <summary>
        /// 向判定数值Timing查询给定音符的判定结果。
        /// </summary>
        protected HitResult JudgeNoteHit(IJudgeUnit unit, NoteHitAction action, TimeUnit time) {
            return Timing?.JudgeNoteHit(unit, action, time) ?? HitResult.None;
        }
        
        /// <summary>
        /// 记录判定结果，并将判定发送至图形。
        /// </summary>
        protected void HandleNoteHit(IJudgeUnit unit, NoteHitAction action, TimeUnit time, HitResult result) {
            LogNoteHit(unit, action, time, result);
            
            // Note and Behavior callbacks
            Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): NoteBehavior.OnHit", this);
            OnHitNote(unit, action, time, result);
            Profiler.EndSample();
            
            Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): PlayBehavior.OnHitNote", this);
            foreach (var behavior in PlayBehavior.ListNoteHitResult) {
                behavior.OnHitNote(unit, action, time, result);
            }
            Profiler.EndSample();

            // Update score & Broadcast score change
            if (Score != null) {
                Profiler.BeginSample("JudgeLogicBase.HandleNoteHit(): Score.HandleScoreResult", this);
                Score.HandleScoreResult(result, time);
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
                Recorder.RecordJudgeNoteHit(action, time, result);
                if (Score != null) {
                    Recorder.RecordScoreUpdate(Score, time);
                }
                Profiler.EndSample();
            }
        }

        /// <summary>
        /// 将空击消息传递给所有子元件上的JudgeResultBehaviour
        /// </summary>
        protected void HandleEmptyHit(EmptyHitAction action, TimeUnit time) {
            LogEmptyHit(action, time);

            Profiler.BeginSample("JudgeLogicBase.HandleEmptyHit(): PlayBehavior", this);
            foreach (var behavior in PlayBehavior.ListNoteHitResult) {
                behavior.OnHitEmpty(action, time);
            }
            Profiler.EndSample();
            
            if (Recorder != null) {
                Profiler.BeginSample("JudgeLogicBase.HandleEmptyHit(): Recorder", this);
                Recorder.RecordJudgeEmptyHit(action, time);
                Profiler.EndSample();
            }
        }
        
        #endregion

    }
}
