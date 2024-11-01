﻿using ChartEditor.ViewModels;
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
    /// SpeedDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PlaySpeedDialog : UserControl
    {
        private ChartEditModel chartEditModel;

        public PlaySpeedDialog(ChartEditModel chartEditModel)
        {
            InitializeComponent();
            this.chartEditModel = chartEditModel;
            this.DataContext = chartEditModel;
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }
    }
}
