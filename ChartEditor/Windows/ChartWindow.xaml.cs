using ChartEditor.Models;
using ChartEditor.Pages;
using ChartEditor.UserControls.Boards;
using ChartEditor.UserControls.Dialogs;
using ChartEditor.Utils;
using ChartEditor.Utils.ChartUtils;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
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

        private ChartEditModel ChartEditModel;

        private bool isClosingHandled = false;

        private bool isDialogShowing = false;

        public ChartWindow(MainWindowModel mainWindowModel, ChartListModel chartListModel, ChartEditModel chartEditModel)
        {
            InitializeComponent();
            chartEditModel.ChartWindow = this;
            this.ChartEditModel = chartEditModel;

            this.Title = Common.GenerateChartWindowTitle(chartEditModel.ChartInfo);
            this.MainWindowModel = mainWindowModel;
            this.ChartListModel = chartListModel;
            ChartEditPage chartEditPage = new ChartEditPage(mainWindowModel, chartListModel, chartEditModel);
            this.ChartEditPage = chartEditPage;
            ChartPage.Navigate(chartEditPage);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosingHandled || isDialogShowing) return;

            e.Cancel = true;
            isDialogShowing = true;

            var result = await DialogHost.Show(new ConfirmDialog("确定要保存谱面吗？"), "ChartWindowDialog");

            if (result is bool && (bool)result)
            {
                // 保存谱面和工作区
                await ChartUtilV1.SaveChart(this.ChartEditModel);
                ChartUtilV1.SaveWorkPlace(this.ChartEditModel);
                // 更新谱面列表数据
                this.ChartListModel.GetChartInfos();
                // 更新主页曲目
                this.MainWindowModel.UpdateChartMusic(this.ChartEditModel.ChartInfo.ChartMusic);
            }

            isClosingHandled = true;
            isDialogShowing = false;
            e.Cancel = false;

            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.ChartEditPage.TrackEditBoard.Dispose();
            // 唤起主窗口
            this.MainWindowModel.MainWindow.Show();
        }
    }
}
