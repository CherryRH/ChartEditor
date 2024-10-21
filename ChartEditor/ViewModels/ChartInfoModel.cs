using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.ViewModels
{
    public class ChartInfoModel : INotifyPropertyChanged
    {
        public string ChartName { set; get; }
        public string Author { set; get; }
        public string Difficulty { set; get; }
        public string Delay { get; set; }
        public string Preview { get; set; }

        public ChartInfoModel(ChartInfo chartInfo)
        {
            this.ChartName = chartInfo.Name;
            this.Author = chartInfo.Author;
            this.Difficulty = chartInfo.Difficulty.ToString();
            this.Delay = (chartInfo.Delay * 1000).ToString();
            this.Preview = chartInfo.Preview.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
