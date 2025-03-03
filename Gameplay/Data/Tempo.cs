// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

namespace MaTech.Gameplay.Data {
    public class TempoChange : TimedObject {
        private readonly ITimePoint start;
        
        public readonly TimeUnit timePerBeat = TimeUnit.FromSeconds(0.5);
        public double BPM => 60000 / timePerBeat.Milliseconds;

        public TempoChange(double bpm, ITimePoint? injectedStart = null) {
            start = injectedStart ?? new TimePoint();
            timePerBeat =  TimeUnit.FromSeconds(60 / bpm);
        }

        public override ITimePoint Start => start;
        public override ITimePoint? End => null;
        
        public TimePoint? MutableStart => start as TimePoint;

        public TimeUnit CalculateTimeFromBeat(BeatUnit beat) => TimeUnit.FromSeconds(beat.DeltaSince(start.Beat) * timePerBeat.Seconds).OffsetBy(start.Time);
        public BeatUnit CalculateBeatFromTime(TimeUnit time) => BeatUnit.FromValue(time.DeltaSince(start.Time) / timePerBeat.Seconds).OffsetBy(start.Beat);
    }

    public static class TempoChangeExtensionMethods {
        public static void UpdateTimeFromBeat(this TimePoint t, TempoChange o) => t.TimeOfBeat = o.CalculateTimeFromBeat(t.Beat);
        public static void UpdateBeatFromTime(this TimePoint t, TempoChange o) => t.Beat = o.CalculateBeatFromTime(t.TimeOfBeat);
    }
}
