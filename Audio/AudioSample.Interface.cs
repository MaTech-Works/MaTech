// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using Cysharp.Threading.Tasks;

namespace MaTech.Audio {
    public interface ISampleData : IDisposable {
        string AudioUrl { get; set; }
        
        bool IsValid { get; }
        bool IsLoaded { get; }
        
        double Length { get; }

        UniTask<bool> Load();
        void Unload();
    }
    
    public interface ISampleControl {
        /// 音频实际开始播放前的延时，指定负值会使得PlayScheduled提早开始播放。
        double Offset { get; set; }
        /// 音频结束时对应的延时，即(Offset+Length)
        double OffsetEnd { get; }
        /// 当前的播放进度，以秒计。
        double PlayTime { get; }

        ushort Channel { get; set; }
        float Volume { get; set; }
        
        bool IsPlaying { get; }
        bool IsFinished { get; }

        /// 无视Offset数值，立即开始播放采样。播放时间以进入混音器的时刻为准（即尽快播放）。
        void PlayImmediate();
        /// 使用Offset数值+额外delay时间延时（或者提早从音乐中途）开始播放。播放时间以进入混音器的时刻为准，即提供负值offset会在准确的音源位置听到播放内容。
        void PlayDelayed(double delayTime, bool withOffset = true);
        /// 在指定的DspTime时刻延时Offset后开始播放。播放时间准确对齐DSPTime，但是指定当前DSPTime作为参数可能因为延迟丢失若干采样（依后台音频库实现而定）。
        void PlayScheduled(double scheduledDspTime, bool withOffset = true);

        void Pause();
        void Stop();
        void Toggle();
        void Resume();

        void Seek(double playTime);
    }
}