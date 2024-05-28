// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MaTech.Common.Algorithm;
using MaTech.Common.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MaTech.Audio {
    public class AudioPool : Singleton<AudioPool> {
        private const long defaultReduceSize = 1024 * 1024 * 120; // 120M samples, 4 bytes per sample

        private readonly Dictionary<string, AudioClip> globalAudio = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AudioClip> chartAudio = new Dictionary<string, AudioClip>();

        private readonly QueueLRU<string> queueChartAudio = new QueueLRU<string>();
        
        private readonly AsyncLock lockLoading = new AsyncLock();
        
        private long currentCacheSize;

        public async UniTask<AudioClip> LoadAudioClip(string url, bool isGlobal) {
            using (await lockLoading.LockAsync()) {
                var listAudio = isGlobal ? globalAudio : chartAudio;
                if (listAudio.TryGetValue(url, out var existClip)) {
                    if (!isGlobal && UnityUtil.IsAssigned(existClip)) {
                        queueChartAudio.Enqueue(url);
                    }
                    return existClip;
                }
                
                var clip = await RequestAudioClip(url);
                
                #if UNITY_EDITOR
                Debug.Log($"Loaded AudioClip {Path.GetFileName(url)}");
                #endif
                
                listAudio.Add(url, clip);
                if (!isGlobal && clip != null) {
                    queueChartAudio.Enqueue(url);
                    currentCacheSize += clip.samples * clip.channels;
                }
                
                return clip;
            }
        }

        public void ReduceCache(long sizeLimit = defaultReduceSize) {
            while (queueChartAudio.Count != 0 && currentCacheSize > sizeLimit) {
                var url = queueChartAudio.Dequeue();
                var clip = chartAudio[url];
                currentCacheSize -= clip.samples * clip.channels;
                chartAudio.Remove(url);
            }
        }

        private async UniTask<AudioClip> RequestAudioClip(string url) {
            if (url == null) return null;
            
            AudioType audioType = GetAudioTypeByPath(url);
            #if UNITY_STANDALONE
            if (audioType == AudioType.MPEG) {
                throw new System.NotImplementedException("MP3 decoders not present in builds. Implement a MP3 decoder here and output the decoded data as an AudioClip.");
            }
            #endif

            await UniTask.SwitchToMainThread();
            
            #if UNITY_EDITOR
            if (url.StartsWith("Assets") && !url.Contains("StreamingAssets")) return AssetDatabase.LoadAssetAtPath<AudioClip>(url);
            #endif
            
            using var request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            await request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"Cannot load audio file \"{url}\"");
                return null;
            }
            
            return DownloadHandlerAudioClip.GetContent(request);
        }
        
        private static AudioType GetAudioTypeByPath(string url) {
            switch (Path.GetExtension(url).ToLower()) {
            case ".ogg": return AudioType.OGGVORBIS;
            case ".wav": return AudioType.WAV;
            case ".mp3": return AudioType.MPEG;
            default: return AudioType.UNKNOWN;
            }
        }
    }
}