using ChartEditor.Models;
using ChartEditor.ViewModels;
using ChartEditor.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using ChartEditor.Utils;

namespace ChartEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.SetFolders();

            MainWindowModel mainWindowModel = new MainWindowModel();
            ChartMusicListPage chartMusicListPage = new ChartMusicListPage(mainWindowModel);
            mainWindowModel.ChartMusicListPage = chartMusicListPage;
            MainPage.Navigate(chartMusicListPage);
        }

        /// <summary>
        /// 窗口启动时设置文件夹
        /// </summary>
        private void SetFolders()
        {
            Common.SetBasicFolders();
        }
    }
}
