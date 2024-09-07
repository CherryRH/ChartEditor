using ChartEditor.Models;
using ChartEditor.Utils;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ChartEditor.UserControls.Dialogs
{
    /// <summary>
    /// CreateChartDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CreateChartDialog : UserControl
    {
        private CreateChartModel Model;

        /// <summary>
        /// 主页数据
        /// </summary>
        private MainWindowModel MainWindowModel;

        /// <summary>
        /// 谱面列表数据
        /// </summary>
        private ChartListModel ChartListModel;

        public CreateChartDialog(CreateChartModel createChartModel, MainWindowModel mainWindowModel, ChartListModel chartListModel)
        {
            InitializeComponent();
            this.Model = createChartModel;
            this.DataContext = this.Model;
            this.MainWindowModel = mainWindowModel;
            this.ChartListModel = chartListModel;

            // 将谱师名设置为默认用户名
            this.Model.Author = this.MainWindowModel.Settings.Username;
        }

        /// <summary>
        /// 处理创建新谱面按钮点击事件
        /// </summary>
        private async void CreateChartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Model.Name))
            {
                await DialogHost.Show(new WarnDialog("谱面名称不能为空哦~"), "CreateChartDialog");
                return;
            }
            if (this.ChartListModel.IsChartExist(this.Model.Name))
            {
                await DialogHost.Show(new WarnDialog("此谱面名称已经创建了哦~"), "CreateChartDialog");
                return;
            }
            if (Directory.Exists(Path.Combine(this.ChartListModel.ChartMusic.FolderPath, this.Model.Name)))
            {
                var result = await DialogHost.Show(new WarnDialog("此谱面名称文件夹已经存在了，无法创建"), "CreateChartDialog");
                return;
            }
            if (string.IsNullOrWhiteSpace(this.Model.Author))
            {
                await DialogHost.Show(new WarnDialog("谱师名不能为空哦~"), "CreateChartDialog");
                return;
            }

            DialogHost.CloseDialogCommand.Execute(new ChartInfo(this.ChartListModel.ChartMusic, this.Model.Name, this.Model.Author), this);
        }

        /// <summary>
        /// 处理取消按钮点击事件
        /// </summary>
        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }
    }
}
