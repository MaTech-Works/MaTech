// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaTech.Gameplay {
    public partial class ChartPlayer {
        public interface IUnit { }
        public interface IJudgeUnit : IUnit { }
        public interface IVisualUnit : IUnit { }
        
        public partial class NoteCarrier {
            public void CreateUnits(params IUnit[] paramsUnits) {
                if (units != null && units.Length > 0) {
                    Debug.LogWarning(
                        "[NoteCarrier] Created units for NoteCarrier that already contains units, that are of types: " +
                        string.Join(",", units.Select(unit => unit.GetType().Name)) +
                        ". Please set units to null before creating new ones to suppress this warning."
                    );
                }
                units = paramsUnits;
            }
            
            public TUnit UnitOf<TUnit>() where TUnit : IUnit {
                if (units == null) return default;
                foreach (var unit in units) {
                    if (unit is TUnit t) return t;
                }
                return default;
            }
            
            public UnitsOfType<TUnit> UnitsOf<TUnit>() where TUnit : IUnit {
                return new UnitsOfType<TUnit>(this);
            }

            public bool ContainsUnit(IUnit unit) {
                return units?.Contains(unit) ?? false;
            }
        }

        public readonly struct UnitsOfType<TUnit> : IEnumerable<TUnit> where TUnit : IUnit {
            private readonly NoteCarrier target;
            public UnitsOfType(NoteCarrier target) { this.target = target; }
            
            public Enumerator GetEnumerator() => new Enumerator(target);
            IEnumerator<TUnit> IEnumerable<TUnit>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<TUnit> {
                private readonly IUnit[] arr;
                private int index;
            
                public Enumerator(NoteCarrier target) {
                    arr = target.units;
                    index = -1;
                }
            
                public void Reset() => index = -1;
                public bool MoveNext() {
                    if (arr == null) return false;
                    do index += 1;
                    while (index < arr.Length && arr[index] is not TUnit);
                    return index < arr.Length;
                }
            
                public TUnit Current => (TUnit)arr![index];
                object IEnumerator.Current => Current;
            
                public void Dispose() { }
            }
        }
    }
}