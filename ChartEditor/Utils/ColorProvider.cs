using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 提供绘制颜色
    /// </summary>
    public class ColorProvider
    {
        /// <summary>
        /// 消息框字体颜色
        /// </summary>
        public static SolidColorBrush NoticeMessageBrush = Brushes.White;
        public static SolidColorBrush WarnMessageBrush = Brushes.Yellow;
        public static SolidColorBrush ErrorMessageBrush = Brushes.Red;

        /// <summary>
        /// Track以及Note的填充颜色和边界颜色
        /// </summary>
        public static SolidColorBrush TrackBrush = Brushes.LightGray;
        public static SolidColorBrush TrackBorderBrush = Brushes.Gray;
        public static SolidColorBrush TapNoteBrush = Brushes.LightBlue;
        public static SolidColorBrush TapNoteBorderBrush = Brushes.Blue;
        public static SolidColorBrush FlickNoteBrush = Brushes.Pink;
        public static SolidColorBrush FlickNoteBorderBrush = Brushes.Magenta;
        public static SolidColorBrush HoldNoteBrush = Brushes.LightGreen;
        public static SolidColorBrush HoldNoteBorderBrush = Brushes.Green;
        public static SolidColorBrush CatchNoteBrush = Brushes.Yellow;
        public static SolidColorBrush CatchNoteBorderBrush = Brushes.Orange;

        /// <summary>
        /// Track渐变颜色
        /// </summary>
        public static LinearGradientBrush TrackGradientBrush
        {
            get
            {
                LinearGradientBrush gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 0.5);
                gradientBrush.EndPoint = new Point(1, 0.5);
                gradientBrush.GradientStops.Add(new GradientStop(TrackBrush.Color, 0));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.5));
                gradientBrush.GradientStops.Add(new GradientStop(TrackBrush.Color, 1));
                return gradientBrush;
            }
        }

        /// <summary>
        /// HoldNote渐变颜色
        /// </summary>
        public static LinearGradientBrush HoldNoteGradientBrush
        {
            get
            {
                LinearGradientBrush gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0.5, 1);
                gradientBrush.EndPoint = new Point(0.5, 0);
                gradientBrush.GradientStops.Add(new GradientStop(HoldNoteBrush.Color, 0));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
                return gradientBrush;
            }
        }

        /// <summary>
        /// FlickNote渐变颜色
        /// </summary>
        public static LinearGradientBrush FlickNoteGradientBrush
        {
            get
            {
                LinearGradientBrush gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 0.5);
                gradientBrush.EndPoint = new Point(1, 0.5);
                gradientBrush.GradientStops.Add(new GradientStop(FlickNoteBrush.Color, 0));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.5));
                gradientBrush.GradientStops.Add(new GradientStop(FlickNoteBrush.Color, 1));
                return gradientBrush;
            }
        }
    }
}
