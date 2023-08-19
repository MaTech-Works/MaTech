// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Cysharp.Threading.Tasks;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Audio {
    public partial class AudioSample : ISampleData {
        public enum UrlType {
            None, External, AudioClipInResource
        }
        
        private string url;
        private AudioClip clip;
        private IntPtr audio = MaAudio.NullAudio;

        private UrlType urlType = UrlType.None;

        private float volume = 1;
        private MaAudio.Channel channel = MaAudio.ChannelAutoAssign;
        private MaAudio.Channel channelActive = MaAudio.InvalidChannel;

        private double length = double.NaN; // set when loaded

        public string AudioUrl {
            get => url;
            set {
                ResetAudio();
                url = value;
                urlType = UrlType.External;
            }
        }
        
        public string AudioClipUrlInResources {
            get => url;
            set {
                ResetAudio();
                url = value;
                urlType = UrlType.AudioClipInResource;
            }
        }

        public AudioClip AudioClip {
            get => clip;
            set {
                ResetAudio();
                clip = value;
            }
        }
        
        public IntPtr Audio {
            get => audio;
            set {
                ResetAudio();
                audio = value;
            }
        }

        public bool IsValid => url != null || clip != null || audio != MaAudio.NullAudio;
        public bool IsLoaded => audio != MaAudio.NullAudio;

        public double Length => length;

        private void ResetAudio() {
            url = null;
            urlType = UrlType.None;
            clip = null;
            Unload();
        }

        public async UniTask<bool> Load() {
            if (audio != MaAudio.NullAudio) return true;

            if (urlType == UrlType.AudioClipInResource) {
                clip = (AudioClip) await Resources.LoadAsync<AudioClip>(url);
            }

            if (UnityUtil.IsUnassigned(clip)) {
                if (url == null) return false;
                clip = await AudioPool.SingletonInstance.LoadAudioClip(url, false);
                await UniTask.SwitchToMainThread();
                if (clip == null || clip.loadState == AudioDataLoadState.Failed) return false;
            }
            
            await UniTask.SwitchToMainThread();
            length = clip.length;
            audio = await MaAudio.CreateAudioFromClipAsync(clip);

            if (audio == MaAudio.NullAudio) {
                Debug.LogError($"[Sample] Audio {Path.GetFileName(url)} cannot be loaded.");
                return false;
            }
            return true;
        }

        public void Unload() {
            if (audio != MaAudio.NullAudio) {
                MaAudio.ReleaseAudio(audio);
                audio = MaAudio.NullAudio;
            }
        }

        public AudioSample MakeClone() {
            return new AudioSample() {
                url = url,
                clip = clip,
                audio = audio,
                urlType = urlType,
                volume = volume,
                channel = channel,
                length = length,
            };
        }
        
        ~AudioSample() => Unload();
        public void Dispose() => Unload();
    }
}