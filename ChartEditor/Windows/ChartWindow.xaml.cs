using ChartEditor.Models;
using ChartEditor.Pages;
using ChartEditor.UserControls.Boards;
using ChartEditor.Utils;
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
using System.Windows.Shapes;

namespace ChartEditor.Windows
{
    /// <summary>
    /// ChartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChartWindow : Window
    {
        private ChartEditPage ChartEditPage;

        private MainWindowModel MainWindowModel;

        private ChartListModel ChartListModel;

        public ChartWindow(ChartInfo chartInfo, MainWindowModel mainWindowModel, ChartListModel chartListModel)
        {
            InitializeComponent();
            this.Title = Common.GenerateChartWindowTitle(chartInfo);
            this.MainWindowModel = mainWindowModel;
            this.ChartListModel = chartListModel;
            ChartEditPage chartEditPage = new ChartEditPage(chartInfo, mainWindowModel, chartListModel, this);
            this.ChartEditPage = chartEditPage;
            ChartPage.Navigate(chartEditPage);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 保存谱面和工作区

            // 刷新谱面列表

            // 更新曲目数据
            this.ChartListModel.GetChartInfos();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.ChartEditPage.TrackEditBoard.Dispose();
            // 唤起主窗口
            this.MainWindowModel.MainWindow.Show();
        }
    }
}
