using ChartEditor.Models;
using ChartEditor.Utils.Cache;
using ChartEditor.Utils.ChartUtils;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TagLib.Png;

namespace ChartEditor.ViewModels
{
    public class ChartListModel : INotifyPropertyChanged
    {
        private static string logTag = "[ChartListModel]";

        /// <summary>
        /// 谱面歌曲信息
        /// </summary>
        private ChartMusic chartMusic;
        public ChartMusic ChartMusic { get { return chartMusic; } }

        public string Title { get { return this.chartMusic.Title; } }

        public BitmapImage Cover
        {
            get
            {
                return CoverImageCache.Instance.GetImage(this.chartMusic.CoverPath);
            }
        }

        /// <summary>
        /// 谱面列表
        /// </summary>
        private List<ChartInfo> chartInfos;
        public List<ChartInfo> ChartInfos { get { return chartInfos; } }

        public List<ChartItemModel> ChartItemModels { get { return CreateChartItemModels(); } }

        private List<ChartItemModel> CreateChartItemModels()
        {
            List<ChartItemModel> models = new List<ChartItemModel>();
            foreach (ChartInfo chartInfo in this.chartInfos)
            {
                models.Add(new ChartItemModel(chartInfo));
            }
            return models;
        }

        public int ChartNum { get { return chartInfos.Count; } }

        public ChartListModel(ChartMusic chartMusic)
        {
            // 从歌曲文件夹读取谱面列表
            this.chartInfos = ChartUtilV1.ReadChartList(chartMusic);
            this.SortChartInfosByUpdatedAt();
            this.chartMusic = chartMusic;
        }

        /// <summary>
        /// 添加谱面对象
        /// </summary>
        public void AddChart(ChartInfo chartInfo)
        {
            this.chartInfos.Add(chartInfo);
            this.SortChartInfosByUpdatedAt();
            this.OnPropertyChanged(nameof(ChartNum));
        }

        /// <summary>
        /// 根据更新时间对谱面列表进行排序
        /// </summary>
        public void SortChartInfosByUpdatedAt()
        {
            this.chartInfos.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
            this.OnPropertyChanged(nameof(ChartItemModels));
        }

        /// <summary>
        /// 歌曲是否存在，根据名称搜索
        /// </summary>
        public bool IsChartExist(string name)
        {
            if (this.chartInfos.Any(m => m.Name == name))
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// 删除谱面（回收站），删除失败返回false
        /// </summary>
        public bool DeleteChart(ChartInfo chartInfo)
        {
            try
            {
                if (Directory.Exists(chartInfo.FolderPath))
                {
                    FileSystem.DeleteDirectory(chartInfo.FolderPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                    this.chartInfos.Remove(chartInfo);
                    this.OnPropertyChanged(nameof(ChartItemModels));
                    this.OnPropertyChanged(nameof(ChartNum));
                    Console.WriteLine(logTag + "谱面文件夹已删除");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
