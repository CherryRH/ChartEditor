using ChartEditor.Models;
using ChartEditor.ViewModels;
using ManagedBass;
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

        private double musicStartTime;

        private Stopwatch stopwatch = new Stopwatch();

        public MusicPlayer(ChartEditModel chartEditModel)
        {
            this.ChartInfo = chartEditModel.ChartInfo;
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
            return (long)(second * Bass.ChannelGetInfo(streamHandle).Frequency * 4);
        }

        /// <summary>
        /// 重播音乐，播放失败返回false
        /// </summary>
        public bool ReplayMusic()
        {
            try
            {
                this.musicStartTime = Math.Max(this.ChartInfo.Delay, double.Epsilon);

                Bass.ChannelSetPosition(streamHandle, this.GetPositionFromSecond(this.musicStartTime));
                if (!Bass.ChannelPlay(streamHandle)) throw new Exception("播放失败");

                stopwatch.Restart();
                Console.WriteLine(logTag + "重新播放");
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
        public bool PlayMusic(double currentTime)
        {
            try
            {
                this.musicStartTime = currentTime;

                Bass.ChannelSetPosition(streamHandle, this.GetPositionFromSecond(this.musicStartTime));

                if (!Bass.ChannelPlay(streamHandle)) throw new Exception("播放失败");

                stopwatch.Restart();
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
        /// 停止音乐播放
        /// </summary>
        public void PauseMusic()
        {
            Bass.ChannelStop(streamHandle);
            stopwatch.Stop();
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

        /// <summary>
        /// 获取当前音乐播放的时间（秒）
        /// </summary>
        public double GetMusicTime()
        {
            return this.musicStartTime + stopwatch.Elapsed.TotalSeconds;
        }
    }
}
