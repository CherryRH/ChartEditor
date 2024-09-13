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
            this.divideIndex = divideIndex;
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
            return (this.beat == other.beat && this.divide == other.divide && this.divideIndex == other.divideIndex);
        }

        /// <summary>
        /// 是否晚于
        /// </summary>
        public bool IsLaterThan(BeatTime other)
        {
            return this.GetEquivalentBeat() > other.GetEquivalentBeat();
        }

        /// <summary>
        /// 是否早于
        /// </summary>
        public bool IsEarlierThan(BeatTime other)
        {
            return this.GetEquivalentBeat() < other.GetEquivalentBeat();
        }
    }
}
