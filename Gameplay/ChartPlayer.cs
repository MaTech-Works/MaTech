// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Cysharp.Threading.Tasks;
using MaTech.Audio;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;
using MaTech.Gameplay.Data;
using MaTech.Gameplay.Display;
using MaTech.Gameplay.Input;
using MaTech.Gameplay.Scoring;
using UnityEngine;

namespace MaTech.Gameplay {
    public partial class ChartPlayer : MonoBehaviour {
        public IPlayInfo SourcePlayInfo { get; private set; }
        public Chart LoadedChart { get; private set; }

        public IScore Score => judgeLogic.Score;
        
        public ReplayFile ReplayFile => replayFileSource?.ReplayFile;

        public Processor.Processor Processor => processor; // todo: replace with a timeline class

        [Space]
        [Header("Play Components")]
        
        [SerializeField]
        private Processor.Processor processor;

        [SerializeField] private SampleSequencer sequencer;
        [SerializeField] private PlayInput playInput;
        [SerializeField] private JudgeLogicBase judgeLogic;

        [Space]
        
        [SerializeField] private NoteLayer[] noteLayers;
        [SerializeField] private BarLayer[] barLayers;

        [Space]
        [Header("Gameplay Options")]
       
        [Tooltip("继续游戏的时候，谱面以怎样的速度回退多少时间，单位秒")]
        public AnimationCurve timeCurveResumeRewind = AnimationCurve.EaseInOut(0, 0, 1, -2);
        [Tooltip("继续游戏以后多少时间允许再次暂停")]
        public double timePauseCooldown = 1;
        
        [Space]
        [Header("Display Options")]
        
        [Tooltip("Override the display window of assigned NoteLayer and BarLayer to be the provided value below.")]
        public bool overrideLayerDisplayWindow = true;
        [Tooltip("图形轴的显示范围（较晚一侧），在此范围内音符的图形不会被移除。")]
        public double displayWindowUpY = 1;
        [Tooltip("图形轴的显示范围（较早一侧），在此范围内音符的图形不会被移除。")]
        public double displayWindowDownY = -0.1;

        [Space]
        [Header("Audio & Time Options")]
        
        public double offsetTrackStart = -2;
        public double offsetFinishCheck = 2;
        
        [Space]
        [Header("Misc Options")]
        
        public bool finishByJudgeLogic = true;
        public bool finishWhenFailed = true;
        public bool pauseWhenFailed = true;
        public bool pauseOnError = true;
        public bool unloadOnError = true;

        public bool IsLoaded => loaded;
        public bool IsPlaying => playing;
        public bool IsFinished => judgeLogic.IsFinished;
        public bool IsResumeRewinding => rewinding;
        public bool AllowPause => playing && PlayTime.JudgeTime.Seconds >= timeNextAllowedPause;
        public int PlayCount => playCount;

        private bool loaded;
        private bool playing;
        private bool finishing;
        private bool rewinding;
        
        private int playCount;

        private QueueList<TimeCarrier> timeList;

        private TimeCarrier nextTime;
        private TimeCarrier currTime;

        private PlayTime.Setter timeSetter;

        private IReplayFileSource replayFileSource;
        private IReplayRecordInput inputRecorder;

        private double timeTrackStart;
        private double timeFinishCheck;

        private float unityTimeLastResume;  // 对应Unity的Time.time
        private double audioTimeLastResume;  // 对应ChartAudioPlayer的时间
        
        private double timeNextAllowedPause = double.MinValue;
        
        private double speedScale = 1.0;
        public double SpeedScale {
            get => speedScale;
            set {
                speedScale = value;
                UpdateObjectLayerSpeedScale(speedScale);
            }
        }

        private bool NeedFrameUpdate => playing || rewinding;

        #if UNITY_EDITOR
        public void OnValidate() {
            if (processor == null) processor = GetComponent<Processor.Processor>();
            if (sequencer == null) sequencer = GetComponent<SampleSequencer>();
            if (playInput == null) playInput = GetComponent<PlayInput>();
            if (judgeLogic == null) judgeLogic = GetComponent<JudgeLogicBase>();
        }
        #endif

        public async UniTask<bool> Load(IPlayInfo playInfo, bool fullReload = true) {
            if (CheckBusy("Load"))
                return false;

            if (playing) {
                timeSetter.SetPlaying(false);
                playing = false;
                DisableController();
                sequencer.Pause();
            }

            if (loaded && fullReload) {
                await Unload();
            }

            using var busy = SetBusy(BusyReason.Loading);

            /////////////////////////////////

            #region Prepare for loading

            #if UNITY_EDITOR
            // todo: display error in inspector instead
            if (processor == null) Debug.LogError("<b>[ChartPlayer]</b> Processor not assigned for ChartPlayer", this);
            if (judgeLogic == null) Debug.LogError("<b>[ChartPlayer]</b> JudgeLogic not assigned for ChartPlayer", this);
            if (playInput == null) Debug.LogError("<b>[ChartPlayer]</b> PlayInput not assigned for ChartPlayer", this);
            if (noteLayers == null || noteLayers.Length == 0) Debug.LogError("<b>[ChartPlayer]</b> NoteLayer not assigned for ChartPlayer", this);
            #endif

            var chart = playInfo.Chart;

            if (chart == null) {
                Debug.LogError("<b>[ChartPlayer]</b> No chart to be loaded");
                return false;
            }

            SourcePlayInfo = playInfo;

            #endregion

            /////////////////////////////////

            #region Game Data: Parse and process the chart

            await UniTask.SwitchToThreadPool();

            if (fullReload || !chart.FullLoaded) {
                var parser = chart.CreateParser();
                parser.Parse(false);
            }

            if (!chart.FullLoaded) {
                Debug.LogError("<b>[ChartPlayer]</b> Failed to full load the chart");
                await UniTask.SwitchToMainThread();
                return false;
            }

            processor.PlayInfo = playInfo;
            processor.Process();

            timeList = processor.ResultTimeList;
            if (timeList == null) {
                Debug.LogError("<b>[ChartPlayer]</b> Failed to process the chart");
                await UniTask.SwitchToMainThread();
                return false;
            }

            timeTrackStart = (playInfo.TrackStartTime?.Seconds ?? 0) + offsetTrackStart;
            timeFinishCheck = (playInfo.FinishCheckTime?.Seconds ?? 0) + offsetFinishCheck;

            LoadedChart = chart;

            #endregion

            /////////////////////////////////

            #region Game Rules: Judge logic, input, and replay recorder

            // todo: 将以下的一切数据的来源打包成一个ModeRule类，来规定模式相关的所有数据，递交模式特定的派生类组件
            if (judgeLogic != null) {
                judgeLogic.OnLoadChart(playInfo, processor);

                // todo: 需要禁用成绩上传时，增加一步操作：“将加密成绩计算替换为明文成绩计算，使得最终成绩无法被上传并接受”
                /* if (playInfo.playBy == PlayByType.ReplayPlayer) {
                    if (playInfo.Replay != null) {
                        Controller = new ReplayPlayback(playInfo.Replay);
                    } else {
                        Debug.LogError("<b>[ChartPlayer]</b> Replay is requested without a replay file.");
                    }
                } else */ if (playInfo.NeedAutoPlay) {
                    if (judgeLogic.AutoPlayController != null) {
                        Controller = judgeLogic.AutoPlayController;
                    } else {
                        Debug.LogError("<b>[ChartPlayer]</b> Auto play is requested without an auto play controller.");
                    }
                } else if (playInput != null) {
                    Controller = playInput;
                }

                IReplayRecorder recorder;
                /* if (Controller != null && Controller.IsPlayer) {
                    recorder = new ReplayRecorder();
                } else */ {
                    recorder = new DummyReplayRecorder();
                }

                recorder.StartRecording(playInfo);
                replayFileSource = recorder;
                inputRecorder = recorder;
                judgeLogic.Recorder = recorder;
            }

            #endregion

            /////////////////////////////////

            #region Audio: Track and key sounds

            if (fullReload) {
                Debug.Log("<b>[ChartPlayer]</b> Full loading SampleTrack");
                var track = chart.sampleTrack;
                if (await track.Load()) {
                    sequencer.Track = track;
                } else {
                    Debug.LogError("<b>[ChartPlayer]</b> SampleTrack failed to load. Audio might not be able to start at all.");
                }
            } else {
                Debug.Log("<b>[ChartPlayer]</b> Not full loading, skipping SampleTrack");
            }

            #endregion

            /////////////////////////////////

            #region Graphics: The object layers

            if (fullReload) {
                // Notice that on non-full-reloading, objects originally on the layer are still loaded here,
                // which means updating settings will trigger graphic updates and thus required to be on the main thread.
                // Here we avoided switching threads because settings don't need to be updated.
                // However, there should be a better way to universally setup display windows instead of relying on operation orders,
                // which is another story.
                UpdateObjectLayerSettings();
            }
            
            await noteLayers.Select(layer => layer.Load(processor.ResultNoteList));
            await barLayers.Select(layer => layer.Load(processor.ResultBarList));

            #endregion

            /////////////////////////////////

            #region Finish loading

            await UniTask.SwitchToMainThread();

            // TODO: 对外暴露Offset设置接口
            // TODO: 将PlayTime的类型定义封装成ChartPlayer的nested类
            timeSetter = new PlayTime.Setter {
                offsetDisplay = 0,
                offsetAudio = 0,
            };
            timeSetter.UpdateTime(timeTrackStart, true);
            
            await PlayBehavior.ListAll.WhenAll(behavior => behavior.OnLoad(SourcePlayInfo));
            
            loaded = true;
            playCount = 0;

            #endregion

            return true;
        }

        public async UniTask Unload() {
            if (CheckBusy("Unload")) return;
            if (!loaded) return;
            
            using var busy = SetBusy(BusyReason.Unloading);

            timeSetter.SetPlaying(false);
            playing = false;
            
            await PlayBehavior.ListAll.WhenAll(behavior => behavior.OnUnload(SourcePlayInfo));
            await sequencer.Track.Unload();
            
            Controller = null;
            loaded = false;
        }

        public void Play() {
            if (CheckBusy("Play")) return;
            if (!loaded || playing) return;

            bool isRetry = playCount > 0;
            PlayBehavior.ListAll.ForEach(isRetry ? behavior => behavior.OnStart(true) : behavior => behavior.OnStart(false));

            UpdateSettings();

            timeList.Restart();
            currTime = timeList.Next();
            nextTime = timeList.PeekOrDefault();

            timeSetter.UpdateTime(timeTrackStart, true);
            timeSetter.SetPlaying(true);

            playing = true;
            finishing = false;
            playCount += 1;
            timeNextAllowedPause = double.MinValue;

            ResetController();
            EnableController();

            UpdateTime();
            isEarlyUpdateScheduled = false;
            isLateUpdateScheduled = false;
            ScheduleUpdateForNextFrame();
            
            sequencer.Play(timeTrackStart);
        }
        
        public void Pause() {
            if (CheckBusy("Pause")) return;
            if (!loaded || !playing) return;

            timeNextAllowedPause = PlayTime.ChartTime + timePauseCooldown;

            DisableController();

            sequencer.Pause();
            
            timeSetter.SetPlaying(false);
            playing = false;
            
            PlayBehavior.ListAll.ForEach(behavior => behavior.OnPause());
        }

        /// <summary>
        /// 恢复谱面播放，默认有倒退效果
        /// </summary>
        /// <param name="skipRewinding">跳过倒退，从暂停点立即恢复</param>
        public void Resume(bool skipRewinding = false) {
            if (CheckBusy("Resume")) return;
            if (!loaded || playing || rewinding) return;

            PlayBehavior.ListAll.ForEach(behavior => behavior.OnResume());
            
            UpdateSettings();

            // TODO 移除rewind，并允许外部代码任意控制回放时间
            if (skipRewinding) {
                ResumeImmediate();
            } else {
                unityTimeLastResume = UnityEngine.Time.time;
                audioTimeLastResume = PlayTime.AudioTime;
                rewinding = true;
            }
            
            UpdateTime();
            ScheduleUpdateForNextFrame();
        }

        private void ResumeImmediate() {
            timeSetter.SetPlaying(true);
            rewinding = false;
            playing = true;
            EnableController();
            sequencer.Resume();
        }

        private void Finish(bool isFailed) {
            if (isFailed && pauseWhenFailed) Pause();

            if (UnityUtil.IsAssigned(judgeLogic)) {
                using var listLock = PlayBehavior.ListScoreResult.LockRAII();
                if (!PlayBehavior.ListScoreResult.IsEmpty) {
                    var scoreSnapshot = judgeLogic.UpdateScoreSnapshot();
                    PlayBehavior.ListScoreResult.ForEach(behavior => behavior.OnFinishWithScore(isFailed, scoreSnapshot));
                }
            }
            
            PlayBehavior.ListAll.ForEach(behavior => behavior.OnFinish(isFailed));
        }
        
        private async UniTaskVoid HandleError(Exception ex) {
            Debug.LogException(ex);
            if (pauseOnError) Pause();
            await PlayBehavior.ListAll.WhenAll(behavior => behavior.OnError(ex));
            if (unloadOnError) await Unload();
        }

    }
}