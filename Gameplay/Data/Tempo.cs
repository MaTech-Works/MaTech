// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Gameplay.Time;

namespace MaTech.Gameplay.Data {
    public class TempoChange : TimedObjectStartOnly {
        public double beatLength = 0.5; // seconds per beat

        public double BPM {
            get => 60 / beatLength;
            set => beatLength = 60 / value;
        }

        public TempoChange(double bpm, ITimePoint? injectedStart = null) : base(injectedStart) => BPM = bpm;

        public TimeUnit CalculateTimeFromBeat(BeatUnit beat) => TimeUnit.FromSeconds((beat.Value - t.Beat.Value) * beatLength).OffsetBy(t.Time);
        public BeatUnit CalculateBeatFromTime(TimeUnit time) => BeatUnit.FromValue((time.Seconds - t.Time.Seconds) / beatLength).OffsetBy(t.Beat);
    }

    public static class TempoChangeExtensionMethods {
        public static void UpdateTimeFromBeat(this TimePoint t, TempoChange o) => t.TimeOfBeat = o.CalculateTimeFromBeat(t.Beat);
        public static void UpdateBeatFromTime(this TimePoint t, TempoChange o) => t.Beat = o.CalculateBeatFromTime(t.TimeOfBeat);
    }
}
