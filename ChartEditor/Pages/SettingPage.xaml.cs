using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ChartEditor.ViewModels;

namespace ChartEditor.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        private MainWindowModel Model;

        public SettingPage(MainWindowModel mainWindowModel)
        {
            InitializeComponent();
            this.DataContext = mainWindowModel;
            this.Model = mainWindowModel;
        }

        /// <summary>
        /// 处理返回按钮点击事件
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // 保存设置文件
            this.Model.Settings.SaveSettings();
            this.NavigationService.GoBack();
        }
    }
}
