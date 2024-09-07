using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Input;
using ChartEditor.Utils;
using ChartEditor.UserControls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic.FileIO;
using ChartEditor.Pages;
using ChartEditor.Utils.Cache;
using System.Collections.ObjectModel;

namespace ChartEditor.ViewModels
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private static string logTag = "[MainWindowModel]";

        /// <summary>
        /// 保存主页实例
        /// </summary>
        private ChartMusicListPage chartMusicListPage;
        public ChartMusicListPage ChartMusicListPage { get { return chartMusicListPage; } set { chartMusicListPage = value; } }

        /// <summary>
        /// 谱面歌曲数组
        /// </summary>
        private List<ChartMusic> chartMusics;
        public List<ChartMusic> ChartMusics { get { return chartMusics; } }
        public int ChartMusicNum { get { return chartMusics.Count; } }
        public List<ChartMusicItemModel> ChartMusicItemModels {  get { return CreateChartMusicItemModels(); } }

        /// <summary>
        /// 构建歌曲列表
        /// </summary>
        private List<ChartMusicItemModel> CreateChartMusicItemModels()
        {
            List<ChartMusicItemModel> models = new List<ChartMusicItemModel>();
            foreach (ChartMusic chartMusic in this.chartMusics)
            {
                models.Add(new ChartMusicItemModel(chartMusic));
            }
            return models;
        }

        /// <summary>
        /// 制谱器设置
        /// </summary>
        private Settings settings;
        public Settings Settings { get { return settings; } }

        public MainWindowModel()
        {
            this.settings = new Settings();
            // 读取歌曲列表
            this.chartMusics = ChartMusicReader.ReadChartMusics();
            this.SortChartMusicsByUpdatedAt();
        }

        ~MainWindowModel()
        {
            
        }

        /// <summary>
        /// 添加歌曲对象
        /// </summary>
        public void AddChartMusic(ChartMusic chartMusic)
        {
            this.chartMusics.Add(chartMusic);
            this.SortChartMusicsByUpdatedAt();
            this.OnPropertyChanged(nameof(ChartMusicNum));
            this.OnPropertyChanged(nameof(ChartMusicItemModels));
        }

        /// <summary>
        /// 根据更新时间对歌曲列表进行排序
        /// </summary>
        public void SortChartMusicsByUpdatedAt()
        {
            this.chartMusics.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
            this.OnPropertyChanged(nameof(ChartMusicItemModels));
        }

        /// <summary>
        /// 歌曲是否存在，根据标题搜索
        /// </summary>
        public bool IsChartMusicExist(string title)
        {
            if (this.chartMusics.Any(m => m.Title == title))
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// 删除歌曲及其所有谱面和文件（回收站）
        /// </summary>
        public void DeleteChartMusic(ChartMusic chartMusic)
        {
            try
            {
                if (Directory.Exists(chartMusic.FolderPath))
                {
                    chartMusic.CoverPath = string.Empty;
                    this.chartMusics.Remove(chartMusic);
                    this.OnPropertyChanged(nameof(ChartMusicNum));
                    this.OnPropertyChanged(nameof(ChartMusicItemModels));
                    FileSystem.DeleteDirectory(chartMusic.FolderPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    // 移除封面的缓存
                    CoverImageCache.Instance.RemoveImage(chartMusic.CoverPath);
                    Console.WriteLine(logTag + "歌曲文件夹已删除");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }
        }

        /// <summary>
        /// 更新歌曲信息
        /// </summary>
        public void UpdateChartMusic(ChartMusic chartMusic, string newTitle, string newArtist, double newBpm)
        {
            try
            {
                if (chartMusic == null || !this.chartMusics.Contains(chartMusic))
                {
                    return;
                }
                string oldTitle = chartMusic.Title;
                chartMusic.Title = newTitle;
                chartMusic.Artist = newArtist;
                chartMusic.Bpm = newBpm;
                chartMusic.UpdateAtNow();
                File.WriteAllText(Path.Combine(chartMusic.FolderPath, Common.ChartMusicConfigFileName), chartMusic.toJsonString());
                if (oldTitle != newTitle)
                {
                    Directory.Move(chartMusic.FolderPath, Path.Combine(Common.GetChartMusicFolderPath(), newTitle));
                    chartMusic.FolderPath = Path.Combine(Common.GetChartMusicFolderPath(), newTitle);
                }
                Console.WriteLine(logTag + "歌曲信息已更新");
                this.SortChartMusicsByUpdatedAt();
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }
        }

        public void UpdateChartMusic(ChartMusic chartMusic)
        {
            try
            {
                if (chartMusic == null || !this.chartMusics.Contains(chartMusic))
                {
                    return;
                }
                chartMusic.UpdateAtNow();
                File.WriteAllText(Path.Combine(chartMusic.FolderPath, Common.ChartMusicConfigFileName), chartMusic.toJsonString());
                Console.WriteLine(logTag + "歌曲信息已更新");
                this.SortChartMusicsByUpdatedAt();
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
