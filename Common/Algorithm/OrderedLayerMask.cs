// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Algorithm {
    public class OrderedLayerMask {
        public const int maxLayers = 32;
        public const int invalidIndex = -1;

        public readonly int[] listLayer;
        public readonly int[] mapLayerToIndex;
        public readonly int[] listMaskBefore;
        public readonly int[] listMaskAfter;
        public readonly int maskTotal;

        public int LayerCount => listLayer.Length;
        public static implicit operator int(OrderedLayerMask mask) => mask.maskTotal;

        public bool Contains(int layer) {
            if (layer < 0 || layer >= maxLayers) return false;
            return (maskTotal & (1 << layer)) != 0;
        }

        public OrderedLayerMask(params int[] layers) {
            listLayer = layers;
            mapLayerToIndex = new int[maxLayers];
            listMaskBefore = new int[layers.Length];
            listMaskAfter = new int[layers.Length];
            maskTotal = 0;

            for (int i = 0; i < maxLayers; ++i)
                mapLayerToIndex[i] = invalidIndex;

            for (int i = 0; i < layers.Length; ++i) {
                int index = layers[i];
                if (index < 0 || index >= maxLayers) continue;
                mapLayerToIndex[index] = i;
                maskTotal |= (1 << index);
            }

            for (int mask = 0, i = 0; i < layers.Length; ++i) {
                int index = layers[i];
                if (index < 0 || index >= maxLayers) continue;
                listMaskBefore[i] = mask;
                mask |= (1 << index);
            }

            for (int mask = 0, i = layers.Length - 1; i >= 0; --i) {
                int index = layers[i];
                if (index < 0 || index >= maxLayers) continue;
                listMaskAfter[i] = mask;
                mask |= (1 << index);
            }

        }
    }
}