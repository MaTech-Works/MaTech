﻿// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Cysharp.Threading.Tasks;
using MaTech.Common.Utils;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Processor;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        
        void LateUpdate() {
            if (!NeedFrameUpdate) return;
            ScheduleUpdateForNextFrame();
        }

        async void OnDestroy() {
            await Unload();
        }

        // 这个函数一定要在LateUpdate时至下一帧EarlyUpdate前调用，否则规划的update会错位。
        private void ScheduleUpdateForNextFrame() {
            if (actionEarlyUpdate == null) actionEarlyUpdate = UpdateEarly;
            if (actionLateUpdate == null) actionLateUpdate = UpdateLate;
            if (!isEarlyUpdateScheduled) {
                PlayerLoopHelper.AddContinuation(PlayerLoopTiming.EarlyUpdate, actionEarlyUpdate);
                isEarlyUpdateScheduled = true;
            }
            if (!isLateUpdateScheduled) {
                PlayerLoopHelper.AddContinuation(PlayerLoopTiming.PreLateUpdate, actionLateUpdate);
                isLateUpdateScheduled = true;
            }
        }

        private Action actionEarlyUpdate;
        private Action actionLateUpdate;
        private bool isEarlyUpdateScheduled = false;
        private bool isLateUpdateScheduled = false;

        /// Update Time --> Judge --> Input --> Judge
        private void UpdateEarly() {
            isEarlyUpdateScheduled = false;
            if (!NeedFrameUpdate) return;
            
            Profiler.BeginSample("ChartPlayer.UpdateEarly()", this);

            try {
                TimeUnit judgeTimeBeforeInput = PlayTime.JudgeTime;
                UpdateTime();
                TimeUnit judgeTimeAfterInput = PlayTime.JudgeTime;
                
                bool canUpdateJudgeLogic = !finishing && UnityUtil.IsAssigned(judgeLogic);
                if (canUpdateJudgeLogic) {
                    judgeLogic.OnUpdateLogicBeforeInput(judgeTimeBeforeInput, judgeTimeAfterInput);
                }
                
                UpdateController();
                FlushPendingInput();
                
                if (canUpdateJudgeLogic) {
                    judgeLogic.OnUpdateLogicAfterInput(judgeTimeBeforeInput, judgeTimeAfterInput);
                }

            } catch (Exception ex) {
                HandleError(ex).Forget();
            } finally {
                Profiler.EndSample();
            }
        }

        /// Update Graphics --> Callback --> Hardcoded Logic
        private void UpdateLate() {
            isLateUpdateScheduled = false;
            if (!NeedFrameUpdate) return;
            
            Profiler.BeginSample("ChartPlayer.UpdateLate()", this);
            
            try {
                foreach (var layer in noteLayers) {
                    if (UnityUtil.IsAssigned(layer))
                        layer.UpdateGraphics();
                }
                foreach (var layer in barLayers) {
                    if (UnityUtil.IsAssigned(layer))
                        layer.UpdateGraphics();
                }

                if (!finishing && PlayTime.ChartTime > timeFinishCheck) {
                    // ReSharper disable once SimplifyConditionalTernaryExpression  <-- if in doubt, try it yourself
                    finishing = finishByJudgeLogic && UnityUtil.IsAssigned(judgeLogic) ? judgeLogic.IsFinished : true;
                    if (finishing) {
                        replayFileSource.FinishRecording(SourcePlayInfo, Score);
                        Finish(false);
                    }
                }

                if (finishWhenFailed && judgeLogic.IsFailed) {
                    Finish(true);
                }

            } catch (Exception ex) {
                HandleError(ex).Forget();
            } finally {
                Profiler.EndSample();
            }
        }

        private void UpdateTime() {
            if (timeSetter == null) return;

            if (rewinding) {
                var deltaTimeIn = Time.time - unityTimeLastResume;
                var deltaTimeOut = timeCurveResumeRewind.Evaluate(Time.time - unityTimeLastResume);
                timeSetter.UpdateTime(audioTimeLastResume + deltaTimeOut, true);
                if (deltaTimeIn > timeCurveResumeRewind.GetEndTime()) {
                    sequencer.Seek(audioTimeLastResume + deltaTimeOut);
                    ResumeImmediate();
                }
            }
            
            if (playing) {
                timeSetter.UpdateTime(sequencer.PlayTime, true);
            }

            if (rewinding || playing) {
                timeSetter.UpdateDisplayY(processor.CurrentRoll);
            }
        }

        private void UpdateObjectLayerWindows() {
            double judgeWindowUp = 0;
            double judgeWindowDown = 0;

            var timing = UnityUtil.IsUnassigned(judgeLogic) ? null : judgeLogic.Timing;
            if (timing != null) {
                judgeWindowUp = timing.WindowEarly.Seconds;
                judgeWindowDown = -timing.WindowLate.Seconds;
            }

            var effectiveDisplayWindowUp = displayWindowUpY;
            var effectiveDisplayWindowDown = displayWindowDownY;

            foreach (var layer in noteLayers) {
                if (UnityUtil.IsUnassigned(layer)) continue;
                if (!layer.overrideJudgeWindow) {
                    layer.judgeWindowUp = judgeWindowUp;
                    layer.judgeWindowDown = judgeWindowDown;
                }
                if (!layer.overrideDisplayWindow) {
                    layer.displayWindowUpY = effectiveDisplayWindowUp;
                    layer.displayWindowDownY = effectiveDisplayWindowDown;
                }
            }

            foreach (var layer in barLayers) {
                if (UnityUtil.IsUnassigned(layer)) continue;
                if (!layer.overrideJudgeWindow) {
                    layer.judgeWindowUp = 0;
                    layer.judgeWindowDown = 0;
                }
                if (!layer.overrideDisplayWindow) {
                    layer.displayWindowUpY = effectiveDisplayWindowUp;
                    layer.displayWindowDownY = effectiveDisplayWindowDown;
                }
            }
        }

        private void UpdateObjectLayerSpeedScale() {
            foreach (var layer in noteLayers) {
                if (UnityUtil.IsAssigned(layer)) {
                    layer.UpdateSpeedScale(speedScale, loaded);
                }
            }
            foreach (var layer in barLayers) {
                if (UnityUtil.IsAssigned(layer)) {
                    layer.UpdateSpeedScale(speedScale, loaded);
                }
            }
        }
        
        private void CollectLayersFromScene() {
            noteLayers = FindObjectsByType<NoteLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            barLayers = FindObjectsByType<BarLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
    }
}