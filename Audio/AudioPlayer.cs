// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MaTech.Audio {
    public class AudioPlayer : MonoBehaviour {

        [SerializeField]
        private AudioClip audioClip;

        public bool mute;
        public bool playOnAwake;
        public bool loop;

        public float volume = 1;

        private IntPtr audioData;

        private async Task LoadAudio() {
            audioData = await MaAudio.CreateAudioFromClipAsync(audioClip);
            Debug.Log("In total " + MaAudio.TestAudio(audioData) + " samples are loaded into our audio clip");
        }

        public void Play() {
            MaAudio.Play(audioData, volume, MaAudio.Mixer.Instant, MaAudio.ChannelAutoAssign);
        }
    
        public async void Start() {
            await LoadAudio();
            if (playOnAwake) {
                Play();
            }
        }
    
    }
}