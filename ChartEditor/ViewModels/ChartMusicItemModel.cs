using ChartEditor.Models;
using ChartEditor.Utils.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChartEditor.ViewModels
{
    public class ChartMusicItemModel : INotifyPropertyChanged
    {
        private ChartMusic chartMusic;
        public ChartMusic ChartMusic { get { return chartMusic; } }

        public string Title { get { return this.chartMusic.Title; } }

        public string Artist { get { return this.chartMusic.Artist; } }

        public string Bpm { get { return "Bpm：" + this.chartMusic.Bpm.ToString(); } }

        public string Duration { get { return "时长：" + this.chartMusic.GetDurationFormatString(); } }

        public BitmapImage Cover
        {
            get
            {
                return CoverImageCache.Instance.GetImage(this.chartMusic.CoverPath);
            }
        }

        public string CreatedAt { get { return "创建时间：" + this.chartMusic.CreatedAt.ToString(); } }

        public string UpdatedAt { get { return "更新时间：" + this.chartMusic.UpdatedAt.ToString(); } }

        public ChartMusicItemModel()
        {
            
        }

        public ChartMusicItemModel(ChartMusic chartMusic)
        {
            this.chartMusic = chartMusic;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
