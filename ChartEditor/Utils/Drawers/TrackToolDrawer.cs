using ChartEditor.Models;
using ChartEditor.UserControls.Boards;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChartEditor.Utils.Drawers
{
    /// <summary>
    /// 
    /// </summary>
    public class TrackToolDrawer
    {
        /// <summary>
        /// TrackCanvas对象
        /// </summary>
        private Canvas TrackCanvas;

        /// <summary>
        /// ChartEditModel对象
        /// </summary>
        private ChartEditModel ChartEditModel;

        /// <summary>
        /// 提示轨道连接的工具框
        /// </summary>
        private Grid trackConnectUpTool;
        private Grid trackConnectDownTool;

        private bool isTrackConnectUpToolShowing = false;
        public bool IsTrackConnectUpToolShowing { get { return isTrackConnectUpToolShowing; } }
        private bool isTrackConnectUpToolHighLighting = false;
        public bool IsTrackConnectUpToolHighLighting { get { return isTrackConnectUpToolHighLighting; } }

        private bool isTrackConnectDownToolShowing = false;
        public bool IsTrackConnectDownToolShowing { get { return isTrackConnectDownToolShowing; } }
        private bool isTrackConnectDownToolHighLighting = false;
        public bool IsTrackConnectDownToolHighLighting { get { return isTrackConnectDownToolHighLighting; } }

        // 图形参数
        private static double TrackConnectToolHeight = 100;
        private static double TrackConnectToolInOpacity = 1.0;
        private static double TrackConnectToolOutOpacity = 0.5;

        // 图形ZIndex
        private static int ToolZIndex = 200;

        public TrackToolDrawer(TrackEditBoard trackEditBoard)
        {
            this.TrackCanvas = trackEditBoard.TrackCanvas;
            this.ChartEditModel = trackEditBoard.Model;

            this.InitTools();
        }

        /// <summary>
        /// 显示轨道上连接工具框
        /// </summary>
        public void ShowTrackConnectUpTool(Track track)
        {
            if (this.isTrackConnectUpToolShowing) return;
            // 重置宽度和位置
            this.trackConnectUpTool.Width = this.ChartEditModel.ColumnWidth;
            this.trackConnectUpTool.Visibility = Visibility.Visible;
            Canvas.SetLeft(this.trackConnectUpTool, track.ColumnIndex * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.trackConnectUpTool, Canvas.GetBottom(track.Rectangle));
            this.isTrackConnectUpToolShowing = true;
        }

        /// <summary>
        /// 显示轨道下连接工具框
        /// </summary>
        public void ShowTrackConnectDownTool(Track track)
        {
            if (this.isTrackConnectDownToolShowing) return;
            // 重置宽度和位置
            this.trackConnectDownTool.Width = this.ChartEditModel.ColumnWidth;
            this.trackConnectDownTool.Visibility = Visibility.Visible;
            Canvas.SetLeft(this.trackConnectDownTool, track.ColumnIndex * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.trackConnectDownTool, Canvas.GetBottom(track.Rectangle) + track.Rectangle.Height - TrackConnectToolHeight);
            this.isTrackConnectDownToolShowing = true;
        }

        public void HighLightTrackConnectUpTool()
        {
            this.trackConnectUpTool.Opacity = TrackConnectToolInOpacity;
            this.isTrackConnectUpToolHighLighting = true;
        }

        public void HighLightTrackConnectDownTool()
        {
            this.trackConnectDownTool.Opacity = TrackConnectToolInOpacity;
            this.isTrackConnectDownToolHighLighting = true;
        }

        public void ClearTrackConnectUpToolState()
        {
            this.trackConnectUpTool.Opacity = TrackConnectToolOutOpacity;
            this.isTrackConnectUpToolHighLighting = false;
        }

        public void ClearTrackConnectDownToolState()
        {
            this.trackConnectDownTool.Opacity = TrackConnectToolOutOpacity;
            this.isTrackConnectDownToolHighLighting = false;
        }

        /// <summary>
        /// 隐藏轨道上连接工具框
        /// </summary>
        public void HideTrackConnectUpTool()
        {
            if (!this.isTrackConnectUpToolShowing) return;
            this.ClearTrackConnectUpToolState();
            this.trackConnectUpTool.Visibility = Visibility.Collapsed;
            Canvas.SetRight(this.trackConnectUpTool, -100);
            this.isTrackConnectUpToolShowing = false;
        }

        /// <summary>
        /// 隐藏轨道下连接工具框
        /// </summary>
        public void HideTrackConnectDownTool()
        {
            if (!this.isTrackConnectDownToolShowing) return;
            this.ClearTrackConnectDownToolState();
            this.trackConnectDownTool.Visibility = Visibility.Collapsed;
            Canvas.SetRight(this.trackConnectDownTool, -100);
            this.isTrackConnectDownToolShowing = false;
        }

        /// <summary>
        /// 初始化工具框图形
        /// </summary>
        private void InitTools()
        {
            SolidColorBrush iconBrush = Brushes.Purple;
            PackIcon upPackIcon = new PackIcon
            {
                Kind = PackIconKind.FormatVerticalAlignCenter,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = iconBrush,
                Width = 20,
                Height = 20
            };
            PackIcon downPackIcon = new PackIcon
            {
                Kind = PackIconKind.FormatVerticalAlignCenter,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = iconBrush,
                Width = 20,
                Height = 20
            };
            this.trackConnectUpTool = new Grid
            {
                Name = "TrackConnectUpTool",
                Tag = "Tool",
                Width = this.ChartEditModel.ColumnWidth,
                Height = TrackConnectToolHeight,
                Background = ColorProvider.TrackConnectUpToolGradientBrush,
                Opacity = TrackConnectToolOutOpacity,
                Visibility = Visibility.Collapsed
            };
            this.trackConnectUpTool.Children.Add(upPackIcon);
            this.trackConnectDownTool = new Grid
            {
                Name = "TrackConnectDownTool",
                Tag = "Tool",
                Width = this.ChartEditModel.ColumnWidth,
                Height = TrackConnectToolHeight,
                Background = ColorProvider.TrackConnectDownToolGradientBrush,
                Opacity = TrackConnectToolOutOpacity,
                Visibility = Visibility.Collapsed
            };
            this.trackConnectDownTool.Children.Add(downPackIcon);
            this.TrackCanvas.Children.Add(this.trackConnectUpTool);
            this.TrackCanvas.Children.Add(this.trackConnectDownTool);
            Canvas.SetZIndex(this.trackConnectUpTool, ToolZIndex);
            Canvas.SetZIndex(this.trackConnectDownTool, ToolZIndex);
        }
    }
}
