using ChartEditor.ViewModels;
using ChartEditor.UserControls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.IO;
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
using ChartEditor.Models;
using ChartEditor.Utils;

namespace ChartEditor.Pages
{
    /// <summary>
    /// CreateChartMusicPage.xaml 的交互逻辑
    /// </summary>
    public partial class CreateChartMusicPage : Page
    {
        private CreateChartMusicModel Model;

        private MainWindowModel MainWindowModel;

        public CreateChartMusicPage(MainWindowModel mainWindowModel)
        {
            InitializeComponent();
            this.MainWindowModel = mainWindowModel;
            this.Model = new CreateChartMusicModel();
            this.DataContext = this.Model;
        }

        /// <summary>
        /// 处理返回按钮点击事件
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// 处理选择封面图片按钮点击事件
        /// </summary>
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
                var result = await DialogHost.Show(new WarnDialog("图片读取失败，请换一张图片哦~"), "CreateChartMusicDialog");
            }
        }

        /// <summary>
        /// 选择音频文件的事件处理器
        /// </summary>
        private void SelectMusicButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音频文件 (*.mp3;*.ogg)|*.mp3;*.ogg";
            if (openFileDialog.ShowDialog() == true)
            {
                this.Model.MusicPath = openFileDialog.FileName;
                Console.WriteLine("已选择音频文件");
            }
        }

        /// <summary>
        /// 处理创建音乐逻辑
        /// </summary>
        private async void CreateChartMusicButton_Click(Object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Model.Title))
            {
                var result = await DialogHost.Show(new WarnDialog("曲目名不能为空哦~"), "CreateChartMusicDialog");
                return;
            }
            if (this.MainWindowModel.IsChartMusicExist(this.Model.Title))
            {
                var result = await DialogHost.Show(new WarnDialog("此曲目名已经创建了哦~"), "CreateChartMusicDialog");
                return;
            }
            if (Directory.Exists(Path.Combine(Common.GetChartMusicFolderPath(), this.Model.Title)))
            {
                var result = await DialogHost.Show(new WarnDialog("此曲目名文件夹已经存在了，换个名字吧"), "CreateChartMusicDialog");
                return;
            }
            if (string.IsNullOrWhiteSpace(this.Model.Artist))
            {
                var result = await DialogHost.Show(new WarnDialog("作曲艺术家不能为空哦~"), "CreateChartMusicDialog");
                return;
            }
            if (!double.TryParse(this.Model.Bpm, out double bpm) || bpm < 0)
            {
                await DialogHost.Show(new WarnDialog("BPM要为非负数字"), "CreateChartMusicDialog");
                return;
            }
            if (string.IsNullOrWhiteSpace(this.Model.MusicPath))
            {
                var result = await DialogHost.Show(new WarnDialog("音频还没有选择哦~"), "CreateChartMusicDialog");
                return;
            }
            using(var file = TagLib.File.Create(this.Model.MusicPath))
            {
                double duration = file.Properties.Duration.TotalSeconds;
                if (duration > 3600)
                {
                    var result = await DialogHost.Show(new WarnDialog("不能选择超过一个小时的音频哦~"), "CreateChartMusicDialog");
                    return;
                }
            }

            DialogHost.Show(new LoadingDialog("曲目正在创建中~"), "CreateChartMusicDialog");
            ChartMusic createResult = await this.Model.CreateChartMusicFolder();
            DialogHost.Close("CreateChartMusicDialog");

            if (createResult != null)
            {
                this.MainWindowModel.AddChartMusic(createResult);
                this.NavigationService.Navigate(new ChartListPage(this.MainWindowModel, new ChartListModel(createResult)));
            }
            else
            {
                var result = await DialogHost.Show(new WarnDialog("曲目创建失败，再试试吧~"), "CreateChartMusicDialog");
            }
        }
    }
}
