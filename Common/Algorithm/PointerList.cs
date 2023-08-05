// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个内置了index的List。提供一些方便的取元素功能。
    /// </summary>
    public class PointerList<T> : List<T> {
        public PointerList() : base() { }
        public PointerList(IEnumerable<T> collection) : base(collection) { }
        public PointerList(int capacity) : base(capacity) { }

        public int IndexPointer { get; set; }

        public int RemainCountForward => Count - IndexPointer - 1;
        public int RemainCountBackward => IndexPointer;

        public void SetPointerToFirst(int offset = 0) => IndexPointer = offset;
        public void SetPointerToLast(int offset = 0) => IndexPointer = Count - 1 + offset;

        private bool CheckInRange(int index) => index >= 0 && index < Count;

        public bool IsPointerValid => CheckInRange(IndexPointer);
        public bool IsNextValid => CheckInRange(IndexPointer + 1);
        public bool IsLastValid => CheckInRange(IndexPointer - 1);
        public bool HasNext => CheckInRange(IndexPointer + 1);
        public bool HasLast => CheckInRange(IndexPointer - 1);

        public T Get() => base[IndexPointer];
        public T MoveNext() => base[++IndexPointer];
        public T MoveLast() => base[--IndexPointer];
        public T PeekNext() => base[IndexPointer + 1];
        public T PeekLast() => base[IndexPointer - 1];
        public void SkipToNext() => ++IndexPointer;
        public void SkipToLast() => --IndexPointer;

        private bool TryDo(bool condition, Action action) {
            if (condition) action();
            return condition;
        }

        private bool TryGet(out T result, bool condition, Func<T> getter) {
            result = condition ? getter() : default;
            return condition;
        }

        public bool TryGet(out T result) => TryGet(out result, IsPointerValid, Get);
        public bool TryMoveNext(out T result) => TryGet(out result, IsNextValid, MoveNext);
        public bool TryMoveLast(out T result) => TryGet(out result, IsLastValid, MoveLast);
        public bool TryPeekNext(out T result) => TryGet(out result, IsNextValid, PeekNext);
        public bool TryPeekLast(out T result) => TryGet(out result, IsLastValid, PeekLast);
        public bool TrySkipToNext() => TryDo(IsNextValid, SkipToNext);
        public bool TrySkipToLast() => TryDo(IsLastValid, SkipToLast);

        public T GetOrDefault(T defaultValue = default) => IsPointerValid ? Get() : defaultValue;
        public T MoveNextOrDefault(T defaultValue = default) => IsNextValid ? MoveNext() : defaultValue;
        public T MoveLastOrDefault(T defaultValue = default) => IsLastValid ? MoveLast() : defaultValue;
        public T PeekNextOrDefault(T defaultValue = default) => IsNextValid ? PeekNext() : defaultValue;
        public T PeekLastOrDefault(T defaultValue = default) => IsLastValid ? PeekLast() : defaultValue;
    }
}