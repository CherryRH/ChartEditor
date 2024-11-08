using ManagedBass;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Png;

namespace ChartEditor.Utils.AudioUtils
{
    /// <summary>
    /// 混音器
    /// </summary>
    public class AudioMixer
    {
        /// <summary>
        /// 播放器
        /// </summary>
        private WaveOutEvent waveOutEvent = new WaveOutEvent();

        /// <summary>
        /// 混音器
        /// </summary>
        private MixingSampleProvider mixer = new MixingSampleProvider(NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
        {
            ReadFully = true
        };

        public AudioMixer()
        {
            this.waveOutEvent.Init(this.mixer);
            this.SetVolume(1);
            if (!Bass.Init())
            {
                Console.WriteLine("BASS初始化失败");
            }
        }

        public void AddMixerInput(VolumeSampleProvider provider)
        {
            this.mixer.AddMixerInput(provider);
        }

        public void RemoveMixerInput(VolumeSampleProvider provider)
        {
            this.mixer.RemoveMixerInput(provider);
        }

        public void RemoveAllMixerInput()
        {
            this.mixer.RemoveAllMixerInputs();
        }

        public bool ContainsMixerInput(VolumeSampleProvider provider)
        {
            return this.mixer.MixerInputs.Contains(provider);
        }

        public void SetVolume(float volume)
        {
            if (volume < 0 || volume > 1) return;
            this.waveOutEvent.Volume = volume;
        }

        public void Dispose()
        {
            this.waveOutEvent.Dispose();
        }
    }
}
