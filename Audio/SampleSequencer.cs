// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace MaTech.Audio {
    /// <summary>
    /// 谱面音轨的序列器，填入一个SampleTrack即可播放。
    /// 可以实例化多份，可以播放同个track，但是必须在主线程使用。
    /// </summary>
    public partial class SampleSequencer : MonoBehaviour {
        [FormerlySerializedAs("bufferTime")]
        [SerializeField] private double bufferOffset = 0.5;
        public double BufferOffset => bufferOffset;

        public double Length { get; private set; }

        [SerializeField] private double loopTime = Double.PositiveInfinity;
        public double LoopTime {
            get => loopTime;
            set => loopTime = value;
        }
	    
        public SampleTrack Track {
            get => track;
            set {
                Stop();
                
                track = value;
                trackSamplesQueued = false;
                
                int sampleCount = queueSample.Count;
                listAudioPlaying.Capacity = Math.Max(listAudioPlaying.Capacity, sampleCount);
                listAudioPool.Capacity = Math.Max(listAudioPool.Capacity, sampleCount);
                
                Reload();
            }
        }

        public bool IsPlaying => playing;
        public bool IsEnded => PlayTime > Length;
        
        public double PlayTimeRaw => playing ? DSPTime : timeWhenStopped;
        public double PlayTime => PlayTimeRaw; // todo: use smooth timer

        public void Play(double startTime = 0) {
            Assert.IsTrue(track.IsLoaded, "SampleTrack needs to be prepared before playing in SampleSequencer");
            Reload();
            startDspTime = MaAudio.OutputDSPTime - startTime;
            playing = true;
        }

        public void Stop() {
            Pause();
            timeWhenStopped = 0;
        }

        public void Resume() {
            Play(timeWhenStopped);
        }

        public void Pause() {
            if (!playing) return;
            timeWhenStopped = PlayTime;
            playing = false;
            StopAllSamples();
        }

        public void Toggle() {
            if (playing) Pause();
            else Resume();
        }

        public void Seek(double position) {
            bool wasPlaying = playing;
            Pause();
            timeWhenStopped = position;
            if (wasPlaying) Resume();
        }
        
        // todo: implement methods below
        public float Volume { get; set; } = 1; // todo: add per-mixer volume
        public float Speed { get; set; } = 1; // todo: add pitch shifting library

        public void FadeIn(double duration, double delay = 0) { } // todo: add mixer effect & one mixer per sequencer
        public void FadeOut(double duration, double delay = 0) { }
        public void ClearFade() { }

    }
}