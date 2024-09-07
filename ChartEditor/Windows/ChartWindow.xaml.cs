using ChartEditor.Models;
using ChartEditor.Pages;
using ChartEditor.ViewModels;
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
        public ChartWindow(ChartInfo chartInfo, MainWindowModel mainWindowModel, ChartListModel chartListModel)
        {
            InitializeComponent();
            this.Title = chartInfo.Name + " - " + chartInfo.ChartMusic.Title;
            ChartEditPage chartEditPage = new ChartEditPage(chartInfo, mainWindowModel, chartListModel);
            ChartPage.Navigate(chartEditPage);
        }
    }
}
