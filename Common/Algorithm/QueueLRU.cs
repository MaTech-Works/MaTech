// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace MaTech.Common.Algorithm {
    public class QueueLRU<T> {
        private class Node {
            public T val;
            public Node next;
            public Node prev;
        }

        private readonly ObjectPool<Node> pool;
        private readonly Dictionary<T, Node> nodes;

        private readonly Node headNode = new Node(); // Least recently used
        private readonly Node tailNode = new Node(); // Most recently used

        public QueueLRU(int initCapacity = 0) {
            pool = new ObjectPool<Node>(() => new Node(), initCapacity);
            nodes = new Dictionary<T, Node>(initCapacity);
            headNode.next = tailNode;
            tailNode.prev = headNode;
        }

        public int Count => nodes.Count;
        public bool Contains(T val) => nodes.ContainsKey(val);

        public void Clear() {
            nodes.Clear();
            while (headNode.next != tailNode) {
                var node = headNode.next;
                RemoveNode(node);
                pool.Recycle(node);
            }
        }

        public void Enqueue(T val) {
            if (nodes.TryGetValue(val, out var existNode)) {
                RefreshNode(existNode);
                return;
            }
            Node node = pool.Get();
            node.val = val;
            QueueNode(node);
            nodes.Add(val, node);
        }

        public T Dequeue() {
            var node = headNode.next;
            if (node == tailNode) throw new InvalidOperationException("The LRU queue is empty when dequeued.");
            RemoveNode(node);
            var result = node.val;
            nodes.Remove(result);
            pool.Recycle(node);
            return result;
        }

        public T Peek() {
            var node = headNode.next;
            if (node == tailNode) throw new InvalidOperationException("The LRU queue is empty when dequeued.");
            return node.val;
        }

        private void RefreshNode(Node node) {
            if (node.next == tailNode) return;
            RemoveNode(node);
            QueueNode(node);
        }

        private void RemoveNode(Node node) {
            node.prev.next = node.next;
            node.next.prev = node.prev;
        }

        private void QueueNode(Node node) {
            node.prev = tailNode.prev;
            node.next = tailNode;
            tailNode.prev = node;
        }
    }
}