using ChartEditor.Models;
using ChartEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartEditor.Pages
{
    /// <summary>
    /// ChartEditPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChartEditPage : Page
    {
        /// <summary>
        /// 主窗口数据
        /// </summary>
        private MainWindowModel MainWindowModel;

        /// <summary>
        /// 
        /// </summary>
        private ChartListModel ChartListModel;

        /// <summary>
        /// 谱面编辑对象
        /// </summary>
        private ChartEditModel Model;

        public ChartEditPage(ChartInfo chartInfo, MainWindowModel mainWindowModel, ChartListModel chartListModel)
        {
            InitializeComponent();
            this.MainWindowModel = mainWindowModel;
            this.ChartListModel = chartListModel;
            this.Model = new ChartEditModel(chartInfo);
            this.DataContext = this.Model;
        }
    }
}
