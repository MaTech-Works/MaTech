// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using MaTech.Common.Algorithm;
using Debug = UnityEngine.Debug;

namespace MaTech.Audio {
    public class SampleTrack {
        private readonly List<AudioSample> samples = new List<AudioSample>();

        public IEnumerable<AudioSample> Samples {
            get => samples;
            set {
                if (value == null) return;
                IsLoaded = false;
                samples.Clear();
                samples.AddRange(value.Where(s => s != null));
            }
        }

        public bool IsLoaded { get; private set; }
        
        /// <summary>
        /// 加载音轨中的所有采样并且排序。完成后，使用AddSample与RemoveSample时会当场加载增加的采样。
        /// </summary>
        public async UniTask<bool> Load() {
            if (IsLoaded) {
                Debug.Log($"[SampleTrack] Trying to load a already loaded track.");
                return true;
            }

            if (samples == null) {
                Debug.LogError("[SampleTrack] Sample list is null, except it shouldn't be.");
                return false;
            }

            if (samples.Count == 0) {
                Debug.Log($"[SampleTrack] Track loaded with no samples at all.");
                IsLoaded = true;
                return true;
            }
            
            await UniTask.SwitchToThreadPool();

            Stopwatch sw = Stopwatch.StartNew();
            
            samples.Sort();
            await UniTask.WhenAll(Enumerable.Select(samples, sample => sample.Load()));
            
            Debug.Log($"[SampleTrack] Track loaded in {sw.Elapsed.TotalSeconds}s");

            IsLoaded = true;
            return true;
        }
        
        /// <summary>
        /// 释放音轨中的所有采样音频，并将IsLoaded设为false。
        /// </summary>
        public async UniTask Unload() {
            IsLoaded = false;
            await UniTask.SwitchToThreadPool();
            foreach (var sample in samples) {
                sample.Unload();
            }
        }
        
        /// <summary>
        /// 为音轨增加一个采样，若采样尚未加载，则会将IsLoaded设为false。
        /// </summary>
        public void AddSample(AudioSample sample) {
            if (sample == null) return;
            if (IsLoaded && sample.IsLoaded) {
                samples.OrderedInsert(sample);
            } else {
                IsLoaded = false;
                samples.Add(sample);
            }
        }
        
        /// <summary>
        /// 为音轨增加并立即加载一个采样，不会改变IsLoaded状态。
        /// </summary>
        public async UniTask LoadSample(AudioSample sample, bool loadOnlyForLoadedTrack = false) {
            if (sample == null) return;
            if (IsLoaded || !loadOnlyForLoadedTrack) {
                await sample.Load();
            }
            samples.OrderedInsert(sample);
        }
        
        /// <summary>
        /// 从音轨中移除一个采样，不会改变IsLoaded状态。
        /// </summary>
        /// <param name="sample"> 被移除的采样 </param>
        /// <param name="unloadAudio"> 是否立即释放采样本身的资源；默认不释放，允许其他音轨和引用继续使用这个采样的资源，直到对象被垃圾回收（此项无论如何也不会改变IsLoaded状态） </param>
        /// <returns> 是否成功移除采样 </returns>
        public bool RemoveSample(AudioSample sample, bool unloadAudio = false) {
            if (sample == null) return false;
            if (samples.Remove(sample)) {
                if (unloadAudio) sample.Unload();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 从音轨中移除所有的采样，不会改变IsLoaded状态。
        /// </summary>
        /// <param name="unloadAudio"> 是否立即释放采样本身的资源；默认不释放，允许其他音轨和引用继续使用这个采样的资源，直到对象被垃圾回收（此项无论如何也不会改变IsLoaded状态） </param>
        public async UniTask Clear(bool unloadAudio = false) {
            if (unloadAudio) {
                await UniTask.SwitchToThreadPool();
                foreach (var sample in samples) {
                    sample.Unload();
                }
            }
            samples.Clear();
        }

        public bool Contains(AudioSample sample) => samples.Contains(sample);

    }
}