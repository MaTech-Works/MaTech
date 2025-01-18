// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MaTech.Audio {
    public static partial class MaAudio {
        private const string DllNameDefault = "MaAudio_Core";
        private const string DllNameInternal = "__Internal";
        
        #if UNITY_IOS
        private const string DllName = DllNameInternal;
        #elif UNITY_ANDROID || UNITY_WSA || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
        private const string DllName = DllNameDefault;
        #else
        #warning "MaAudio does not support this build platform. Exception will be thrown on calling native APIs since the native binary is not available."
        #endif

        public static IntPtr NullAudio => IntPtr.Zero;

        public struct Channel {
            public ushort index;
            public bool IsValid => index != ushort.MaxValue;
            public bool IsInvalid => index == ushort.MaxValue;
            public bool IsManualAssignOnly => (index & (1 << 15)) == 0;  // 小于 1 << 15 的 index 仅用于手动指定，不会被自动分配
                
            public static implicit operator Channel(int i) => new Channel { index = (ushort)i }; // 参数可以直接填int类型
        }
        
        public static Channel InvalidChannel => ushort.MaxValue;
        public static Channel ChannelAutoAssign => InvalidChannel; // 自动指定channel，需要保存Play返回的实际channel

        public enum Mixer : ushort {
            Buffered = 0, // 缓冲混音，延迟稍高，对性能要求不高，用于音轨等提前预定播放的内容
            Instant = 1,  // 快速混音，延迟低，对性能要求高，用于实时反馈的打击音效
        }

        #region Audio System

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_Create")]
        public static extern bool Create(int sampleRate); // note: call MaAudio.LoadForUnity to register extra handling for Unity

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_Destroy")]
        public static extern bool Destroy(); // note: call MaAudio.UnloadForUnity to register extra handling for Unity

        public static extern bool IsValid {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_IsValid")] get;
        }

        public static extern bool Paused {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_Pause")] set;
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_IsPaused")] get;
        }

        #endregion

        #region Audio Data
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_CreateAudio")]
        public static extern IntPtr CreateAudio();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_CreateAudioWithData")]
        public static extern IntPtr CreateAudioWithData(float[] samples, int sampleCount, int sampleRate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_ReleaseAudio")]
        public static extern bool ReleaseAudio(IntPtr audio);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_ReleaseAllAudio")]
        public static extern bool ReleaseAllAudio();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_LoadAudio")]
        public static extern bool LoadAudio(IntPtr audio, float[] samples, int sampleCount, int sampleRate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_TestAudio")]
        public static extern int TestAudio(IntPtr audio);
        
        #endregion

        #region Audio Playback

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_Play")]
        public static extern Channel Play(IntPtr audio, float volume, Mixer mixer, Channel channel);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_PlayScheduled")]
        public static extern Channel PlayScheduled(IntPtr audio, double scheduledDspTime, float volume, Mixer mixer, Channel channel);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_PlayDelayed")]
        public static extern Channel PlayDelayed(IntPtr audio, double delay, float volume, Mixer mixer, Channel channel);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_Stop")]
        public static extern Channel Stop(Mixer mixer, Channel channel);

        // todo: native侧export还没有完成
        /*
        [DllImport(dllName, EntryPoint = "MaAudio_SetPosition")]
        public static extern void SetPosition(Mixer mixer, Channel channel, double timeFromStart);
        [DllImport(dllName, EntryPoint = "MaAudio_GetPosition")]
        public static extern double GetPosition(Mixer mixer, Channel channel);
        */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_SetVolume")]
        public static extern bool SetVolume(Mixer mixer, Channel channel, float volume);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_GetVolume")]
        public static extern float GetVolume(Mixer mixer, Channel channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_SetMixerVolume")]
        public static extern bool SetMixerVolume(Mixer mixer, float volume);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_GetMixerVolume")]
        public static extern float GetMixerVolume(Mixer mixer);
        
        #endregion
        
        #region Audio System Properties
        
        // todo: native侧export还没有完成
        /*
        [DllImport(dllName, EntryPoint = "MaAudio_GetMixerDSPTime")]
        public static extern double GetMixerDSPTime(Mixer mixer);
        [DllImport(dllName, EntryPoint = "MaAudio_GetMixerBufferSize")]
        public static extern double GetMixerBufferSize(Mixer mixer);
        [DllImport(dllName, EntryPoint = "MaAudio_GetMixerLatencyMeasured")]
        public static extern double GetMixerLatencyMeasured(Mixer mixer);
        */
        
        /// <summary> 音频进入到输出缓冲的时间，以秒计，可以用于游戏音效的准确计时，精度因平台而定。与GetMixerDSPTime(Instant)结果相同。 </summary>
        public static extern double OutputDSPTime {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_GetOutputDSPTime")] get;
        }
        
        /// <summary> 缓冲区的总大小，以秒计 </summary>
        public static extern double OutputBufferSize {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_GetOutputBufferSize")] get;
        }
        
        /// <summary> 缓冲区的最小填充大小，以秒计，这个数值可能来自音频驱动提供的数值，或者在无信息时可能取缓冲区大小的一半 </summary>
        public static extern double OutputBufferPadding {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_GetOutputBufferPadding")] get;
        }
        
        /// <summary>
        /// 混音器与驱动的输出延迟（不计算被系统混音器或者音频硬件小号后的处理时间），
        /// 通常大于但是逼近BufferPadding，大约等于BufferPadding加填充周期的一半。
        /// 当ActiveOutput开启时，本数值可能为自动测量的数值，可能在计算机切换硬件时发生变化。
        /// </summary>
        public static extern double OutputLatency {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_GetOutputLatencyMeasured")] get;
        }

        /// <summary>
        /// 音频库是否以理想的低延迟方式，以最快的速度输出音频。
        /// Windows：混音线程与WaveRT驱动配合，用spinlock保持wait-free状态，并且尝试通过IAudioClient3降低系统缓冲区大小。典型缓冲大小为10-20ms。
        /// Android：由AAudio向设备要求LowLatency模式，低版本系统不一定支持。典型缓冲大小为4ms。
        /// iOS/macOS：向CoreAudio设备要求setPreferredIOBufferDuration至0.005s（苹果文档注明的典型值），并且在macOS上要求降低设备缓冲区大小kAudioDevicePropertyBufferFrameSize至最低值。典型缓冲大小为5-15ms。
        /// todo: 增加方法以强制禁用此方式，以便节省性能。
        /// </summary>
        public static extern bool IsLowLatency {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_IsLowLatency")] get;
        }
        
        #endregion
        
        #region Debug
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LogFunc([MarshalAs(UnmanagedType.LPStr)] string s);

        private static extern LogFunc DebugLogFunction {
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MaAudio_SetDebugLogFunction", CharSet = CharSet.Ansi)] set;
        }
        
        #endregion

        #region Utility Methods

        /// <summary>
        /// 异步从Unity的AudioClip数据读入采样并创建混音器可用的音频数据。
        /// 会拷贝完整音频数据至混音器，中途可能会消耗3n的内存大小（AudioClip，GetData的临时数据，以及内部数据）。
        /// todo：分段读取音频数据以节省中间内存大小，并缓存临时数组
        /// </summary>
        public static async UniTask<IntPtr> CreateAudioFromClipAsync(AudioClip clip) {
            await UniTask.SwitchToMainThread();
            
            int channelCount = clip.channels;
            int sampleCount = clip.samples;
            int frequency = clip.frequency;
            
            if (clip == null || (channelCount != 2 && channelCount != 1)) {
                return NullAudio;
            }

            float[] data = new float[sampleCount * channelCount];
            if (!clip.GetData(data, 0)) {
                return NullAudio;
            }

            await UniTask.SwitchToThreadPool();
            
            if (channelCount == 1) {
                var stereo = new float[sampleCount * 2];
                for (int i = 0; i < sampleCount; ++i) {
                    stereo[i * 2 + 1] = stereo[i * 2] = data[i];
                }
                data = stereo;
            }
            
            return CreateAudioWithData(data, sampleCount, frequency);
        }

        // FMOD 已经从框架代码移除，以下仅供参考
        /*
        private const int readAllocSize = 3072; // multiple times of 1, 2, 3, and 4 bytes that fits inside 4K cache size

        /// <summary>
        /// 用FMOD的解码器，从一个url加载音频数据至MaAudio。阻塞调用。
        /// todo：固定长度buffer，分段加载进MaAudio
        /// </summary>
        public static IntPtr CreateAudioFromUrl(string url, out double length) {
            length = 0;
            var unit = AudioEngineFMOD.SingletonInstance.CreateAudio(url, true, true);
            return unit == null ? NullAudio : CreateAudioFromFMOD(unit, out length);
        }
        
        // FIXME: 开源哥你来解释解释，为啥不能用？
        public static IntPtr CreateAudioFromBytes(byte[] bytes, out double length) {
            length = 0;
            var unit = AudioEngineFMOD.SingletonInstance.CreateAudio(bytes, true);
            return unit == null ? NullAudio : CreateAudioFromFMOD(unit, out length);
        }

        private static IntPtr CreateAudioFromFMOD(AudioEngineFMOD.AudioUnit unit, out double length) {
            length = 0;
            
            unit.sound.getDefaults(out var sampleRate, out _);
            unit.sound.getFormat(out _, out var format, out var channelCount, out var bits);
            unit.sound.getLength(out var sampleCount, TIMEUNIT.PCM);
            if (channelCount > 2 || channelCount < 1) return NullAudio;
            
            int sampleSize = bits / 8;
            float sampleAmpInv = 1.0f / (1L << (bits - 1));

            Func<byte[], int, int> convertBytes;
            switch (sampleSize) { // the following lambdas have no captures, thus no alloc
                case 1: convertBytes = (b, i) => unchecked((sbyte)(b[i] ^ (byte)0x80)); break; // 8-bit samples are unsigned
                case 2: convertBytes = (b, i) => BitConverter.ToInt16(b, i); break;
                case 4: convertBytes = BitConverter.ToInt32; break;
                case 3: convertBytes = (b, i) => (b[i] | b[i + 1] << 8 | (sbyte)b[i + 2] << 16); break;
                default: return NullAudio;
            }
            
            var samples = new float[sampleCount * channelCount];
            int sampleIndex = 0;
            
            var buffer = Marshal.AllocHGlobal(readAllocSize);
            var tempBytes = new byte[readAllocSize];

            var result = RESULT.OK;
            while (result == RESULT.OK) {
                result = unit.sound.readData(buffer, readAllocSize, out var readBytes);
                Assert.IsTrue(readBytes % sampleSize == 0);
                Assert.IsTrue(sampleIndex + readBytes / sampleSize <= samples.Length);
                if (format == SOUND_FORMAT.PCMFLOAT) {
                    int deltaIndex = (int) readBytes / sampleSize;
                    Marshal.Copy(buffer, samples, sampleIndex, deltaIndex);
                    sampleIndex += deltaIndex;
                } else {
                    Marshal.Copy(buffer, tempBytes, 0, (int)readBytes);
                    for (int i = 0; i < readBytes; i += sampleSize, ++sampleIndex) {
                        samples[sampleIndex] = convertBytes(tempBytes, i) * sampleAmpInv;
                    }
                }
            }

            if (result != RESULT.ERR_FILE_EOF) return NullAudio;
            
            Marshal.FreeHGlobal(buffer);
            unit.Unload();

            if (channelCount == 1) {
                var stereo = new float[sampleCount * 2];
                for (int i = 0; i < sampleCount; ++i) {
                    stereo[i * 2 + 1] = stereo[i * 2] = samples[i];
                }
                samples = stereo;
            }
            
            var audio = CreateAudioWithData(samples, (int)sampleCount, (int)sampleRate);
            if (audio != NullAudio) length = sampleCount / sampleRate;
            return audio;
        }
        */
        
        #endregion

    }
}