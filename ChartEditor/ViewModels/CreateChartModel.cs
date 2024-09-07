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
        private string name;
        public string Name { set { name = value; } get { return name; } }

        /// <summary>
        /// 谱师名称
        /// </summary>
        private string author;
        public string Author { set { author = value; } get { return author; } }

        public CreateChartModel() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
