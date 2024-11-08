using ChartEditor.Models;
using ChartEditor.Utils.AudioUtils;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
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
        /// 音源
        /// </summary>
        private VolumeSampleProvider provider;

        /// <summary>
        /// 混音器
        /// </summary>
        private AudioMixer AudioMixer;

        private ChartInfo ChartInfo;

        private float volume = 0.5f;
        public float Volume { get { return volume; } set { volume = value; } }

        /// <summary>
        /// 本次音乐开始播放的时间
        /// </summary>
        private double musicStartTime;

        /// <summary>
        /// 计时器（提供本次播放持续的时间）
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();

        public MusicPlayer(ChartEditModel chartEditModel, AudioMixer audioMixer)
        {
            this.ChartInfo = chartEditModel.ChartInfo;
            this.volume = chartEditModel.MusicVolume / 100;
            this.vorbisReader = new VorbisWaveReader(this.ChartInfo.ChartMusic.GetMusicPath());
            this.AudioMixer = audioMixer;
            this.provider = new VolumeSampleProvider(this.vorbisReader.ToSampleProvider()) { Volume = this.volume };
        }

        /// <summary>
        /// 重播音乐，播放失败返回false
        /// </summary>
        public bool ReplayMusic()
        {
            try
            {
                this.musicStartTime = Math.Max(this.ChartInfo.Delay, double.Epsilon);
                this.vorbisReader.CurrentTime = TimeSpan.FromSeconds(this.musicStartTime);
                this.AudioMixer.AddMixerInput(this.provider);
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
        /// 播放音乐，播放失败返回false
        /// </summary>
        public bool PlayMusic(double currentTime)
        {
            try
            {
                this.musicStartTime = currentTime;
                this.vorbisReader.CurrentTime = TimeSpan.FromSeconds(currentTime);
                this.AudioMixer.AddMixerInput(this.provider);
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
        /// 音乐是否接近结束
        /// </summary>
        public bool IsMusicAboutOver(double currentTime)
        {
            // 减少音乐结尾处报错
            return currentTime >= this.vorbisReader.TotalTime.TotalSeconds - 1;
        }

        public void PauseMusic()
        {
            Console.WriteLine(logTag + "播放暂停");
            this.AudioMixer.RemoveMixerInput(this.provider);
            this.stopwatch.Stop();
        }

        /// <summary>
        /// 设置音量，参数在0-1之间
        /// </summary>
        public void SetVolume(float volume)
        {
            if (volume < 0 || volume > 1) return;
            this.volume = volume;
            if (this.provider != null) this.provider.Volume = volume;
        }

        public void Dispose()
        {
            this.vorbisReader.Dispose();
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
