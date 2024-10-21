using ChartEditor.Models;
using ChartEditor.Utils;
using ChartEditor.Utils.Cache;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
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
    /// ChartMusicEditDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ChartMusicEditDialog : UserControl
    {
        private ChartMusicEditModel Model;

        private MainWindowModel MainWindowModel;

        private ChartMusic ChartMusic;

        public ChartMusicEditDialog(ChartMusic chartMusic, MainWindowModel mainWindowModel)
        {
            InitializeComponent();
            this.ChartMusic = chartMusic;
            this.Model = new ChartMusicEditModel(chartMusic);
            this.DataContext = this.Model;
            this.MainWindowModel = mainWindowModel;
            MusicCover.Source = CoverImageCache.Instance.GetImage(this.Model.CoverPath);
        }

        private async void SelectCoverButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "图片文件 (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                if (openFileDialog.ShowDialog() == true)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    MusicCover.Source = bitmap;
                    Console.WriteLine("已选择封面图片文件");
                    this.Model.CoverPath = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                var result = await DialogHost.Show(new WarnDialog("图片读取失败，请换一张图片哦~"), "ChartMusicEditDialog");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            double parsedBpm = 0.0;
            if (string.IsNullOrWhiteSpace(this.Model.Title))
            {
                var result = await DialogHost.Show(new WarnDialog("曲目名不能为空哦~"), "ChartMusicEditDialog");
                return;
            }
            if (this.Model.Title != this.ChartMusic.Title && this.MainWindowModel.IsChartMusicExist(this.Model.Title))
            {
                var result = await DialogHost.Show(new WarnDialog("此曲目名已经创建了哦~"), "ChartMusicEditDialog");
                return;
            }
            if (Directory.Exists(System.IO.Path.Combine(Common.GetChartMusicFolderPath(), this.Model.Title)))
            {
                var result = await DialogHost.Show(new WarnDialog("此曲目名文件夹已经存在了，换个名字吧"), "ChartMusicEditDialog");
                return;
            }
            if (string.IsNullOrWhiteSpace(this.Model.Artist))
            {
                var result = await DialogHost.Show(new WarnDialog("作曲艺术家不能为空哦~"), "ChartMusicEditDialog");
                return;
            }
            if (!double.TryParse(this.Model.Bpm, out parsedBpm) || parsedBpm < 0)
            {
                await DialogHost.Show(new WarnDialog("BPM要为非负数字"), "ChartMusicEditDialog");
                return;
            }
            // 保存曲目信息
            this.ChartMusic.Title = this.Model.Title;
            this.ChartMusic.Artist = this.Model.Artist;
            this.ChartMusic.Bpm = Math.Round(parsedBpm, 2);
            // 保存封面
            if (this.ChartMusic.CoverPath != this.Model.CoverPath)
            {
                ImageUtil.ConvertToPng(this.Model.CoverPath, this.ChartMusic.CoverPath);
                // 重置缓存
                CoverImageCache.Instance.RemoveImage(this.ChartMusic.CoverPath);
            }
            
            DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}
