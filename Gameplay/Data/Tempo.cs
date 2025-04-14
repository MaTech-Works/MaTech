// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

namespace MaTech.Gameplay.Data {
    public class TempoChange : TimedObject {
        private readonly ITimePoint start;
        
        public readonly double beatSeconds;
        public readonly double beatsPerSecond;
        
        public double BPM => beatsPerSecond * 60;

        public TempoChange(ITimePoint start, double bpm) {
            this.start = start;
            beatSeconds = 60 / bpm;
            beatsPerSecond = bpm / 60;
        }

        public override ITimePoint Start => start;
        public override ITimePoint? End => null;

        public TimeValue CalculateTimeFromBeat(BeatValue beat) => TimeValue.FromSeconds(beat.DeltaSince(start.Beat).Value * beatSeconds).OffsetBy(start.Time);
        public BeatValue CalculateBeatFromTime(TimeValue time) => BeatValue.FromValue(time.DeltaSince(start.Time).Seconds * beatsPerSecond).OffsetBy(start.Beat);
    }

    public static class TempoChangeExtensionMethods {
        public static void UpdateTimeFromBeat(this TimePoint t, TempoChange o) => t.TimeOfBeat = o.CalculateTimeFromBeat(t.Beat);
        public static void UpdateBeatFromTime(this TimePoint t, TempoChange o) => t.Beat = o.CalculateBeatFromTime(t.TimeOfBeat);
    }
}
