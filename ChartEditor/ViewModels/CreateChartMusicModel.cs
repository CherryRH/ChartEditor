using ChartEditor.Models;
using ChartEditor.UserControls.Dialogs;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using System.Windows;
using ChartEditor.Utils;

namespace ChartEditor.ViewModels
{
    public class CreateChartMusicModel : INotifyPropertyChanged
    {
        private static string logTag = "[CreateChartMusicModel]";

        /// <summary>
        /// 歌曲名
        /// </summary>
        private string title;
        public string Title { get { return  title; } set { title = value; } }

        /// <summary>
        /// 作曲家
        /// </summary>
        private string artist;
        public string Artist { get { return artist; } set { artist = value; } }

        /// <summary>
        /// 歌曲Bpm
        /// </summary>
        private double bpm;
        public double Bpm { get { return bpm; } set { bpm = value; } }

        /// <summary>
        /// 封面图片路径
        /// </summary>
        private string coverPath;
        public string CoverPath
        {
            get { return coverPath; }
            set
            {
                if (coverPath != value)
                {
                    coverPath = value;
                    OnPropertyChanged(nameof(CoverPath));
                }
            }
        }

        /// <summary>
        /// 音频路径
        /// </summary>
        private string musicPath;
        public string MusicPath
        {
            get { return musicPath; }
            set
            {
                if (musicPath != value)
                {
                    musicPath = value;
                    OnPropertyChanged(nameof(MusicPath));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CreateChartMusicModel()
        {
            this.coverPath = string.Empty;
            this.musicPath = string.Empty;
            this.artist = string.Empty;
            this.bpm = 120;
        }

        ~CreateChartMusicModel()
        {

        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 创建并设置歌曲文件夹以及其他文件，返回一个歌曲对象
        /// </summary>
        public async Task<ChartMusic> CreateChartMusicFolder()
        {
            try
            {
                // 歌曲文件夹路径
                string chartMusicPath = Path.Combine(Common.GetChartMusicFolderPath(), this.title);
                if (!Directory.Exists(chartMusicPath))
                {
                    Directory.CreateDirectory(chartMusicPath);
                }

                // 创建歌曲配置文件
                double duration;
                using (var file = TagLib.File.Create(this.musicPath))
                {
                    duration = file.Properties.Duration.TotalSeconds;
                }

                ChartMusic music = new ChartMusic(this.title, this.artist, this.bpm, duration, chartMusicPath);
                File.WriteAllText(Path.Combine(chartMusicPath, Common.ChartMusicConfigFileName), music.toJsonString());

                // 保存封面图片和音频文件
                if (!string.IsNullOrEmpty(this.coverPath))
                {
                    ImageUtil.ConvertToPng(this.coverPath, music.CoverPath);
                }
                else
                {
                    Uri resourceUri = new Uri("pack://application:,,,/Resources/Textures/音乐.png", UriKind.Absolute);
                    StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);
                    using (FileStream fileStream = new FileStream(music.CoverPath, FileMode.Create, FileAccess.Write))
                    {
                        resourceInfo.Stream.CopyTo(fileStream);
                        fileStream.Dispose();
                    }
                }

                if (Path.GetExtension(this.musicPath) == ".mp3")
                {
                    await MusicUtil.ConvertMp3ToOgg(this.musicPath, music.GetMusicPath());
                }
                else if (Path.GetExtension(this.musicPath) == ".ogg")
                {
                    File.Copy(this.musicPath, music.GetMusicPath(), overwrite: true);
                }

                Console.WriteLine(logTag + "歌曲文件夹已创建");
                return music;
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                return null;
            }
        }
    }
}
