// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        // TODO: 加入protobuf并启用以下代码
        /*
        protected class ReplayPlayback : IPlayController {
            // Malody 5.0 Replay 格式
            //
            // replay文件由若干meta信息与最终成绩，以及一个record列表组成。
            // record列表中的record可能是无序排列的，但会记录其判定时间time与逻辑顺序index，实际回放时应当首先按时间排序，时间相同则按index排序。
            //
            // 每个record的实际数据，由内含的cell序列来表示。
            // 这个cell序列在回放时，应当始终按照在序列中的顺序，在同一批次的操作内一起生效（比如同一帧的Update回调内，用for循环完成对一个record的cell序列的操作，中途不能去处理别的record）。
            // 每个cell可以是不同的类型，如input、判定结果、成绩等，目前总计六种类型。
            // cell数据的含义由业务逻辑来实际解释。可以参考ReplayRecorder或者本类的实现，两者对数据的用法保持一致。
            //
            // 设计意图：
            // 回放时，每帧的时间码与游玩时绝对不同。
            // 但是原本游玩时一同生效的操作，回放时也应一同生效，并且保证操作顺序。
            // 反之，游玩时某一帧积攒的输入处理可能会经过多个pass，处理产生的记录不一定按照时间顺序到达replay内，但是回放时却完全可以按照时间码重排顺序。
            // 故replay需要三种关键结构：
            // - 作为不可拆分个体的记录单元（record）
            // - 以序列形式存放的数据单元序列，每个序列与记录单元一一对应（record上的cell序列）
            // - 每个数据序列的时间码与逻辑顺序，用于在时间轴上定位数据单元（record的time和index）
            // 这就对应了盛放时间码的record，与record中的cell数组，二者的结构。
            //
            // 用伪PB语法简单表示数据结构：
            // message ReplayFile {
            //   repeated Record records {
            //     uint32 time;
            //     uint32 index;
            //     repeated oneof Cell cells {
            //       ...  // 各种cell
            //     }
            //   }
            //   ...  // meta与最终成绩
            // }

            // todo: 成绩验证等功能，实现新的control接口与这里对接即可。
            //
            // 若需要实现在replay播放时同时验证成绩，需要注意：
            //
            // 每帧所有的判定成绩计算会使用队列延迟到LateUpdate左右处理（避免输入处理时间过长），
            // 成绩验证也需与这个处理队列耦合，在队列出队的时候去插入验证。
            //
            // 有一种简化处理的方式是，成绩cell均写在每个record的最后（不能先于任何input cell），
            // 回放replay并验证成绩时，即便每次update需要输出多个record，也可以只取最后一个输出的record，在**下一次update的开头**进行成绩验证。
            // 因为下一次update开始时，这一次update的成绩计算必然完成了。（没有的话就是bug，二猫给背书的）
            //
            // 离线或服务器端验证成绩不用管这个。

            public ReplayFile ReplayFile { get; }
            public ReplayPlayback(ReplayFile replay) {
                ReplayFile = replay;
            }
            
            ////////////////////////
            
            protected static readonly Comparison<Record> compareRecord = (left, right) => {
                int result = left.Time.CompareTo(right.Time);
                if (result == 0) result = left.Index.CompareTo(right.Index);
                return result;
            };
            
            protected readonly List<Record> listRecordSorted = new List<Record>();
            
            protected IPlayControl control;

            protected int lastTime = 0;
            protected int nextRecordIndex = 0;

            private const int fingerCapacity = 100; // trying to be mercy
            private readonly PlayInput.FingerDictionary fingers = new PlayInput.FingerDictionary(fingerCapacity);
            
            ////////////////////////

            public bool IsPlayer => false;
            
            public void AttachController(IPlayControl playControl) => control = playControl;
            public void DetachController() => control = null;

            public virtual void ResetControl(int time) {
                listRecordSorted.Clear();
                if (ReplayFile?.Records != null) {
                    listRecordSorted.AddRange(ReplayFile.Records);
                    listRecordSorted.Sort(compareRecord);
                }

                lastTime = time;
                nextRecordIndex = listRecordSorted.FindIndex(record => record.Time >= time);
            }

            public virtual void UpdateControl(int time) {
                Score.Snapshot scoreSnapshot = default;
                scoreSnapshot.source = Score.ScoreSource.Replay;

                var hasScore = false;
                for (int n = listRecordSorted.Count; nextRecordIndex < n; ++nextRecordIndex) {
                    var record = listRecordSorted[nextRecordIndex];
                    if (record.Time > time) break;
                    
                    if (record.Cells == null) {
                        Debug.LogError("[Replay] null cell list???");
                        continue;
                    }
                    
                    foreach (var cellMulti in record.Cells) {
                        if (cellMulti == null) {
                            Debug.LogError("[Replay] null cell???");
                            continue;
                        }
                        
                        switch (cellMulti.CellCase) {
                        case Cell.CellOneofCase.InputIndexedDown: {
                            control.PlayIndexedInput((int)cellMulti.InputIndexedDown, true, record.Time);
                            break;
                        }
                        case Cell.CellOneofCase.InputIndexedUp: {
                            control.PlayIndexedInput((int)cellMulti.InputIndexedUp, false, record.Time);
                            break;
                        }
                        case Cell.CellOneofCase.InputKeyDown: {
                            control.PlayKeyInput((KeyCode)cellMulti.InputKeyDown, true, record.Time);
                            break;
                        }
                        case Cell.CellOneofCase.InputKeyUp: {
                            control.PlayKeyInput((KeyCode)cellMulti.InputKeyUp, false, record.Time);
                            break;
                        }
                        case Cell.CellOneofCase.InputTouch: {
                            var cell = cellMulti.InputTouch;
                            var finger = fingers.GetFinger(cell.Id, (PlayInput.FingerPhase)cell.Phase);
                            finger.ray = ReplayUtil.DeserializeRay(cell.Ray);
                            finger.coord = ReplayUtil.DeserialVector2Int(cell.Coord);
                            finger.state.state = cell.State;
                            control.PlayTouchInput(finger, record.Time);
                            break;
                        }
                            
                        // todo: 对接成绩验证，见本文件开头的todo注释
                        case Cell.CellOneofCase.HitNote: break;
                        case Cell.CellOneofCase.HitEmpty: break;
                        case Cell.CellOneofCase.Score: {
                            scoreSnapshot.score = cellMulti.Score;
                            hasScore = true;
                            break;
                        }
                        case Cell.CellOneofCase.Combo: {
                            scoreSnapshot.combo = cellMulti.Combo;
                            hasScore = true;
                            break;
                        }
                        case Cell.CellOneofCase.Acc: {
                            scoreSnapshot.acc = cellMulti.Acc;
                            hasScore = true;
                            break;
                        }
                        case Cell.CellOneofCase.Hp: {
                            scoreSnapshot.hp = cellMulti.Hp;
                            hasScore = true;
                            break;
                        }
                        case Cell.CellOneofCase.None: break; // silent skip
                            
                        default:
                            Debug.LogError($"[Replay] wrong cell, case {cellMulti.CellCase} ({(int)cellMulti.CellCase})");
                            break;
                        }
                    }
                }
                
                if (hasScore) {
                    control.PlayScoreInput(scoreSnapshot, time);
                }
                lastTime = time;
            }
            
        }
        */
    }
}