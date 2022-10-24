// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    /// <summary>
    /// 一个基于小根二叉堆的优先队列实现，要求模板参数T实现IComparable&lt;T&gt;
    /// </summary>
    public class PriorityQueue<T> where T : IComparable<T> {
        private List<T> heap;

        public PriorityQueue() {
            heap = new List<T>(1);
            heap.Add(default(T));
        }

        public PriorityQueue(IEnumerable<T> collection) : this() {
            heap.AddRange(collection);
            for (int i = 1, n = heap.Count; i < n; ++i) {
                Float(i);
            }
        }

        public PriorityQueue(int capacity) {
            heap = new List<T>(capacity + 1);
        }

        public int Count {
            get {
                return heap.Count - 1;
            }
        }

        public int Capacity {
            get {
                return heap.Capacity - 1;
            }
            set {
                heap.Capacity = value + 1;
            }
        }

        public bool HasNext {
            get {
                return heap.Count > 1;
            }
        }

        public void Clear() {
            heap.Clear();
            heap.Add(default(T));
        }

        public T Pop() {
            int indexLast = heap.Count - 1;
            T result = heap[1];
            if (indexLast != 1) heap[1] = heap[indexLast];
            heap.RemoveAt(indexLast);
            if (indexLast != 1) Sink(1);
            return result;
        }

        public void Push(T item) {
            int indexLast = heap.Count;
            heap.Add(item);
            Float(indexLast);
        }

        public T Peek() {
            return heap[1];
        }

        public void Discard() {
            int indexLast = heap.Count - 1;
            heap[1] = heap[indexLast];
            heap.RemoveAt(indexLast);
            if (indexLast != 1) Sink(1);
        }

        /// <summary>
        /// 堆的上浮操作，在元素未到堆顶前，每次检查父亲元素是否比自己更大，并将自己上浮（与父亲交换）然后从上浮后的位置继续检查。
        /// </summary>
        private void Float(int i) {
            int j;
            T t = heap[i];
            while (i > 1) { // while i is not the top yet
                j = i >> 1; // who is i's parent?
                if (heap[j].CompareTo(t) <= 0) break; // cannot float anymore, get out of here
                heap[i] = heap[j]; // sink the parent
                i = j; // float i
            }
            heap[i] = t; // place i at the final position
        }

        /// <summary>
        /// 堆的下沉操作，在元素未到堆底前，每次找到较小的孩子元素并且检查是否比自己更小，并将自己下沉（与较小的孩子交换）然后从下沉后的位置继续检查。
        /// </summary>
        private void Sink(int i) {
            int j, n = heap.Count;
            T t = heap[i];
            while ((j = i << 1) < n) { // while i has the left child
                if (j + 1 < n && heap[j].CompareTo(heap[j + 1]) > 0) j = j + 1; // targets the right child if it exists and smaller than the left
                if (heap[j].CompareTo(t) >= 0) break; // cannot sink anymore, get out of here
                heap[i] = heap[j]; // float the child
                i = j; // sink i
            }
            heap[i] = t; // place i at the final position
        }
    }
}