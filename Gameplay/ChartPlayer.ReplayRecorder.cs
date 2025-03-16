// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using MaTech.Gameplay.Data;
using MaTech.Gameplay.Input;
using MaTech.Gameplay.Logic;
using UnityEngine;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        // 需要直播功能时，以下接口和实现都可以复用

        public interface IReplayFileSource {
            ReplayFile ReplayFile { get; }
            void StartRecording(IPlayInfo playInfo);
            void FinishRecording(IPlayInfo playInfo, IScore score);
        }
        
        public interface IReplayRecordInput {
            void FlushRecords();
            void RecordKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime);
            void RecordTouchInput(PlayInput.Finger finger, TimeUnit judgeTime);
            void RecordIndexedInput(int index, bool isDown, TimeUnit judgeTime);
        }
        
        public interface IReplayRecordJudgeScore {
            void RecordJudgeNoteHit(JudgeLogicBase.NoteHitAction action, TimeUnit judgeTime, HitResult result);
            void RecordJudgeEmptyHit(JudgeLogicBase.EmptyHitAction action, TimeUnit judgeTime);
            void RecordScoreUpdate(IScore score, TimeUnit judgeTime);
        }
        
        private interface IReplayRecorder : IReplayFileSource, IReplayRecordInput, IReplayRecordJudgeScore {}
        
        // TODO: 清理malody相关meta项
        /*
        private class ReplayRecorder : IReplayRecorder {
            public ReplayFile ReplayFile { get; private set; }

            private int activeRecordsStartIndex = Int32.MaxValue; // 从何处开始为cell匹配time相同的record
            private int nextRecordIndex = Int32.MinValue;

            public void StartRecording(PlayInfo playInfo) {
                var chart = playInfo.Chart;
                var song = playInfo.Song;
                if (chart == null) return;

                var chartMeta = chart.meta;
                var songMeta = song.meta;
                //var onlineUser = OnlineInfo.SingletonInstance.Self;

                ReplayFile = new ReplayFile {
                    Title = songMeta.Title,
                    Artist = songMeta.Artist,
                    Creator = chartMeta.Creator,
                    Version = chartMeta.Version,
                    ChartHash = chart.Hash,
                    //Player = onlineUser.userName,
                    //UserId = (uint) Math.Max(0, onlineUser.userId),
                    Seed = playInfo.randomSeed
                };
                if (playInfo.customJudge != null) {
                    ReplayFile.Judge = new ReplayFile.Types.Judge {
                        FingerChange = playInfo.customJudge.allowFingerChange,
                        JudgeAtEnd = playInfo.customJudge.judgeAtHoldEnd,
                        MissWindow = playInfo.customJudge.ignoreMissWindow,
                        Speed = playInfo.customJudge.audioSpeed
                    };
                }

                activeRecordsStartIndex = 0;
                nextRecordIndex = 0;
            }

            public void FinishRecording(PlayInfo playInfo, Score score) {
                ReplayFile.Time = TimeUtil.Now;
                ReplayFile.Score = new ScoreRecord {
                    Mode = (uint)playInfo.playMode,
                    Mod = (uint)playInfo.ModForPlay,
                    Rank = (uint)score.FinalRank,
                    Level = (uint)playInfo.Level,
                    Pro = playInfo.proJudge,
                    Score = score.FinalScore,
                    Combo = score.FinalCombo,
                    Custom = playInfo.customJudge != null ? playInfo.customJudge.Format() : "",
                    Best = (uint)score.GetTimingMaskCount(HitResult.Best),
                    Cool = (uint)score.GetTimingMaskCount(HitResult.Cool),
                    Good = (uint)score.GetTimingMaskCount(HitResult.Good),
                    Miss = (uint)score.GetTimingMaskCount(HitResult.Miss),
                    Acc = score.FinalAcc,
                    Time = TimeUtil.Now, // 原本需要保存时的时间，这里暂时以finish时间为准，若有需求请在别处覆盖
                    //OnlineRank = ,
                    //ReplayFile = ,
                    Fc = score.FinalFullCombo
                };
            }
            
            /// <summary>
            /// 保证flush操作之后添加的cell，绝对不会放入比flush操作更早创建的record内。
            /// 否则，cell会寻找time相同的record并将自己添加到序列中。
            /// </summary>
            public void FlushRecords() {
                activeRecordsStartIndex = ReplayFile.Records.Count;
            }

            /// <summary>
            /// 将一个cell放到time相同的record内，保证不会放到最近一次flush操作前已有的record中。
            /// </summary>
            private void AddCellToRecord(ReplayFile.Types.Cell cell, TimeUnit judgeTime) {
                Assert.IsNotNull(cell, "null cells not supported by default. what do you want to do?");

                ReplayFile.Types.Record activeRecord = null;
                for (int i = activeRecordsStartIndex, n = ReplayFile.Records.Count; i < n; ++i) {
                    var record = ReplayFile.Records[i];
                    if (record.Time == judgeTime) {
                        activeRecord = record;
                        break;
                    }
                }

                if (activeRecord == null) {
                    activeRecord = new ReplayFile.Types.Record {
                        Time = judgeTime,
                        Index = nextRecordIndex++,
                    };
                    ReplayFile.Records.Add(activeRecord);
                }

                activeRecord.Cells.Add(cell);
                foreach (var behavior in PlayBehavior.ListReplayHander) {
                    behavior.OnNewReplay(cell, judgeTime);
                }
            }

            public void RecordKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime) {
                if (isDown) {
                    AddCellToRecord(new ReplayFile.Types.Cell {InputKeyDown = (uint)keyCode}, judgeTime);
                } else {
                    AddCellToRecord(new ReplayFile.Types.Cell {InputKeyUp = (uint)keyCode}, judgeTime);
                }
            }

            public void RecordTouchInput(PlayInput.Finger finger, TimeUnit judgeTime) {
                var cell = new ReplayFile.Types.CellInputTouch {
                    Id = finger.ID,
                    Phase = (int)finger.Phase,
                    State = finger.KeyState.state,
                };
                ReplayUtil.SerializeRay(finger.Ray, cell.Ray);
                ReplayUtil.SerialVector2Int(finger.Coord, cell.Coord);

                AddCellToRecord(new ReplayFile.Types.Cell {InputTouch = cell}, judgeTime);
            }

            public void RecordIndexedInput(int index, bool isDown, TimeUnit judgeTime) {
                if (index < 0) return;
                if (isDown) {
                    AddCellToRecord(new ReplayFile.Types.Cell {InputIndexedDown = (uint)index}, judgeTime);
                } else {
                    AddCellToRecord(new ReplayFile.Types.Cell {InputIndexedUp = (uint)index}, judgeTime);
                }
            }

            public void RecordJudgeNoteHit(JudgeLogicBase.NoteHitResult result, int index, TimeUnit judgeTime, HitResult result) {
                var cell = new ReplayFile.Types.CellHitNote {
                    Index = index,
                    Result = (int)result,
                    Timing = (int)result,
                };

                AddCellToRecord(new ReplayFile.Types.Cell {HitNote = cell}, judgeTime);
            }

            public void RecordJudgeEmptyHit(JudgeLogicBase.EmptyHitResult result, int index, TimeUnit judgeTime) {
                var cell = new ReplayFile.Types.CellHitEmpty {
                    Index = index,
                    Result = (int)result,
                };

                AddCellToRecord(new ReplayFile.Types.Cell {HitEmpty = cell}, judgeTime);
            }

            public void RecordScoreUpdate(Score score, TimeUnit judgeTime) {
                AddCellToRecord(new ReplayFile.Types.Cell {Score = score.FinalScore}, judgeTime);
                AddCellToRecord(new ReplayFile.Types.Cell {Combo = score.FinalCombo}, judgeTime);
                AddCellToRecord(new ReplayFile.Types.Cell {Acc = score.FinalAcc}, judgeTime);
                AddCellToRecord(new ReplayFile.Types.Cell {Hp = score.FinalHP}, judgeTime);
            }
        }
        */
        
        private class DummyReplayRecorder : IReplayRecorder {
            public ReplayFile ReplayFile => null;

            public void StartRecording(IPlayInfo playInfo) {}
            public void FinishRecording(IPlayInfo playInfo, IScore score) {}

            public void FlushRecords() {}

            public void RecordKeyInput(KeyCode keyCode, bool isDown, TimeUnit judgeTime) {}
            public void RecordTouchInput(PlayInput.Finger finger, TimeUnit judgeTime) {}
            public void RecordIndexedInput(int index, bool isDown, TimeUnit judgeTime) {}

            public void RecordJudgeNoteHit(JudgeLogicBase.NoteHitAction action, TimeUnit judgeTime, HitResult result) {}
            public void RecordJudgeEmptyHit(JudgeLogicBase.EmptyHitAction action, TimeUnit judgeTime) {}
            public void RecordScoreUpdate(IScore score, TimeUnit judgeTime) {}
        }

    }
}