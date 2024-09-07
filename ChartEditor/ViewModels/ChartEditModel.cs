using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.ViewModels
{
    public class ChartEditModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 谱面信息
        /// </summary>
        private ChartInfo chartInfo;
        public ChartInfo ChartInfo { get { return chartInfo; } }


        public ChartEditModel(ChartInfo chartInfo)
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
