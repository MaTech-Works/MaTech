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
    
        #if UNITY_EDITOR
        private static bool warnedAboutMP3DecoderInEditor = false;
        #endif

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
                Debug.Log($"[MaAudio] Loaded AudioClip from file {Path.GetFileName(url)}");
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
            if (url is null) return null;
            
            AudioType audioType = GetAudioTypeByPath(url);

            await UniTask.SwitchToMainThread();
            
            #if UNITY_EDITOR
            if (url.StartsWith("Assets") && !url.Contains("StreamingAssets")) {
                return AssetDatabase.LoadAssetAtPath<AudioClip>(url);
            }
            #endif
            
            using var request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            await request.SendWebRequest();
            
            if (request.result is not UnityWebRequest.Result.Success) {
                Debug.LogError($"[MaAudio] Cannot load audio file \"{url}\", audio type {audioType}");
                #if !UNITY_EDITOR && !UNITY_2022_OR_NEWER
                if (audioType == AudioType.MPEG) {
                    Debug.LogError("In non-editor builds, MP3 decoder might not exist or having poor support in older version of Unity due to licensing issues.");
                }
                #endif
                return null;
            }
            
            #if UNITY_EDITOR
            if (audioType is AudioType.MPEG && !warnedAboutMP3DecoderInEditor) {
                Debug.LogWarning("In non-editor builds, MP3 decoder might not exist or having poor support in older version of Unity due to licensing issues. " +
                                 "In Extra, different decoder implementations may set different lengths of blank before audio starts, which might create audio-syncing issues across platforms. " +
                                 "Please avoid using MP3 audio sources in builds.");
                warnedAboutMP3DecoderInEditor = true;
            }
            #endif
            
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