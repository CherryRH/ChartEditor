using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 动画效果提供器
    /// </summary>
    public class AnimationProvider
    {
        /// <summary>
        /// 对double循环线性插值
        /// </summary>
        public static DoubleAnimation GetRepeatDoubleAnimation(double from, double to, double duration)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                RepeatBehavior = RepeatBehavior.Forever
            };
        }
    }
}
