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
    /// WarnDialog.xaml 的交互逻辑
    /// </summary>
    public partial class WarnDialog : UserControl
    {
        /// <summary>
        /// 警告信息内容
        /// </summary>
        private string message;
        public string Message { get { return message; } set { message = value; } }

        public WarnDialog(string message)
        {
            InitializeComponent();
            this.Message = message;
        }

        // 关闭按钮点击事件处理程序
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute("Ok", this);
        }
    }
}
