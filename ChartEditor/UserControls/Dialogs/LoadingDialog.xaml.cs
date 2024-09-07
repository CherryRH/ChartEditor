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
    /// LoadingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingDialog : UserControl
    {
        /// <summary>
        /// 加载信息内容
        /// </summary>
        private string message;
        public string Message { get { return message; } set { message = value; } }

        public LoadingDialog(string message)
        {
            InitializeComponent();
            this.Message = message;
        }
    }
}
