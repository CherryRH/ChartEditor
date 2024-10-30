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
using ChartEditor.Models;
using ChartEditor.ViewModels;

namespace ChartEditor.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        private MainWindowModel Model;
        private Settings Settings { get { return Model.Settings; } }

        public SettingPage(MainWindowModel mainWindowModel)
        {
            InitializeComponent();
            this.DataContext = mainWindowModel;
            this.Model = mainWindowModel;
            // 初始化页面
            this.InitSettingsUI();
        }

        /// <summary>
        /// 处理返回按钮点击事件
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // 保存设置文件
            this.Settings.SaveSettings();
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// 根据settings初始化设置页面
        /// </summary>
        private void InitSettingsUI()
        {
            switch (this.Settings.AutoSaveType)
            {
                case AutoSaveType.Never: AutoSaveSelectBox.SelectedIndex = 0; break;
                case AutoSaveType.OneMinute: AutoSaveSelectBox.SelectedIndex = 1; break;
                case AutoSaveType.FiveMinutes: AutoSaveSelectBox.SelectedIndex = 2; break;
                case AutoSaveType.TenMinutes: AutoSaveSelectBox.SelectedIndex = 3; break;
            }
            TrackOrNotePutWarnEnabledToggleButton.IsChecked = this.Settings.TrackOrNotePutWarnEnabled;
        }

        private void AutoSaveSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (AutoSaveSelectBox.SelectedIndex)
            {
                case 0: this.Settings.AutoSaveType = AutoSaveType.Never; break;
                case 1: this.Settings.AutoSaveType = AutoSaveType.OneMinute; break;
                case 2: this.Settings.AutoSaveType = AutoSaveType.FiveMinutes; break;
                case 3: this.Settings.AutoSaveType = AutoSaveType.TenMinutes; break;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.Settings.TrackOrNotePutWarnEnabled = true;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Settings.TrackOrNotePutWarnEnabled = false;
        }
    }
}
