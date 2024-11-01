﻿using ChartEditor.Models;
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
        /// 曲目名
        /// </summary>
        private string title;
        public string Title { get { return  title; } set { title = value; } }

        /// <summary>
        /// 作曲家
        /// </summary>
        private string artist;
        public string Artist { get { return artist; } set { artist = value; } }

        /// <summary>
        /// 曲目Bpm
        /// </summary>
        private string bpm;
        public string Bpm { get { return bpm; } set { bpm = value; } }

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
            this.bpm = "120";
        }

        ~CreateChartMusicModel()
        {

        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 创建并设置曲目文件夹以及其他文件，返回一个曲目对象
        /// </summary>
        public async Task<ChartMusic> CreateChartMusicFolder()
        {
            try
            {
                // 曲目文件夹路径
                DateTime createdAt = DateTime.Now;
                string chartMusicPath = ChartMusicUtil.GenerateNewChartMusicFolderPath(createdAt);
                if (!Directory.Exists(chartMusicPath))
                {
                    Directory.CreateDirectory(chartMusicPath);
                }

                ChartMusic music = new ChartMusic(this.title, this.artist, Math.Round(double.Parse(this.bpm), 1), 0, chartMusicPath, createdAt);

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

                // 保存曲目文件
                using (var file = TagLib.File.Create(music.GetMusicPath()))
                {
                    music.Duration = file.Properties.Duration.TotalSeconds;
                }
                ChartMusicUtil.SaveChartMusic(music);

                Console.WriteLine(logTag + "曲目文件夹已创建");
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
