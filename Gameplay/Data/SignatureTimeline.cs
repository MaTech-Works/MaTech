// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using UnityEngine;

namespace MaTech.Gameplay.Data {
    /// <summary>
    /// 将小节映射至节拍的数值映射，用于辅助解析基于小节的谱面格式（如BMS、SM和TJA）。
    /// 不支持负数的bar序号。
    /// </summary>
    [Serializable]
    public class SignatureTimeline : ISerializationCallbackReceiver {
        [Serializable]
        public struct SignatureChange : IComparable<SignatureChange> {
            public int barAtChange;
            public Fraction beatAtChange;
            public FractionSimple beatsPerBar;
            public FractionSimple quarterNotesPerBeat;
            public Fraction BeatAt(int bar) => beatAtChange + (Fraction)beatsPerBar * (bar - barAtChange);
            public Fraction BeatAt(Fraction barPlusSubPosition) => beatAtChange + (Fraction)beatsPerBar * (barPlusSubPosition - barAtChange);
            public int CompareTo(SignatureChange other) => barAtChange.CompareTo(other.barAtChange);
        }

        [SerializeField] private List<SignatureChange> signatures = new List<SignatureChange>();

        public IEnumerable<SignatureChange> SignatureChanges => signatures;

        public SignatureTimeline() : this(new FractionSimple(4)) { }
        public SignatureTimeline(FractionSimple defaultBeatPerBar, FractionSimple? defaultQuarterNotesPerBeat = null) {
            signatures.Add(new SignatureChange {
                barAtChange = 0,
                beatAtChange = Fraction.zero,
                beatsPerBar = defaultBeatPerBar,
                quarterNotesPerBeat = defaultQuarterNotesPerBeat ?? new FractionSimple(1),
            });
        }

        public SignatureChange? SetSignatureChangeAt(int bar, FractionSimple beatsPerBar, FractionSimple? quarterNotesPerBeat = null) {
            if (bar < 0) return null;
            
            int indexInserted = signatures.OrderedInsert(new SignatureChange {
                barAtChange = bar,
                beatsPerBar = beatsPerBar,
                quarterNotesPerBeat = quarterNotesPerBeat ?? new FractionSimple(1),
            }, CollectionsExtension.ResolveEqual.ReplaceFirstEqual);

            var referenceSignature = signatures[indexInserted == 0 ? 0 : indexInserted - 1];
            if (indexInserted == 0)
                referenceSignature.beatAtChange = Fraction.zero;
            
            for (int i = indexInserted; i < signatures.Count; ++i) {
                var signature = signatures[i];
                signature.beatAtChange = referenceSignature.BeatAt(signature.barAtChange);
                signatures[i] = referenceSignature = signature;
            }

            return signatures[indexInserted];
        }

        public SignatureChange? GetSignatureAt(int bar) {
            int index = signatures.IndexOfLastMatchedValueBinarySearch(bar, (signature, bar) => signature.barAtChange <= bar);
            return index == -1 ? null : signatures[index];
        }
        
        public SignatureChange? GetNextSignature(int bar) {
            int index = signatures.IndexOfFirstMatchedValueBinarySearch(bar, (signature, bar) => signature.barAtChange > bar);
            return index == -1 ? null : signatures[index];
        }

        public SignatureChange? GetPreviousSignature(int bar) {
            int index = signatures.IndexOfLastMatchedValueBinarySearch(bar, (signature, bar) => signature.barAtChange <= bar);
            return index > 0 ? null : signatures[index - 1];
        }

        public Fraction? BeatAt(int bar) => GetSignatureAt(bar)?.BeatAt(bar);
        public Fraction? BeatAt(Fraction barPlusSubPosition) => GetSignatureAt(barPlusSubPosition.Integer)?.BeatAt(barPlusSubPosition);
        public Fraction UnclampedBeatAt(int bar) => GetSignatureAt(Math.Max(0, bar))!.Value.BeatAt(bar);
        public Fraction UnclampedBeatAt(Fraction barPlusSubPosition) => GetSignatureAt(Math.Max(0, barPlusSubPosition.Integer))!.Value.BeatAt(barPlusSubPosition);

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() {
            if (signatures.Count == 0) {
                signatures.Add(new SignatureChange {
                    barAtChange = 0,
                    beatsPerBar = new FractionSimple(4),
                    quarterNotesPerBeat = new FractionSimple(1),
                });
            } else {
                ShellSort.Hibbard(signatures);
            }
        }
    }
}