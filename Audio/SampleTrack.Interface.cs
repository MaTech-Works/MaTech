// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Audio {
    public interface ISampleTrackData {
        /// 增加为音轨在指定时间增加一个采样。当IsLoaded为true时，此方法应当当场加载采样数据，并且保持IsLoaded状态。
        /// 阻塞调用，由业务逻辑来处理异步调用。
        ISampleControl AddSample(ISampleData audio, double offset, float volume = 100);
        void RemoveSample(ISampleControl item);
        void UpdateSample(ISampleControl item);
        
        /// 音轨数据是否已经全部加载就绪并可以立即开始播放。
        /// 已加载就绪时再增加未加载的采样，应当将此属性设置为false，并在Load后再恢复回true。
        bool IsLoaded { get; }
        
        /// 加载音轨中的所有采样并为序列播放做准备。
        /// 可以重复调用，应当只加载还未加载的采样（ISampleData.IsLoaded），避免重复加载已有数据。
        /// 结束时应当返回主线程。
        bool Load();
        
        void Unload();
        void Clear();
    }
    
    public interface ISampleTrackControl {
        void Update();
        
        void Restart(double startTime = 0);
        void Resume();
        void Pause();
        void Toggle();
        void Seek(double playTime, bool safe = false);

        bool IsPlaying { get; }
        bool IsEnd { get; }
        
        double PlayTime { get; }
        double Length { get; }

        float Volume { get; set; }
        float Speed { get; set; }

        double DSPLatency { get; }

        bool Loop { get; set; }
        double LoopStartTime { get; set; }
        
        void FadeIn(int duration, int delay = 0);
        void FadeOut(int duration, int delay = 0);
        void ClearFade();
    }
}