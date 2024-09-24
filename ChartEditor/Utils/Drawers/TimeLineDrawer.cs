using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChartEditor.Utils.Drawers
{
    /// <summary>
    /// 绘制时间轴
    /// </summary>
    public class TimeLineDrawer : FrameworkElement
    {
        // 当前时间轴
        private DrawingVisual currentTimeLine = null;

        public TimeLineDrawer()
        {

        }

        public void DrawTimeLine(int beatNum, double rowWidth)
        {
            this.RemoveVisualChild(this.currentTimeLine);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                for (int i = 0; i <= beatNum; i++)
                {
                    if (i < beatNum)
                    {
                        // 绘制拍号
                        FormattedText formattedText = new FormattedText(
                            (beatNum - i).ToString(),
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Consolas"),
                            16,
                            Brushes.Gray,
                            VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip
                        );
                        dc.DrawText(formattedText, new Point(0, (i + 0.5) * rowWidth - formattedText.Height / 2));
                    }
                }
            }
            this.currentTimeLine = drawingVisual;
            this.AddVisualChild(drawingVisual);
        }

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException();
            return this.currentTimeLine;
        }
    }
}
