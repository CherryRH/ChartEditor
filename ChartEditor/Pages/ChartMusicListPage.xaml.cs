using ChartEditor.UserControls.Dialogs;
using ChartEditor.UserControls.Items;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChartEditor.Pages
{
    /// <summary>
    /// ChartMusicListPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChartMusicListPage : Page
    {
        private MainWindowModel Model;

        public ChartMusicListPage(MainWindowModel model)
        {
            InitializeComponent();
            this.DataContext = model;
            this.Model = model;
        }

        /// <summary>
        /// 处理创建新歌曲按钮点击事件
        /// </summary>
        private void CreateChartMusicButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CreateChartMusicPage(this.Model));
        }

        /// <summary>
        /// 处理设置按钮点击事件
        /// </summary>
        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingPage(this.Model));
        }

        /// <summary>
        /// 处理歌曲点击事件
        /// </summary>
        private void ChartMusicItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button item && item.DataContext is ChartMusicItemModel selectedItem)
            {
                this.NavigationService.Navigate(new ChartListPage(this.Model, new ChartListModel(selectedItem.ChartMusic)));
            }
        }

        /// <summary>
        /// 处理歌曲修改点击事件
        /// </summary>
        private void ChartMusicSettingButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 处理歌曲删除点击事件
        /// </summary>
        private async void ChartMusicDeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            bool result = (bool)await DialogHost.Show(new ConfirmDialog("确认要删除歌曲吗？删除后歌曲和谱面还可以在回收站中恢复。"), "ChartMusicListDialog");
            if (result)
            {
                if (sender is Button item && item.DataContext is ChartMusicItemModel selectedItem)
                {
                    this.Model.DeleteChartMusic(selectedItem.ChartMusic);
                }
            }
        }
    }
}
