using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ChartEditor.Models;
using ChartEditor.ViewModels;
using Microsoft.VisualBasic.FileIO;
using ChartEditor.Utils;
using System.Windows.Interop;
using System.Windows.Media;

namespace ChartEditor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static string logTag = "[App]";

        protected override void OnStartup(StartupEventArgs e)
        {
            // 使用 GPU 硬件加速
            RenderOptions.ProcessRenderMode = RenderMode.Default;
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
        }
    }
}
