using ChartEditor.Models;
using ChartEditor.ViewModels;
using ManagedBass;
using ManagedBass.Fx;
using System;
using System.Diagnostics;

namespace ChartEditor.Utils.MusicUtils
{
    /// <summary>
    /// 音乐播放器
    /// </summary>
    public class MusicPlayer
    {
        private static string logTag = "[MusicPlayer]";

        private int streamHandle;

        private float volume = 0.5f;

        private ChartInfo ChartInfo;

        private Timer Timer;

        public MusicPlayer(ChartEditModel chartEditModel, Timer timer)
        {
            this.ChartInfo = chartEditModel.ChartInfo;
            this.Timer = timer;
            this.volume = chartEditModel.MusicVolume / 100;

            streamHandle = Bass.CreateStream(this.ChartInfo.ChartMusic.GetMusicPath(), Flags: BassFlags.Default);
            if (streamHandle == 0)
            {
                Console.WriteLine(logTag + "音频加载失败：" + Bass.LastError);
            }
            Bass.ChannelSetAttribute(streamHandle, ChannelAttribute.Volume, this.volume);
        }

        /// <summary>
        /// 将秒数时间转化为音频流字节偏移量
        /// </summary>
        public long GetPositionFromSecond(double second)
        {
            long totalBytes = Bass.ChannelGetLength(streamHandle);
            double totalSeconds = Bass.ChannelBytes2Seconds(streamHandle, Bass.ChannelGetLength(streamHandle));
            return (long)((second / totalSeconds) * totalBytes);
        }

        /// <summary>
        /// 设置音乐位置
        /// </summary>
        public bool SetMusic(double currentTime)
        {
            try
            {
                Bass.ChannelSetPosition(streamHandle, this.GetPositionFromSecond(currentTime));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 播放音乐
        /// </summary>
        public bool PlayMusic()
        {
            try
            {
                if (!Bass.ChannelPlay(streamHandle)) throw new Exception("播放失败");
                Console.WriteLine(logTag + "播放开始");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 暂停音乐
        /// </summary>
        public void PauseMusic()
        {
            Bass.ChannelStop(streamHandle);
        }

        /// <summary>
        /// 音乐是否接近结束
        /// </summary>
        public bool IsMusicAboutOver(double currentTime)
        {
            return currentTime >= this.ChartInfo.ChartMusic.Duration - 0.5;
        }

        /// <summary>
        /// 设置音量，参数在0-1之间
        /// </summary>
        public void SetVolume(float volume)
        {
            if (volume < 0 || volume > 1) return;
            this.volume = volume;
            if (streamHandle != 0)
            {
                Bass.ChannelSetAttribute(streamHandle, ChannelAttribute.Volume, volume);
            }
        }

        public void Dispose()
        {
            // 释放资源
            if (streamHandle != 0)
            {
                Bass.StreamFree(streamHandle);
            }
            Bass.Free();
        }
    }
}
