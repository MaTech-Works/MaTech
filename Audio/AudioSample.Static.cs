// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Cysharp.Threading.Tasks;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Audio {
    public partial class AudioSample {
        public static async UniTask<AudioSample> LoadFromExternalUrl(string url) {
            var sample = new AudioSample { AudioUrl = url };
            if (!await sample.Load()) return null;
            return sample;
        }

        public static async UniTask<AudioSample> LoadFromAudioClipInResources(string url) {
            var sample = new AudioSample { AudioClipUrlInResources = url };
            if (!await sample.Load()) return null;
            return sample;
        }

        public static async UniTask<AudioSample> LoadFromAudioClip(AudioClip clip) {
            if (UnityUtil.IsUnassigned(clip)) return null;
            var sample = new AudioSample { AudioClip = clip };
            if (!await sample.Load()) return null;
            return sample;
        }
    }
}