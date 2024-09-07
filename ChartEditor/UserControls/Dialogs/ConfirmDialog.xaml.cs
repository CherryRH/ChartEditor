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
    /// ConfirmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : UserControl
    {
        /// <summary>
        /// 确认信息内容
        /// </summary>
        private string message;
        public string Message { get { return message; } set { message = value; } }

        public ConfirmDialog(string message)
        {
            InitializeComponent();
            this.message = message;
        }

        /// <summary>
        /// 处理取消按钮点击事件
        /// </summary>
        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }

        /// <summary>
        /// 处理确认按钮点击事件
        /// </summary>
        public void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}
