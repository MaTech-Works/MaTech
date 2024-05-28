// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using MaTech.Gameplay.Time;

namespace MaTech.Gameplay.Data {
    public class TempoChange : TimedObject {
        private readonly ITimePoint start;
        
        public double beatLength = 0.5; // seconds per beat

        // ReSharper disable once InconsistentNaming
        public double BPM {
            get => 60 / beatLength;
            set => beatLength = 60 / value;
        }

        public TempoChange(double bpm, ITimePoint? injectedStart = null) {
            start = injectedStart ?? new TimePoint();
            BPM = bpm;
        }

        public override ITimePoint Start => start;
        public override ITimePoint? End => null;
        
        public TimePoint? MutableStart => start as TimePoint;

        public TimeUnit CalculateTimeFromBeat(BeatUnit beat) => TimeUnit.FromSeconds((beat.Value - start.Beat.Value) * beatLength).OffsetBy(start.Time);
        public BeatUnit CalculateBeatFromTime(TimeUnit time) => BeatUnit.FromValue((time.Seconds - start.Time.Seconds) / beatLength).OffsetBy(start.Beat);
    }

    public static class TempoChangeExtensionMethods {
        public static void UpdateTimeFromBeat(this TimePoint t, TempoChange o) => t.TimeOfBeat = o.CalculateTimeFromBeat(t.Beat);
        public static void UpdateBeatFromTime(this TimePoint t, TempoChange o) => t.Beat = o.CalculateBeatFromTime(t.TimeOfBeat);
    }
}
