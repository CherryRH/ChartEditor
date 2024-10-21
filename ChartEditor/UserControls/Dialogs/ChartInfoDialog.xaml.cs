using ChartEditor.Models;
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
using System.Windows.Shapes;

namespace ChartEditor.UserControls.Dialogs
{
    /// <summary>
    /// ChartInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ChartInfoDialog : UserControl
    {
        private ChartEditModel ChartEditModel;
        private ChartInfo ChartInfo { get { return ChartEditModel.ChartInfo; } }

        private ChartInfoModel ChartInfoModel;

        public ChartInfoDialog(ChartEditModel chartEditModel)
        {
            InitializeComponent();
            this.ChartEditModel = chartEditModel;
            this.ChartInfoModel = new ChartInfoModel(ChartInfo);
            this.DataContext = this.ChartInfoModel;
        }

        public async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            int parsedDifficulty = 0;
            double parsedDelay = 0.0;
            double parsedPreview = 0.0;
            if (string.IsNullOrWhiteSpace(this.ChartInfoModel.ChartName))
            {
                await DialogHost.Show(new WarnDialog("谱面名称不能为空哦~"), "ChartInfoDialog");
                return;
            }
            if (this.ChartInfoModel.ChartName != this.ChartInfo.Name && Directory.Exists(System.IO.Path.Combine(this.ChartInfo.ChartMusic.FolderPath, this.ChartInfoModel.ChartName)))
            {
                await DialogHost.Show(new WarnDialog("此谱面名称文件夹已经存在了，换个名字吧"), "ChartInfoDialog");
                return;
            }
            if (string.IsNullOrWhiteSpace(this.ChartInfoModel.Author))
            {
                await DialogHost.Show(new WarnDialog("谱师名不能为空哦~"), "ChartInfoDialog");
                return;
            }
            if (!int.TryParse(this.ChartInfoModel.Difficulty, out parsedDifficulty) || parsedDifficulty < 0)
            {
                await DialogHost.Show(new WarnDialog("难度要为非负整数值"), "ChartInfoDialog");
                return;
            }
            if (!double.TryParse(this.ChartInfoModel.Delay, out parsedDelay) || parsedDelay < 0)
            {
                await DialogHost.Show(new WarnDialog("音乐延迟要为非负毫秒数"), "ChartInfoDialog");
                return;
            }
            if (parsedDelay / 1000 > this.ChartInfo.ChartMusic.Duration - 5)
            {
                await DialogHost.Show(new WarnDialog("音乐延迟超过允许范围"), "ChartInfoDialog");
                return;
            }
            if (!double.TryParse(this.ChartInfoModel.Preview, out parsedPreview) || parsedPreview < 0)
            {
                await DialogHost.Show(new WarnDialog("预览时间要为非负秒数"), "ChartInfoDialog");
                return;
            }
            if (parsedPreview >= 10000)
            {
                await DialogHost.Show(new WarnDialog("预览时间超过允许范围"), "ChartInfoDialog");
                return;
            }
            // 保存字段
            this.ChartInfo.Name = this.ChartInfoModel.ChartName;
            this.ChartInfo.Author = this.ChartInfoModel.Author;
            this.ChartInfo.Difficulty = parsedDifficulty;
            this.ChartInfo.Delay = Math.Round(parsedDelay / 1000, 4);
            this.ChartInfo.Preview = Math.Round(parsedPreview, 1);
            DialogHost.CloseDialogCommand.Execute(true, this);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }
    }
}
