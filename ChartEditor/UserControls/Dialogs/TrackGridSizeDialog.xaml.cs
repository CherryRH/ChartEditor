using ChartEditor.Utils;
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

namespace ChartEditor.UserControls.Dialogs
{
    /// <summary>
    /// TrackGridSizeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TrackGridSizeDialog : UserControl
    {
        private ChartEditModel chartEditModel;

        public TrackGridSizeDialog(ChartEditModel chartEditModel)
        {
            InitializeComponent();
            this.chartEditModel = chartEditModel;
            this.DataContext = chartEditModel;
            
            ColumnWidthSlider.Minimum = Common.ColumnWidthMin;
            ColumnWidthSlider.Maximum = Common.ColumnWidthMax;
            ColumnWidthSlider.TickFrequency = Common.ColumnWidthTick;
            RowWidthSlider.Minimum = Common.RowWidthMin;
            RowWidthSlider.Maximum = Common.RowWidthMax;
            RowWidthSlider.TickFrequency = Common.RowWidthTick;
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }
    }
}
