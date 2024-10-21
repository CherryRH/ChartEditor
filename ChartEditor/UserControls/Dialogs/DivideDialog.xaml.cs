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
    /// DivideDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DivideDialog : UserControl
    {
        private ChartEditModel ChartEditModel;

        public DivideDialog(ChartEditModel chartEditModel)
        {
            InitializeComponent();
            this.ChartEditModel = chartEditModel;
            this.DataContext = chartEditModel;
            if (this.ChartEditModel.Divide == 2) DivideChooseBox.SelectedIndex = 0;
            else if (this.ChartEditModel.Divide == 3) DivideChooseBox.SelectedIndex = 1;
            else if (this.ChartEditModel.Divide == 4) DivideChooseBox.SelectedIndex = 2;
            else if (this.ChartEditModel.Divide == 6) DivideChooseBox.SelectedIndex = 3;
            else if (this.ChartEditModel.Divide == 8) DivideChooseBox.SelectedIndex = 4;
            else if (this.ChartEditModel.Divide == 12) DivideChooseBox.SelectedIndex = 5;
            else if (this.ChartEditModel.Divide == 16) DivideChooseBox.SelectedIndex = 6;
            else if (this.ChartEditModel.Divide == 24) DivideChooseBox.SelectedIndex = 7;
            else if (this.ChartEditModel.Divide == 32) DivideChooseBox.SelectedIndex = 8;
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }

        private void DivideChooseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DivideChooseBox.SelectedIndex == 0 && this.ChartEditModel.Divide != 2) this.ChartEditModel.Divide = 2;
            else if (DivideChooseBox.SelectedIndex == 1 && this.ChartEditModel.Divide != 3) this.ChartEditModel.Divide = 3;
            else if (DivideChooseBox.SelectedIndex == 2 && this.ChartEditModel.Divide != 4) this.ChartEditModel.Divide = 4;
            else if (DivideChooseBox.SelectedIndex == 3 && this.ChartEditModel.Divide != 6) this.ChartEditModel.Divide = 6;
            else if (DivideChooseBox.SelectedIndex == 4 && this.ChartEditModel.Divide != 8) this.ChartEditModel.Divide = 8;
            else if (DivideChooseBox.SelectedIndex == 5 && this.ChartEditModel.Divide != 12) this.ChartEditModel.Divide = 12;
            else if (DivideChooseBox.SelectedIndex == 6 && this.ChartEditModel.Divide != 16) this.ChartEditModel.Divide = 16;
            else if (DivideChooseBox.SelectedIndex == 7 && this.ChartEditModel.Divide != 24) this.ChartEditModel.Divide = 24;
            else if (DivideChooseBox.SelectedIndex == 8 && this.ChartEditModel.Divide != 32) this.ChartEditModel.Divide = 32;
        }
    }
}
