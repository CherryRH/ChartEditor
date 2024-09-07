using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.ViewModels
{
    public class ChartItemModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 谱面信息
        /// </summary>
        private ChartInfo chartInfo;
        public ChartInfo ChartInfo { get { return chartInfo; } }

        public string Name { get { return this.chartInfo.Name; } }

        public string Difficult { get { return "Lv " + this.chartInfo.Difficulty.ToString(); } }

        public string Author { get { return "谱师：" + this.chartInfo.Author; } }

        public string CreatedAt { get { return "创建时间：" + this.chartInfo.CreatedAt.ToString(); } }

        public string UpdatedAt { get { return "更新时间：" + this.chartInfo.UpdatedAt.ToString(); } }

        public ChartItemModel()
        {

        }

        public ChartItemModel(ChartInfo chartInfo)
        {
            this.chartInfo = chartInfo;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
