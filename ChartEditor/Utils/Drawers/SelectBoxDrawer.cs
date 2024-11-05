using ChartEditor.UserControls.Boards;
using ChartEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChartEditor.Utils.Drawers
{
    public class SelectBoxDrawer
    {
        private TrackEditBoard TrackEditBoard;

        private ChartEditModel ChartEditModel { get { return TrackEditBoard.Model; } }

        private Canvas TrackCanvas { get { return TrackEditBoard.TrackCanvas; } }

        private Rectangle selectBox;

        public Rectangle SelectBox { get { return selectBox; } }

        private static DoubleAnimation StrokeAnimation = AnimationProvider.GetRepeatDoubleAnimation(8, 0, 1.0);

        private Point? anchorPoint = null;

        public SelectBoxDrawer(TrackEditBoard trackEditBoard)
        {
            this.TrackEditBoard = trackEditBoard;
            this.selectBox = new Rectangle
            {
                Name = "SelectedBox",
                Stroke = Brushes.Black,
                Fill = Brushes.Transparent,
                RadiusX = 5,
                RadiusY = 5,
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection() { 4, 4 },
                Visibility = Visibility.Collapsed
            };
            this.TrackCanvas.Children.Add(this.selectBox);
            Canvas.SetZIndex(this.selectBox, 200);
        }

        /// <summary>
        /// 以一点为锚点设置多选框，如果不能设置，则返回false
        /// </summary>
        public bool SetSelectedBoxAt(Point? point)
        {
            if (!point.HasValue) return false;
            // 如果在轨道或音符的范围内，则退出
            if (this.TrackEditBoard.IsMouseInTrackOrNote(point)) return false;
            this.anchorPoint = point;
            // 启动虚线旋转动画
            this.selectBox.BeginAnimation(Shape.StrokeDashOffsetProperty, StrokeAnimation);
            this.selectBox.Width = 0;
            this.selectBox.Height = 0;
            this.selectBox.Visibility = Visibility.Visible;
            this.TrackCanvas.CaptureMouse();
            return true;
        }

        /// <summary>
        /// 将多选框拖到一点
        /// </summary>
        public void DragSelectBoxTo(Point? point)
        {
            if (!point.HasValue || !this.anchorPoint.HasValue) return;
            
            double anchorX = this.anchorPoint.Value.X;
            double anchorY = this.anchorPoint.Value.Y;
            double pointX = point.Value.X;
            double pointY = point.Value.Y;
            double width = Math.Abs(pointX - anchorX);
            double height = Math.Abs(pointY - anchorY);
            this.selectBox.Width = width;
            this.selectBox.Height = height;
            Canvas.SetLeft(this.selectBox, Math.Min(anchorX, pointX));
            Canvas.SetBottom(this.selectBox, this.TrackCanvas.Height - Math.Max(anchorY, pointY));
        }

        /// <summary>
        /// 隐藏多选框
        /// </summary>
        public void HideSelectBox()
        {
            if (!this.anchorPoint.HasValue) return;
            this.anchorPoint = null;
            this.TrackCanvas.ReleaseMouseCapture();
            // 停止动画
            this.selectBox.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
            this.selectBox.Width = 0;
            this.selectBox.Height = 0;
            Canvas.SetLeft(this.selectBox, -100);
            this.selectBox.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 获取多选框的矩形
        /// </summary>
        public Rect GetRect()
        {
            return new Rect(Canvas.GetLeft(this.selectBox), Canvas.GetBottom(this.selectBox), this.selectBox.Width, this.selectBox.Height);
        }
    }
}
