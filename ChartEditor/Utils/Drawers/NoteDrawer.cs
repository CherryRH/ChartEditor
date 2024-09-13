using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChartEditor.Utils.Drawers
{
    /// <summary>
    /// 实现Canvas中Note的绘制
    /// </summary>
    public class NoteDrawer
    {
        /// <summary>
        /// TrackCanvas对象
        /// </summary>
        private Canvas trackCanvas;

        /// <summary>
        /// 预览图形Track，Tap，Flick，Hold，Catch，TrackHead，HoldHead
        /// </summary>
        private List<Rectangle> previews;

        /// <summary>
        /// 当前预览图形的拍数和列数
        /// </summary>
        private BeatTime lastPreviewBeatTime = new BeatTime();
        private int lastPreviewColumnIndex = 0;

        /// <summary>
        /// 指定的颜色
        /// </summary>
        public static SolidColorBrush TrackBrush = Brushes.LightGray;
        public static SolidColorBrush TrackBorderBrush = Brushes.Gray;
        public static SolidColorBrush TapNoteBrush = Brushes.Blue;
        public static SolidColorBrush TapNoteBorderBrush = Brushes.DarkBlue;
        public static SolidColorBrush FlickNoteBrush = Brushes.Magenta;
        public static SolidColorBrush FlickNoteBorderBrush = Brushes.DarkMagenta;
        public static SolidColorBrush HoldNoteBrush = Brushes.Green;
        public static SolidColorBrush HoldNoteBorderBrush = Brushes.DarkGreen;
        public static SolidColorBrush CatchNoteBrush = Brushes.Orange;
        public static SolidColorBrush CatchNoteBorderBrush = Brushes.DarkOrange;

        /// <summary>
        /// 所有Track和Note对应的矩形
        /// </summary>
        private List<Rectangle> notes;

        public NoteDrawer(Canvas canvas)
        {
            this.trackCanvas = canvas;
            this.notes = new List<Rectangle>();
            this.previews = new List<Rectangle>();

            this.InitPreviewNotes();
        }

        /// <summary>
        /// 在指定位置显示一个预览图形
        /// </summary>
        public void ShowPreviewNote(int selectedIndex, Point? point, double columnWidth, double rowWidth, double canvasHeight, double canvasViewerHeight, int divide)
        {
            if (!point.HasValue || selectedIndex < 0 || selectedIndex > this.previews.Count) return;
            double canvasBottomBlank = Common.JudgeLineRate * canvasViewerHeight;
            // 判断位置是否相同
            BeatTime newBeatTime = new BeatTime(divide);
            newBeatTime.UpdateFromJudgeLineOffset(canvasHeight - point.Value.Y - canvasBottomBlank, rowWidth);
            int columnIndex = (int)((point.Value.X - Common.BeatBarWidth) / columnWidth);
            if (this.lastPreviewBeatTime.IsEqualTo(newBeatTime) && this.lastPreviewColumnIndex == columnIndex)
            {
                return;
            }
            this.lastPreviewBeatTime = newBeatTime;
            this.lastPreviewColumnIndex = columnIndex;
            // 重置宽度和位置
            this.previews[selectedIndex].Width = columnWidth - (selectedIndex == 0 ? 0 : 2 * Common.ColumnGap * columnWidth);
            Canvas.SetLeft(this.previews[selectedIndex], Common.BeatBarWidth + columnIndex * columnWidth + (selectedIndex == 0 ? 0 : Common.ColumnGap * columnWidth));
            Canvas.SetBottom(this.previews[selectedIndex], newBeatTime.GetJudgeLineOffset(rowWidth) + canvasBottomBlank);
        }

        /// <summary>
        /// 在指定位置显示一个Head图形
        /// </summary>
        public void ShowHeadNote(int selectedIndex, BeatTime beatTime, int columnIndex, double columnWidth, double rowWidth, double canvasViewerHeight)
        {
            if (selectedIndex != 5 || selectedIndex != 6) return;
            double canvasBottomBlank = Common.JudgeLineRate * canvasViewerHeight;
            // 重置宽度和位置
            this.previews[selectedIndex].Width = columnWidth - (selectedIndex == 5 ? 0 : 2 * Common.ColumnGap * columnWidth);
            Canvas.SetLeft(this.previews[selectedIndex], Common.BeatBarWidth + columnIndex * columnWidth + (selectedIndex == 5 ? 0 : Common.ColumnGap * columnWidth));
            Canvas.SetBottom(this.previews[selectedIndex], beatTime.GetJudgeLineOffset(rowWidth) + canvasBottomBlank);
        }

        /// <summary>
        /// 初始化预览图形
        /// </summary>
        private void InitPreviewNotes()
        {
            double width = (1.0 - 2 * Common.ColumnGap) * Common.ColumnWidth;
            double height = 10;
            int strokeThickness = 2;
            int radius = 5;
            double opacity = 0.2;
            // 5种预览图形
            this.previews.Add(new Rectangle
            {
                Name = "TrackPreview",
                Width = Common.ColumnWidth,
                Height = height,
                Stroke = TrackBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = TrackBrush,
                RadiusX = radius,
                RadiusY = radius,
                Opacity = opacity,
                Visibility = System.Windows.Visibility.Collapsed
            });
            this.previews.Add(new Rectangle
            {
                Name = "TapNotePreview",
                Width = width,
                Height = height,
                Stroke = TapNoteBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = TapNoteBrush,
                RadiusX = radius,
                RadiusY = radius,
                Opacity = opacity,
                Visibility = System.Windows.Visibility.Collapsed
            });
            this.previews.Add(new Rectangle
            {
                Name = "FlickNotePreview",
                Width = width,
                Height = height,
                Stroke = FlickNoteBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = FlickNoteBrush,
                RadiusX = radius,
                RadiusY = radius,
                Opacity = opacity,
                Visibility = System.Windows.Visibility.Collapsed
            });
            this.previews.Add(new Rectangle
            {
                Name = "HoldNotePreview",
                Width = width,
                Height = height,
                Stroke = HoldNoteBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = HoldNoteBrush,
                RadiusX = radius,
                RadiusY = radius,
                Opacity = opacity,
                Visibility = System.Windows.Visibility.Collapsed
            });
            this.previews.Add(new Rectangle
            {
                Name = "CatchNotePreview",
                Width = width,
                Height = height,
                Stroke = CatchNoteBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = CatchNoteBrush,
                RadiusX = radius,
                RadiusY = radius,
                Opacity = opacity,
                Visibility = System.Windows.Visibility.Collapsed
            });
            // 2种开头图形
            this.previews.Add(new Rectangle
            {
                Name = "TrackHead",
                Width = Common.ColumnWidth,
                Height = height,
                Stroke = TrackBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = TrackBrush,
                RadiusX = radius,
                RadiusY = radius,
                Visibility = System.Windows.Visibility.Collapsed
            });
            this.previews.Add(new Rectangle
            {
                Name = "HoldNoteHead",
                Width = width,
                Height = height,
                Stroke = HoldNoteBorderBrush,
                StrokeThickness = strokeThickness,
                Fill = HoldNoteBrush,
                RadiusX = radius,
                RadiusY = radius,
                Visibility = System.Windows.Visibility.Collapsed
            });
            this.trackCanvas.Children.Add(this.previews[0]);
            this.trackCanvas.Children.Add(this.previews[1]);
            this.trackCanvas.Children.Add(this.previews[2]);
            this.trackCanvas.Children.Add(this.previews[3]);
            this.trackCanvas.Children.Add(this.previews[4]);
            this.trackCanvas.Children.Add(this.previews[5]);
            this.trackCanvas.Children.Add(this.previews[6]);
        }

        /// <summary>
        /// 隐藏指定预览图形
        /// </summary>
        public void HidePreviewNote(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= this.previews.Count) return;
            this.previews[selectedIndex].Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// 显示指定预览图形
        /// </summary>
        public void ShowPreviewNote(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= this.previews.Count) return;
            for (int i = 0; i < this.previews.Count; i++)
            {
                this.previews[i].Visibility = System.Windows.Visibility.Collapsed;
            }
            // 先放置在范围外
            Canvas.SetLeft(this.previews[selectedIndex], -1000);
            this.previews[selectedIndex].Visibility = System.Windows.Visibility.Visible;
        }
    }
}
