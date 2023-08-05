﻿// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace MaTech.Audio {
    public partial class AudioSample : ISampleControl, IComparable<ISampleControl> {
        private double startDspTime;
        private double timeWhenStopped;
        private double endTime = 0;
        
        private double offset; // in seconds
        public double Offset {
            get => offset;
            set => offset = value;
        }
        public double OffsetEnd => offset + (double.IsNaN(length) ? 0 : length);
        
        public double PlayTime => playing ? MaAudio.OutputDSPTime - startDspTime : timeWhenStopped;

        public ushort Channel {
            get => channel.index;
            set => channel.index = value;
        }

        public float Volume {
            get => volume;
            set => volume = value;
        }

        private bool playing;
        public bool IsPlaying => playing;
        public bool IsFinished => PlayTime >= endTime;

        public void PlayImmediate() {
            Assert.IsTrue(audio != MaAudio.NullAudio, "Sample should be loaded (or in a prepared SampleTrack) before play.");
            MaAudio.Play(audio, volume, MaAudio.Mixer.Instant, channel);
            startDspTime = MaAudio.OutputDSPTime;
            playing = true;
        }

        public void PlayDelayed(double delayTime, bool withOffset = true) {
            Assert.AreNotEqual(MaAudio.NullAudio, audio, "Sample should be loaded (or in a prepared SampleTrack) before play.");
            MaAudio.PlayDelayed(audio, delayTime, volume, MaAudio.Mixer.Instant, channel);
            playing = true;
        }

        public void PlayScheduled(double scheduledDspTime, bool withOffset = true) {
            Assert.AreNotEqual(MaAudio.NullAudio, audio, "Sample should be loaded (or in a prepared SampleTrack) before play.");
            startDspTime = scheduledDspTime + (withOffset ? offset : 0);
            MaAudio.PlayScheduled(audio, startDspTime, volume, MaAudio.Mixer.Instant, channel);
            playing = true;
        }
        
        public void Pause() {
            if (!playing) return;
            Debug.LogWarning("MaAudio does not support stopping a single sample yet.");
            timeWhenStopped = PlayTime;
            playing = false;
        }

        public void Stop() {
            Pause();
            timeWhenStopped = 0;
        }

        public void Toggle() {
            if (playing) Pause();
            else Resume();
        }

        public void Resume() {
            PlayScheduled(MaAudio.OutputDSPTime - timeWhenStopped);
        }

        public void Seek(double playTime) {
            bool wasPlaying = playing;
            Pause();
            timeWhenStopped = playTime;
            if (wasPlaying) Resume();
        }
        
        public int CompareTo(ISampleControl other) {
            return this == other ? 0 : Offset.CompareTo(other.Offset);
        }
    }
}