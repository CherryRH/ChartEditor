using ChartEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Models
{
    /// <summary>
    /// 表示一个节奏点的时间
    /// </summary>
    public class BeatTime
    {
        /// <summary>
        /// 节拍数，默认以四分音符为一拍
        /// </summary>
        private int beat;
        public int Beat { get { return beat; } set { beat = value; } }

        /// <summary>
        /// 每拍分割数
        /// </summary>
        private int divide;
        public int Divide { get { return divide; } set { divide = value; } }

        /// <summary>
        /// 音符在每拍分割中的序号
        /// </summary>
        private int divideIndex;
        public int DivideIndex { get { return divideIndex; } set { divideIndex = value; } }

        public BeatTime()
        {
            this.beat = 0;
            this.divide = 4;
            this.divideIndex = 0;
        }

        public BeatTime(BeatTime beat)
        {
            this.beat = beat.beat;
            this.divide = beat.divide;
            this.divideIndex = beat.divideIndex;
        }

        public BeatTime(int divide)
        {
            this.beat = 0;
            this.divide = divide;
            this.divideIndex = 0;
        }

        public BeatTime(int beat, int divide, int divideIndex)
        {
            this.beat = beat;
            this.divide = divide;
            this.AddDivideIndex(divideIndex);
        }

        public void AddDivideIndex(int divideIndex)
        {
            this.divideIndex += divideIndex;
            this.beat += this.divideIndex / divide;
            this.divideIndex %= divide;
            if (this.divideIndex < 0)
            {
                this.divideIndex += this.divide;
                this.beat--;
            }
        }

        /// <summary>
        /// 转化为节拍字符串
        /// </summary>
        public string ToBeatString()
        {
            return beat + ":" + divideIndex + "/" + divide;
        }

        public void Reset()
        {
            this.beat = 0;
            this.divideIndex = 0;
        }

        /// <summary>
        /// 计算判定线距离
        /// </summary>
        public double GetJudgeLineOffset(double rowWidth)
        {
            return this.GetEquivalentBeat() * rowWidth;
        }

        /// <summary>
        /// 计算等效拍数
        /// </summary>
        public double GetEquivalentBeat()
        {
            return this.beat + (double)this.divideIndex / this.divide;
        }

        /// <summary>
        /// 根据判定线位置更新拍数
        /// </summary>
        public void UpdateFromJudgeLineOffset(double judgeLineOffset, double rowWidth)
        {
            this.beat = (int)(judgeLineOffset / rowWidth);
            this.divideIndex = (int)((judgeLineOffset % rowWidth) / (rowWidth / this.divide));
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        public bool IsEqualTo(BeatTime other)
        {
            return (this.beat == other.beat && this.divideIndex * other.divide == other.divideIndex * this.divide);
        }

        /// <summary>
        /// 是否晚于
        /// </summary>
        public bool IsLaterThan(BeatTime other)
        {
            if (this.beat == other.beat)
            {
                return this.divideIndex * other.divide > other.divideIndex * this.divide;
            }
            else return this.beat > other.beat;
        }

        /// <summary>
        /// 是否早于
        /// </summary>
        public bool IsEarlierThan(BeatTime other)
        {
            if (this.beat == other.beat)
            {
                return this.divideIndex * other.divide < other.divideIndex * this.divide;
            }
            else return this.beat < other.beat;
        }

        /// <summary>
        /// 比较时间先后
        /// </summary>
        public int CompareTo(BeatTime y)
        {
            if (this.IsEarlierThan(y)) return -1;
            else if (this.IsEqualTo(y)) return 0;
            else return 1;
        }

        /// <summary>
        /// 返回与一个拍数的差值
        /// </summary>
        public BeatTime Difference(BeatTime other)
        {
            if (other == null) return new BeatTime();
            // 通分
            int numerator = this.divideIndex * other.Divide - other.divideIndex * this.divide;
            int denominator = this.divide * other.Divide;
            // 约分
            int gcd = MathUtil.GCD(numerator, denominator);
            int divide = denominator / gcd;
            int divideIndex = numerator > 0 ? (numerator / gcd) : (numerator / gcd + divide);
            int beat = numerator > 0 ? (this.beat - other.beat) : (this.beat - other.beat - 1);
            return new BeatTime(beat, divide, divideIndex);
        }

        /// <summary>
        /// 返回与一个拍数的和
        /// </summary>
        public BeatTime Sum(BeatTime other)
        {
            if (other == null) return new BeatTime();
            // 通分
            int numerator = this.divideIndex * other.Divide + other.divideIndex * this.divide;
            int denominator = this.divide * other.Divide;
            // 约分
            int gcd = MathUtil.GCD(numerator, denominator);
            int divide = denominator / gcd;
            int divideIndex = numerator >= denominator ? ((numerator - denominator) / gcd) : (numerator / gcd);
            int beat = numerator >= denominator ? (this.beat + other.beat + 1) : (this.beat + other.beat);
            return new BeatTime(beat, divide, divideIndex);
        }

        /// <summary>
        /// 获取下一个divide的拍数
        /// </summary>
        public BeatTime GetNextDivideBeatTime(int divide)
        {
            if (this.divideIndex == 0) return new BeatTime(this.beat, divide, 1);
            double thisNum = (double)this.divideIndex / this.divide;
            int floor = (int)Math.Floor(thisNum * divide);
            return new BeatTime(this.beat, divide, floor + 1);
        }

        /// <summary>
        /// 获取到下一个divide的距离
        /// </summary>
        public double GetNextDivideDistance(int divide, double rowWidth)
        {
            double thisNum = (double)this.divideIndex / this.divide;
            double floor = Math.Floor(thisNum * divide);
            double nextNum = (floor + 1) / divide;
            return rowWidth * (nextNum - thisNum);
        }

        /// <summary>
        /// 获取divide下的上一个拍数
        /// </summary>
        public BeatTime GetPreDivideBeatTime(int divide)
        {
            if (this.divideIndex == 0) return new BeatTime(this.beat - 1, divide, divide - 1);
            double thisNum = (double)this.divideIndex / this.divide;
            int ceiling = (int)Math.Ceiling(thisNum * divide);
            return new BeatTime(this.beat, divide, ceiling - 1);
        }

        /// <summary>
        /// 获取到上一个divide的距离
        /// </summary>
        public double GetPreDivideDistance(int divide, double rowWidth)
        {
            double thisNum = (double)this.divideIndex / this.divide;
            double ceiling = Math.Ceiling(thisNum * divide);
            double preNum = (ceiling - 1) / divide;
            return rowWidth * (thisNum - preNum);
        }

        /// <summary>
        /// 根据鼠标位移更新拍数
        /// </summary>
        public BeatTime CreateByOffsetY(int divide, double rowWidth, double offsetY)
        {
            // 触发更新的阈值
            double testRate = 0.8;
            double offsetYAbs = Math.Abs(offsetY);
            double divideWidth = rowWidth / divide;
            BeatTime result = null;
            if (offsetY > 0)
            {
                double preDistance = this.GetPreDivideDistance(divide, rowWidth);
                if (offsetYAbs >= testRate * preDistance)
                {
                    result = this.GetPreDivideBeatTime(divide);
                    double restOffsetY = offsetYAbs - preDistance;
                    if (restOffsetY > 0)
                    {
                        int addDivideIndex = (int)Math.Floor(restOffsetY / divideWidth);
                        if (restOffsetY % divideWidth >= testRate * divideWidth) addDivideIndex++;
                        result.AddDivideIndex(-addDivideIndex);
                    }
                }
                else return this;
            }
            else
            {
                double nextDistance = this.GetNextDivideDistance(divide, rowWidth);
                if (offsetYAbs >= testRate * nextDistance)
                {
                    result = this.GetNextDivideBeatTime(divide);
                    double restOffsetY = offsetYAbs - nextDistance;
                    if (restOffsetY > 0)
                    {
                        int addDivideIndex = (int)Math.Floor(restOffsetY / divideWidth);
                        if (restOffsetY % divideWidth >= testRate * divideWidth) addDivideIndex++;
                        result.AddDivideIndex(addDivideIndex);
                    }
                }
                else return this;
            }
            return result;
        }
    }
}
