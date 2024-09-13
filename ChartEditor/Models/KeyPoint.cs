using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Models
{
    /// <summary>
    /// 关键点
    /// </summary>
    public class KeyPoint
    {
        /// <summary>
        /// 时间
        /// </summary>
        private BeatTime time;
        public BeatTime Time { get { return time; } set { time = value; } }

        /// <summary>
        /// 插值类型
        /// </summary>
        private InterpolationType interpolationType;
        public InterpolationType InterpolationType { get { return interpolationType; } set { interpolationType = value; } }

        public KeyPoint() { }

        public KeyPoint(BeatTime time, InterpolationType interpolationType = InterpolationType.None)
        {
            this.time = time;
            this.interpolationType = interpolationType;
        }
    }

    /// <summary>
    /// 坐标关键点
    /// </summary>
    public class PositionKeyPoint : KeyPoint
    {
        /// <summary>
        /// 横坐标
        /// </summary>
        private double x;
        public double X { get { return x; } set { x = value; } }

        /// <summary>
        /// 纵坐标
        /// </summary>
        private double y;
        public double Y { get { return y; } set { y = value; } }

        public PositionKeyPoint(double x, double y, BeatTime time, InterpolationType interpolationType = InterpolationType.None)
            : base(time, interpolationType)
        {
            this.x = x;
            this.y = y;
            
        }
    }

    /// <summary>
    /// 角度关键点
    /// </summary>
    public class AngleKeyPoint : KeyPoint
    {
        /// <summary>
        /// 角度
        /// </summary>
        private double angle;
        public double Angle { get { return angle; } set { angle = value; } }

        public AngleKeyPoint(double angle, BeatTime time, InterpolationType interpolationType = InterpolationType.None)
            : base(time, interpolationType)
        {
            this.angle = angle;
        }
    }

    /// <summary>
    /// 不透明度关键点
    /// </summary>
    public class OpacityKeyPoint : KeyPoint
    {
        /// <summary>
        /// 不透明度
        /// </summary>
        private double opacity;
        public double Opacity { get { return opacity; } set { opacity = value; } }

        public OpacityKeyPoint(double opacity, BeatTime time, InterpolationType interpolationType = InterpolationType.None)
            : base(time, interpolationType)
        {
            this.opacity = opacity;
        }
    }

    /// <summary>
    /// 插值类型
    /// </summary>
    public enum InterpolationType
    {
        None,
        Linear,
        Quadratic,
        Cubic,
        Bezier,
        Rhythm,
        EaseIn,
        EaseOut,
        EaseInOut
    }
}
