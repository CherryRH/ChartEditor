using ChartEditor.Models;
using ChartEditor.Utils.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.ViewModels
{
    public class ChartMusicEditModel : INotifyPropertyChanged
    {
        private static string logTag = "[ChartMusicEditModel]";

        /// <summary>
        /// 曲目名
        /// </summary>
        private string title;
        public string Title { get { return title; } set { title = value; } }

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

        public ChartMusicEditModel(ChartMusic chartMusic)
        {
            this.coverPath = chartMusic.CoverPath;
            this.title = chartMusic.Title;
            this.artist = chartMusic.Artist;
            this.bpm = chartMusic.Bpm.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
