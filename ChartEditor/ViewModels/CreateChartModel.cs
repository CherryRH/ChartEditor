using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.ViewModels
{
    public class CreateChartModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 谱面名称
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// 谱师名称
        /// </summary>
        public string Author { set; get; }

        public CreateChartModel() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
