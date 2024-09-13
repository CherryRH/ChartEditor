using ChartEditor.Models;
using ChartEditor.Pages;
using ChartEditor.UserControls.Boards;
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
        private ChartEditPage ChartEditPage;

        public ChartWindow(ChartInfo chartInfo, MainWindowModel mainWindowModel, ChartListModel chartListModel)
        {
            InitializeComponent();
            this.Title = chartInfo.Name + " - " + chartInfo.ChartMusic.Title;
            ChartEditPage chartEditPage = new ChartEditPage(chartInfo, mainWindowModel, chartListModel);
            this.ChartEditPage = chartEditPage;
            ChartPage.Navigate(chartEditPage);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.ChartEditPage.TrackEditBoard.Dispose();
        }
    }
}
