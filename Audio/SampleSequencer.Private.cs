// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using MaTech.Common.Algorithm;
using UnityEditor;
using UnityEngine;

namespace MaTech.Audio {
    public partial class SampleSequencer {
        private class AudioInfo : IComparable<AudioInfo> {
            public AudioSample sample;
            public double endTime;
            public int CompareTo(AudioInfo other) {
                return endTime.CompareTo(other.endTime);
            }
        }

        private SampleTrack track;
        private bool trackSamplesQueued;

        private readonly QueueList<AudioSample> queueSample = new QueueList<AudioSample>();
        private readonly PriorityQueue<AudioInfo> listAudioPlaying = new PriorityQueue<AudioInfo>();
        private readonly StackList<AudioInfo> listAudioPool = new StackList<AudioInfo>();

        private bool playing;
        private double startDspTime;

        private double DSPTime => MaAudio.OutputDSPTime - startDspTime;

        private double timeWhenStopped;
        
        #if UNITY_EDITOR
        internal struct TimeDisplay {
            [NonSerialized] public double timePlaying;
            [NonSerialized] public double timeBuffered;
        }
        internal TimeDisplay timeDisplay;
        #endif

        private void Reload() {
            StopAllSamples();

            if (!trackSamplesQueued) {
                queueSample.ClearAndRestart();
                queueSample.AddRange(track.Samples);
                ShellSort.Hibbard(queueSample);

                double maxTime = Double.MaxValue;
                foreach (var sample in queueSample) {
                    maxTime = Math.Max(sample.OffsetEnd, maxTime);
                }
                Length = maxTime;

                trackSamplesQueued = true;
            } else {
                queueSample.Restart();
            }
        }

        private void StopAllSamples() {
            while (listAudioPlaying.HasNext) {
                var audioInfo = listAudioPlaying.Pop();
                RecycleAudioInfo(audioInfo);
            }
        }
        
        private void Update() {
            if (queueSample == null || !playing) return;
            double playTime = PlayTime;
            
            // Loop
            if (playTime >= loopTime) {
                Seek(playTime % loopTime);
                playTime = PlayTime;
            }
            
            // Recycle all AudioSources that has finished playing
            while (listAudioPlaying.HasNext) {
                var nextAudio = listAudioPlaying.Peek();
                if (nextAudio.endTime > playTime) break;
                listAudioPlaying.Discard();
                RecycleAudioInfo(nextAudio);
            }
            
            // buffer all audio samples before (dspTime + bufferOffset)
            double bufferTime = playTime + bufferOffset;
            while (queueSample.HasNext) {
                var nextSample = queueSample.Peek();
                if (nextSample.Offset > bufferTime) break;
                queueSample.Skip();
                PrepareAudio(nextSample);
            }
            
            #if UNITY_EDITOR
            timeDisplay.timePlaying = playTime;
            timeDisplay.timeBuffered = bufferTime;
            #endif
        }

        private void PrepareAudio(AudioSample sample) {
            AudioInfo info;
            if (listAudioPool.Count == 0) {
                info = new AudioInfo();
            } else {
                info = listAudioPool.Pop();
            }
            
            sample.PlayScheduled(startDspTime);
            
            info.sample = sample;
            info.endTime = sample.OffsetEnd;
            listAudioPlaying.Push(info);
        }

        private void RecycleAudioInfo(AudioInfo info) {
            info.sample?.Stop();
            info.sample = null;
            listAudioPool.Add(info);
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(SampleSequencer))]
    internal class SampleSequencerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            EditorGUILayout.Separator();

            var sequencer = (SampleSequencer)target;
            var display = sequencer.timeDisplay;
            
            GUILayout.BeginVertical("Time Infomation", "window");
            GUI.enabled = false;
            EditorGUILayout.DoubleField("Playing", display.timePlaying);
            EditorGUILayout.DoubleField("Buffered", display.timeBuffered);
            GUI.enabled = true;
            GUILayout.EndVertical();
        }
    }
    #endif
}