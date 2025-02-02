// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace MaTech.Audio {
    public class AudioPlayer : MonoBehaviour {
        [SerializeField]
        private AudioClip audioClip;

        [FormerlySerializedAs("volume")]
        public float volumeOnPlay = 1;
        
        [Space]
        public ushort channelIndex = 0;
        public bool autoChannel = true;

        [Space]
        public bool mute;
        public bool loop;
        
        [Space]
        public bool playOnAwake;

        private IntPtr audioData;

        private async Task LoadAudio() {
            audioData = await MaAudio.CreateAudioFromClipAsync(audioClip);
            Debug.Log("In total " + MaAudio.TestAudio(audioData) + " samples are loaded into our audio clip");
        }

        public void Play() {
            MaAudio.Channel channelNew = MaAudio.Play(audioData, volumeOnPlay, MaAudio.Mixer.Instant, autoChannel ? MaAudio.ChannelAutoAssign : channelIndex);
            if (channelNew.IsValid) channelIndex = channelNew.index;
        }
    
        public async void Awake() {
            await LoadAudio();
            if (playOnAwake) {
                Play();
            }
        }
    }
}