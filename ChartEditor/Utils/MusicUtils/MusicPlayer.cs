using ChartEditor.Models;
using MaterialDesignThemes.Wpf;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TagLib.Png;

namespace ChartEditor.Utils.MusicUtils
{
    /// <summary>
    /// 音乐播放器
    /// </summary>
    public class MusicPlayer
    {
        private static string logTag = "[MusicPlayer]";
        /// <summary>
        /// Ogg解码器
        /// </summary>
        private VorbisWaveReader vorbisReader;

        /// <summary>
        /// 播放器
        /// </summary>
        private WaveOutEvent player;

        /// <summary>
        /// 谱面信息
        /// </summary>
        private ChartInfo chartInfo;

        public MusicPlayer(ChartInfo chartInfo, EventHandler<StoppedEventArgs> playbackStopped)
        {
            this.chartInfo = chartInfo;
            this.vorbisReader = new VorbisWaveReader(this.chartInfo.ChartMusic.GetMusicPath());
            this.vorbisReader.CurrentTime = TimeSpan.FromSeconds(Math.Max(this.chartInfo.Delay, double.Epsilon));
            this.player = new WaveOutEvent();
            this.player.Init(this.vorbisReader);
            this.player.PlaybackStopped += playbackStopped;
        }

        /// <summary>
        /// 重播音乐，播放失败返回false
        /// </summary>
        public bool ReplayMusic()
        {
            try
            {
                this.vorbisReader.CurrentTime = TimeSpan.FromSeconds(Math.Max(this.chartInfo.Delay, double.Epsilon));
                this.player.Play();
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
        /// 播放音乐，播放失败返回false
        /// </summary>
        public bool PlayMusic(double currentTime)
        {
            try
            {
                this.vorbisReader.CurrentTime = TimeSpan.FromSeconds(currentTime);
                this.player.Play();
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
        /// 音乐是否结束
        /// </summary>
        public bool IsMusicOver(double currentTime)
        {
            // 减少音乐结尾处报错
            return currentTime >= this.vorbisReader.TotalTime.TotalSeconds - 1;
        }

        public void PauseMusic()
        {
            Console.WriteLine(logTag + "播放暂停");
            this.player.Pause();
        }

        /// <summary>
        /// 设置音量，参数在0-1之间
        /// </summary>
        public void SetVolume(float volume)
        {
            this.player.Volume = volume;
        }

        public void Dispose()
        {
            this.vorbisReader.Dispose();
            this.player.Dispose();
        }

        ~MusicPlayer()
        {
            this.Dispose();
        }
    }
}
