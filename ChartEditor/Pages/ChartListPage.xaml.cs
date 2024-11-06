using ChartEditor.Models;
using ChartEditor.UserControls.Boards;
using ChartEditor.UserControls.Dialogs;
using ChartEditor.Utils.Cache;
using ChartEditor.Utils.ChartUtils;
using ChartEditor.ViewModels;
using ChartEditor.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartEditor.Pages
{
    /// <summary>
    /// ChartListPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChartListPage : Page
    {
        /// <summary>
        /// 制谱器主页数据
        /// </summary>
        private MainWindowModel MainWindowModel;

        /// <summary>
        /// 谱面列表数据
        /// </summary>
        private ChartListModel Model;

        public ChartListPage(MainWindowModel mainWindowModel, ChartListModel chartListModel)
        {
            InitializeComponent();
            this.MainWindowModel = mainWindowModel;
            this.Model = chartListModel;
            this.DataContext = Model;
        }

        /// <summary>
        /// 处理主页按钮点击事件
        /// </summary>
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(this.MainWindowModel.ChartMusicListPage);
        }

        /// <summary>
        /// 处理创建新谱面按钮点击事件
        /// </summary>
        private async void CreateChartButton_Click(object sender, RoutedEventArgs e)
        {
            ChartInfo result = (ChartInfo)await DialogHost.Show(new CreateChartDialog(new CreateChartModel(), this.MainWindowModel, this.Model), "ChartListDialog");
            if (result == null) { return; }
            // 创建基础谱面文件
            ChartUtilV1.CreateBasicChartFile(result);
            // 更新谱面列表
            this.Model.AddChart(result);
            this.MainWindowModel.UpdateChartMusic(this.Model.ChartMusic);
        }

        /// <summary>
        /// 处理谱面点击事件
        /// </summary>
        private async void ChartItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button item && item.DataContext is ChartItemModel selectedItem)
            {
                ChartEditModel chartEditModel = new ChartEditModel(selectedItem.ChartInfo);
                // 加载窗口
                LoadingDialog loadingDialog = new LoadingDialog("正在加载谱面~");

                ChartListDialog.CloseOnClickAway = false;
                // 异步显示 loadingDialog，并等待加载完成
                var dialogTask = DialogHost.Show(loadingDialog, "ChartListDialog");

                // 加载谱面文件
                bool loadChartSuccess = await ChartUtilV1.LoadChartData(chartEditModel);

                // 加载工作区文件
                bool loadWorkspaceSuccess = ChartUtilV1.LoadWorkPlaceData(chartEditModel);

                // 加载完成后，关闭 loadingDialog
                DialogHost.CloseDialogCommand.Execute(null, loadingDialog);
                ChartListDialog.CloseOnClickAway = true;

                if (!loadChartSuccess)
                {
                    // 加载失败显示警告
                    await DialogHost.Show(new WarnDialog("谱面加载失败"), "ChartListDialog");
                    return;
                }
                ChartListDialog.CloseOnClickAway = true;
                // 谱面加载成功，打开编辑窗口
                this.MainWindowModel.MainWindow.Hide();
                ChartWindow chartWindow = new ChartWindow(this.MainWindowModel, this.Model, chartEditModel);
                chartWindow.Show();
            }
        }

        /// <summary>
        /// 处理谱面设置点击事件
        /// </summary>
        private void ChartSettingButton_Click(Object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// 处理谱面删除点击事件
        /// </summary>
        private async void ChartDeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            bool result = (bool?)await DialogHost.Show(new ConfirmDialog("确认要删除谱面吗？删除后谱面还可以在回收站恢复。"), "ChartListDialog") ?? false;
            if (result)
            {
                if (sender is Button item && item.DataContext is ChartItemModel selectedItem)
                {
                    bool deleteResult = this.Model.DeleteChart(selectedItem.ChartInfo);
                    if (deleteResult)
                    {
                        this.MainWindowModel.UpdateChartMusic(this.Model.ChartMusic);
                    }
                }
            }
        }
    }
}
