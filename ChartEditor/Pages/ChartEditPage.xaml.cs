using ChartEditor.Models;
using ChartEditor.UserControls.Boards;
using ChartEditor.ViewModels;
using ChartEditor.Windows;
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
        /// 谱面列表数据
        /// </summary>
        private ChartListModel ChartListModel;

        /// <summary>
        /// 谱面编辑对象
        /// </summary>
        private ChartEditModel Model;

        /// <summary>
        /// 轨道编辑板对象
        /// </summary>
        private TrackEditBoard trackEditBoard;

        public ChartEditPage(MainWindowModel mainWindowModel, ChartListModel chartListModel, ChartEditModel chartEditModel)
        {
            InitializeComponent();
            this.MainWindowModel = mainWindowModel;
            this.ChartListModel = chartListModel;
            this.Model = chartEditModel;
            this.DataContext = this.Model;

            TrackEditBoard.DataContext = this.Model;
            TrackEditBoard.MainWindowModel = this.MainWindowModel;
            UnityBoard.DataContext = this.DataContext;
            AttributeEditBoard.DataContext = this.DataContext;

            this.trackEditBoard = TrackEditBoard;
        }
    }
}
